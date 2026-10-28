"""Validated logical snapshot of business docs only, never users/config/credentials."""
import argparse
from datetime import date
from decimal import Decimal
import hashlib
import os
from pathlib import Path
import re
import sys
import urllib.request
import urllib.error
import urllib.parse
from couch import Couch, CouchError
import json_exact as codec

TYPES=['customer','product','order','delivery']
FORMAT='qlbh-business-v1'

class ExactCouch(Couch):
    def send(self,path='',method='GET',data=None,allowed=()):
        req=urllib.request.Request(self.url+path,method=method,
            data=None if data is None else codec.dumps(data).encode('utf-8'),headers={'Authorization':self.auth,'Content-Type':'application/json'})
        try:
            with urllib.request.urlopen(req,timeout=30) as r: return codec.loads(r.read().decode('utf-8'))
        except urllib.error.HTTPError as e:
            if e.code in allowed: return None
            raise CouchError(e.code) from None
        except (urllib.error.URLError,TimeoutError): raise CouchError(0) from None

def require(condition,message):
    if not condition: raise ValueError(message)

def safe_values(value):
    if isinstance(value,dict):
        for key,item in value.items():
            require(not re.search('password|credential|authorization|cookie|salt|secret|token',key,re.I),'Tệp chứa trường nhạy cảm; từ chối')
            safe_values(item)
    elif isinstance(value,list):
        for item in value: safe_values(item)
    elif isinstance(value,str):
        secret=os.environ.get('COUCHDB_PASSWORD','')
        require(not secret or secret not in value,'Tệp chứa giá trị xác thực; từ chối')

def normalized(doc):
    result={k:v for k,v in doc.items() if k not in ['_rev','syncToken']}
    if result.get('pendingDelivery') is None: result.pop('pendingDelivery',None)
    return result

def number(value):
    require(isinstance(value,(int,Decimal)) and not isinstance(value,bool),'Trường số không hợp lệ')
    result=Decimal(value)
    require(result.is_finite() and result>=0,'Giá trị số phải không âm, hữu hạn')
    require(result<=Decimal('999999999999.99') and result==result.quantize(Decimal('0.01')),'Số tiền tối đa 999999999999.99 và 2 số thập phân')
    return result

def iso(value):
    require(isinstance(value,str) and re.fullmatch(r'\d{4}-\d{2}-\d{2}',value),'Ngày phải là yyyy-MM-dd')
    try: return date.fromisoformat(value)
    except ValueError: raise ValueError('Ngày không hợp lệ') from None

def validate_document(doc):
    require(isinstance(doc,dict) and doc.get('type') in TYPES,'Chỉ chấp nhận bốn loại dữ liệu nghiệp vụ')
    id=doc.get('_id')
    require(isinstance(id,str) and bool(id.strip()) and len(id)<=200 and not id.startswith('_') and id not in ['.','..'],'ID không hợp lệ')
    require(not any(k.startswith('_') and k!='_id' for k in doc),'Không chấp nhận revision, attachment hoặc trường hệ thống trong snapshot')
    require(not doc.get('pendingDelivery'),'Còn đồng bộ chờ; hoàn tất trước khi sao lưu/nhập')
    safe_values(doc)
    kind=doc['type']
    if kind in ['customer','product']: require(isinstance(doc.get('name'),str) and doc['name'].strip(),'Thiếu tên')
    if kind=='customer' and doc.get('email'): require(re.fullmatch(r'[^\s@]+@[^\s@]+\.[^\s@]+',doc['email']),'Email không hợp lệ')
    if kind=='product':
        number(doc.get('price'));q=doc.get('quantity');require(isinstance(q,int) and not isinstance(q,bool) and 0<=q<=2147483647,'Số lượng tồn không hợp lệ')
    if kind=='order':
        iso(doc.get('orderDate'))
        require(doc.get('status') in ['processing','confirmed','preparing','shipping','delivered','cancelled'],'Trạng thái đơn sai')
        require(isinstance(doc.get('items'),list) and 0<len(doc['items'])<=1000,'Đơn thiếu sản phẩm/quá nhiều dòng')
        total=Decimal(0)
        for item in doc['items']:
            require(isinstance(item,dict),'Dòng sản phẩm sai')
            qty=item.get('quantity');require(isinstance(qty,int) and not isinstance(qty,bool) and 0<qty<=2147483647,'Số lượng phải dương và trong giới hạn int')
            total+=qty*number(item.get('price'))
            require(isinstance(item.get('productId'),str) and item['productId'],'Thiếu mã sản phẩm')
        require(number(doc.get('total'))==total,'Tổng tiền không khớp')
        require(isinstance(doc.get('customerId'),str) and doc['customerId'],'Thiếu mã khách')
    if kind=='delivery':
        require(doc.get('status') in ['pending','assigned','shipping','delivered','failed','cancelled'],'Trạng thái giao sai')
        require(isinstance(doc.get('orderId'),str) and doc['orderId'],'Thiếu mã đơn')
        for field in ['pickupDate','expectedDate']:
            if doc.get(field): iso(doc[field])
        if doc.get('pickupDate') and doc.get('expectedDate'): require(iso(doc['expectedDate'])>=iso(doc['pickupDate']),'Ngày giao trước ngày lấy')

