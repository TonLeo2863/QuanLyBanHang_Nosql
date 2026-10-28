"""Real logical export/restore; primary DB read-only, restores retained separately."""
import copy
import importlib.util
from pathlib import Path
import secrets
import sys
from decimal import Decimal
root=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(root/'scripts'))
spec=importlib.util.spec_from_file_location('backup',root/'scripts/Business-Backup.py')
b=importlib.util.module_from_spec(spec);spec.loader.exec_module(b)

def rejected(action,label):
    try: action();raise AssertionError('Accepted '+label)
    except ValueError: print('PASS rejected '+label)

def run():
    source=b.ExactCouch()
    target_name='qlbanhang_restore_'+secrets.token_hex(8)
    path=root/'artifacts/backups'/('demo-'+secrets.token_hex(8)+'.local.json')
    archive=b.export_file(source,path)
    loaded=b.read_file(path)
    assert loaded==archive
    assert all(d['type'] in b.TYPES and '_rev' not in d for d in loaded['documents'])
    target=b.ExactCouch(target_name)
    inserted,skipped=b.restore(target,loaded)
    assert inserted==len(loaded['documents']) and skipped==0
    b.verify(target,loaded)
    revisions={d['_id']:target.read(d['_id'])['_rev'] for d in loaded['documents']}
    assert b.restore(target,loaded)==(0,len(loaded['documents']))
    assert all(target.read(id)['_rev']==rev for id,rev in revisions.items())
    b.verify(target,loaded)
    print('PASS repeat import preserves all target revisions')
    rejected(lambda:b.restore(source,loaded),'restore to primary database')
    try: b.export_file(source,path);raise AssertionError('export overwrote file')
    except FileExistsError: print('PASS export refuses existing file')
    invalid=copy.deepcopy(loaded);invalid['documents'].append(copy.deepcopy(invalid['documents'][0]));invalid['sha256']=b.fingerprint(invalid['documents'])
    rejected(lambda:b.validate_archive(invalid),'duplicate document IDs')
    invalid=copy.deepcopy(loaded);invalid['sha256']='0'*64
    rejected(lambda:b.validate_archive(invalid),'bad checksum')
    for mutation,label in [
        (lambda d:d.update(type='app_user'),'app_user import'),
        (lambda d:d.update(password='fake-forbidden'),'sensitive field'),
        (lambda d:d.update(_rev='1-forbidden'),'supplied revision')]:
        invalid=copy.deepcopy(loaded);mutation(invalid['documents'][0]);invalid['sha256']=b.fingerprint(invalid['documents'])
        rejected(lambda:b.validate_archive(invalid),label)
    for field,value,label in [('total',-1,'negative total'),('customerId','missing-reference','missing customer')]:
        invalid=copy.deepcopy(loaded);order=next(d for d in invalid['documents'] if d['type']=='order');order[field]=value;invalid['sha256']=b.fingerprint(invalid['documents'])
        invalid_target=b.ExactCouch('qlbanhang_restore_invalid_'+secrets.token_hex(8))
        rejected(lambda:b.restore(invalid_target,invalid),label)
        assert invalid_target.send('',allowed=(404,)) is None
    rejected(lambda:b.codec.loads('{"x":1,"x":2}'),'duplicate JSON keys')
    rejected(lambda:b.codec.loads('{"x":NaN}'),'nonfinite JSON')
    # Conflicting target: fail preflight without overwriting or changing revisions.
    conflict=b.ExactCouch('qlbanhang_restore_conflict_'+secrets.token_hex(8));conflict.send('','PUT')
    conflict.send('/_index','POST',dict(index=dict(fields=['type','_id']),ddoc='idx-type-id',name='type-id',type='json'))
    doc=copy.deepcopy(loaded['documents'][0]);doc['customConflict']='test-only';conflict.create(doc)
    rev=conflict.read(doc['_id'])['_rev']
    rejected(lambda:b.restore(conflict,loaded),'different content with same ID')
    assert conflict.read(doc['_id'])['_rev']==rev and sum(len(conflict.all_type(t)) for t in b.TYPES)==1
    bad_index=b.ExactCouch('qlbanhang_restore_index_'+secrets.token_hex(8));bad_index.send('','PUT')
    bad_index.send('/_index','POST',dict(index=dict(fields=['type','name']),ddoc='idx-type-id',name='type-id',type='json'))
    rejected(lambda:b.restore(bad_index,loaded),'same index name with different fields')
    assert bad_index.find({'type':{'$in':b.TYPES}},limit=1)['docs']==[]
    # Decimal precision survives CouchDB and the archive without float conversion.
    decimal_source=b.ExactCouch('qlbanhang_test_decimal_'+secrets.token_hex(8));decimal_source.send('','PUT')
    decimal_source.send('/_index','POST',dict(index=dict(fields=['type','_id']),ddoc='idx-type-id',name='type-id',type='json'))
    unsupported=Decimal('70318705647382105639.70318705')
    assert b.codec.loads(b.codec.dumps(dict(price=unsupported)))['price']==unsupported
    rejected(lambda:b.validate_document(dict(_id='unsupported',type='product',name='Fake',price=unsupported,quantity=1)),'money beyond documented precision contract')
    precise=Decimal('7031870564.27')
    decimal_source.create(dict(_id='decimal-product',type='product',name='Fake precision product',price=precise,quantity=1))
    decimal_source.create(dict(_id='app-user-excluded',type='app_user',passwordHash='fake-not-a-real-hash',enabled=False))
    exact=b.snapshot(decimal_source);assert exact['documents'][0]['price']==precise
    assert len(exact['documents'])==1
    decimal_target=b.ExactCouch('qlbanhang_restore_decimal_'+secrets.token_hex(8));b.restore(decimal_target,exact);b.verify(decimal_target,exact)
    assert decimal_target.read('decimal-product')['price']==precise
    print('PASS exact decimal restore without binary-float conversion')
    decimal_source.create(dict(_id='pending-customer',type='customer',name='Fake'))
    decimal_source.create(dict(_id='pending-order',type='order',customerId='pending-customer',orderDate='2026-10-28',status='preparing',
        items=[dict(productId='decimal-product',quantity=1,price=precise)],total=precise,pendingDelivery=dict(target='test-only')))
    rejected(lambda:b.snapshot(decimal_source),'export while outbox is pending')
    print('PASS app users excluded; pending sync cannot produce misleading snapshot')
    print('PASS P5 snapshot count='+str(len(loaded['documents']))+' restored='+target_name+'; primary unchanged; test DBs retained')

if __name__=='__main__':
    try: run()
    except Exception as e:
        print('FAIL backup test: '+(str(e) if isinstance(e,(AssertionError,ValueError,b.CouchError)) else type(e).__name__))
        sys.exit(1)
