import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parents[1]/'scripts'))
from couch import Couch, CouchError
from importlib import util
spec = util.spec_from_file_location('seed', Path(__file__).resolve().parents[1]/'scripts/Seed-Demo.py')
seed = util.module_from_spec(spec); spec.loader.exec_module(seed)

def run():
    c = Couch()
    seed.seed(c)
    before = {d['_id']:c.read(d['_id']) for d in seed.dataset()}
    seed.seed(c)
    assert all(c.read(id)==d for id,d in before.items())
    docs = list(before.values()); by_id = {d['_id']:d for d in docs}
    assert [sum(d['type']==t for d in docs) for t in ['customer','product','order','delivery']] == [20,30,30,20]
    for d in docs:
        if d['type']=='order':
            assert by_id[d['customerId']]['type']=='customer' and len(d['items'])>=2
            assert all(by_id[x['productId']]['type']=='product' and x['quantity']>0 and x['price']>=0 for x in d['items'])
            assert d['total']==sum(x['quantity']*x['price'] for x in d['items'])
        if d['type']=='delivery': assert by_id[d['orderId']]['type']=='order'
    page = c.find({'type':'order','orderDate':{'$gte':'2026-10-01'}}, sort=[{'type':'asc'},{'orderDate':'asc'},{'_id':'asc'}], use_index='idx-type-date', limit=5)
    assert len(page['docs'])==5
    plan = c.send('/_explain','POST',dict(selector={'type':'order','orderDate':{'$gte':'2026-10-01'}},sort=[{'type':'asc'},{'orderDate':'asc'},{'_id':'asc'}],use_index='idx-type-date'))
    assert plan['index']['name']=='type-date'
    print('PASS P2: 20/30/30/20, references, totals, repeat seed unchanged, real Mango sort and _explain')

if __name__=='__main__':
    try: run()
    except (Exception,) as e:
        print('FAIL P2: '+type(e).__name__); sys.exit(1)