def fingerprint(docs):
    return hashlib.sha256(codec.dumps(sorted([normalized(d) for d in docs],key=lambda d:d['_id'])).encode('utf-8')).hexdigest()

def snapshot(c):
    start=c.send()['update_seq']
    docs=[]
    for type in TYPES: docs.extend(normalized(d) for d in c.all_type(type))
    for d in docs: validate_document(d)
    indexes=[]
    for idx in c.send('/_index')['indexes']:
        if idx['type']=='json':
            indexes.append(dict(name=idx['name'],ddoc=idx['ddoc'].removeprefix('_design/'),index=idx['def'],type='json'))
    require(start==c.send()['update_seq'],'DB thay đổi trong lúc export; dừng ghi rồi thử lại')
    archive=dict(format=FORMAT,documents=sorted(docs,key=lambda d:d['_id']),indexes=indexes,sha256=fingerprint(docs))
    validate_archive(archive,c)
    return archive

def validate_archive(data,destination=None):
    require(isinstance(data,dict) and data.get('format')==FORMAT,'Định dạng snapshot không hỗ trợ')
    docs=data.get('documents');require(isinstance(docs,list) and len(docs)<=10000,'Danh sách document không hợp lệ/quá lớn')
    by_id={}
    for doc in docs:
        validate_document(doc);require(doc['_id'] not in by_id,'ID trùng trong tệp');by_id[doc['_id']]=doc
    require(data.get('sha256')==fingerprint(docs),'Checksum nội dung không khớp')
    def reference(id,type):
        require(isinstance(id,str) and bool(id),'Thiếu tham chiếu')
        linked=by_id.get(id)
        if linked is None and destination is not None:
            try: linked=destination.read(id)
            except CouchError as e:
                if e.status!=404: raise
        require(linked is not None and linked.get('type')==type,'Tham chiếu không tồn tại/khác loại')
        return linked
    for doc in docs:
        if doc['type']=='order':
            reference(doc['customerId'],'customer')
            for item in doc['items']: reference(item['productId'],'product')
            if doc.get('deliveryId'):
                delivery=reference(doc['deliveryId'],'delivery');require(delivery['orderId']==doc['_id'],'Liên kết Order/Delivery không khớp')
                expected='shipping' if delivery['status']=='shipping' else 'delivered' if delivery['status']=='delivered' else 'preparing'
                require(doc['status']==expected,'Trạng thái Order/Delivery không khớp')
        if doc['type']=='delivery': reference(doc['orderId'],'order')
    indexes=data.get('indexes');require(isinstance(indexes,list) and len(indexes)<=100,'Index không hợp lệ')
    for idx in indexes:
        require(isinstance(idx,dict) and idx.get('type')=='json' and re.fullmatch('[a-z][a-z0-9-]{0,80}',idx.get('name','')) and idx.get('ddoc')=='idx-'+idx['name'],'Tên index/design doc không hợp lệ')
        fields=idx.get('index',{}).get('fields');require(isinstance(fields,list) and 0<len(fields)<=20,'Trường index không hợp lệ')
        for field in fields:
            require((isinstance(field,str) and len(field)<=100) or (isinstance(field,dict) and len(field)==1 and next(iter(field.values())) in ['asc','desc']),'Chiều index không hợp lệ')
    safe_values(data)
    return docs

def export_file(c,path):
    path=Path(path)
    if path.exists(): raise FileExistsError('Tệp export đã tồn tại')
    archive=snapshot(c)
    path.parent.mkdir(parents=True,exist_ok=True)
    with path.open('x',encoding='utf-8') as f: f.write(codec.dumps(archive)+'\n')
    print('PASS export documents='+str(len(archive['documents']))+' JSON indexes='+str(len(archive['indexes'])))
    return archive

def read_file(path):
    path=Path(path);require(path.stat().st_size<=10*1024*1024,'Tệp vượt 10 MiB')
    return codec.loads(path.read_text(encoding='utf-8-sig'))

def restore(c,archive):
    require(re.fullmatch(r'qlbanhang_(restore|test)_[a-z0-9_]+',c.database),'Chỉ import/restore vào DB thử riêng có tiền tố qlbanhang_restore_ hoặc qlbanhang_test_')
    # Validate entirely before any database creation or document write.
    docs=validate_archive(archive)
    exists=c.send('',allowed=(404,)) is not None
    missing=[];skipped=0
    for doc in docs:
        existing=None
        if exists:
            try: existing=c.read(doc['_id'])
            except CouchError as e:
                if e.status!=404: raise
        if existing is None: missing.append(doc)
        else:
            require(normalized(existing)==doc,'Trùng ID nhưng nội dung khác; dừng trước khi ghi, không overwrite')
            skipped+=1
    if exists:
        current_indexes={(i['ddoc'].removeprefix('_design/'),i['name']):i['def'] for i in c.send('/_index')['indexes'] if i['type']=='json'}
        for idx in archive['indexes']:
            key=(idx['ddoc'],idx['name'])
            require(key not in current_indexes or current_indexes[key]==idx['index'],'Index đã có nhưng định nghĩa khác; dừng trước khi ghi')
    c.send('','PUT',allowed=(412,))
    for idx in archive['indexes']: c.send('/_index','POST',idx)
    inserted=0
    for doc in missing:
        require(c.create(doc) is not None,'Xung đột trong lúc import; các tài liệu đã ghi được giữ lại, có thể chạy lại sau đối chiếu')
        inserted+=1
    print('PASS import inserted='+str(inserted)+' identical-skipped='+str(skipped))
    return inserted,skipped

def verify(c,archive):
    current=snapshot(c)
    require(current['sha256']==archive['sha256'] and len(current['documents'])==len(archive['documents']),'Đếm/nội dung phục hồi không khớp')
    actual={(i['ddoc'],i['name']):i['index'] for i in current['indexes']}
    require(all(actual.get((i['ddoc'],i['name']))==i['index'] for i in archive['indexes']),'Thiếu/sai định nghĩa index sau phục hồi')
    print('PASS verify count='+str(len(current['documents']))+' canonical JSON SHA256 match; indexes present')

if __name__=='__main__':
    parser=argparse.ArgumentParser(description='Logical business snapshot; excludes app users, revision history and server config')
    parser.add_argument('action',choices=['export','import','verify']);parser.add_argument('file');parser.add_argument('--target')
    args=parser.parse_args()
    try:
        c=ExactCouch(args.target)
        if args.action=='export': export_file(c,args.file)
        elif args.action=='import': restore(c,read_file(args.file))
        else: verify(c,read_file(args.file))
    except Exception as e:
        print('FAIL backup/import: '+(str(e) if isinstance(e,(ValueError,CouchError)) else type(e).__name__))
        sys.exit(1)
