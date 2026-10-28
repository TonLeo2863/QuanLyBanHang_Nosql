"""End-to-end tests. Random app credentials stay in memory; isolated DB retained."""
import html
import http.cookiejar
import importlib.util
import json
import os
from pathlib import Path
import re
import secrets
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
root=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(root/'scripts'))
from couch import Couch, CouchError
def module(file,name):
    spec=importlib.util.spec_from_file_location(name,root/file); m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
users=module('scripts/Create-AppUser.py','users')
seed=module('scripts/Seed-Demo.py','seed')
base=os.environ.get('TEST_WEB_URL','http://localhost:52517')
stage='setup'

def wait_ready():
    deadline=time.monotonic()+60
    while True:
        try:
            with urllib.request.urlopen(base+'/Account/Login',timeout=5) as response:
                assert response.status==200, 'IIS readiness status'
            print('PASS IIS ready')
            return
        except urllib.error.HTTPError:
            raise # A running app returning an error must not be hidden by retries.
        except (urllib.error.URLError,TimeoutError,ConnectionError):
            if time.monotonic()>=deadline: raise
            time.sleep(0.5)

class Browser:
    def __init__(self): self.client=urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
    def get(self,path,fields=None):
        data=None if fields is None else urllib.parse.urlencode(fields).encode()
        with self.client.open(base+urllib.parse.quote(path,safe='/?&=%:'),data,timeout=90) as r:
            return html.unescape(r.read().decode('utf-8-sig')),urllib.parse.urlsplit(r.url).path
    def token(self,path): return re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"',self.get(path)[0]).group(1)
    def post(self,path,fields,token_path=None): return self.get(path,dict(fields,__RequestVerificationToken=self.token(token_path or path)))
    def login(self,name,password):
        body,path=self.post('/Account/Login',dict(Username=name,Password=password))
        assert path=='/Home' or path=='/', 'login failed'
    def denied(self,path,fields=None):
        try: self.get(path,fields); raise AssertionError('authorized unexpectedly')
        except urllib.error.HTTPError as e: assert e.code==403

def flat(doc):
    result={k:str(v) for k,v in doc.items() if v is not None and k not in ['items','history','pendingDelivery','deliveryId']}
    for i,item in enumerate(doc.get('items',[])):
        for k,v in item.items(): result[f'items[{i}].{k}']=str(v)
    return result

def save(browser,entity,doc,editing=False,valid=True):
    action='Edit' if editing else 'Create'
    path='/'+entity+'/'+action
    body,target=browser.post(path,flat(doc),path+('?id='+urllib.parse.quote(doc['_id']) if editing else ''))
    if valid: assert 'validation-summary-errors' not in body and target=='/'+entity, entity+' '+action
    else: assert 'validation-summary-errors' in body,entity+' expected rejection'
    return body

def verify_dashboard(browser,c):
    counts=[len(c.all_type(t)) for t in ['customer','product','order','delivery']]
    shipping=[d for d in c.all_type('delivery') if d['status']=='shipping']
    body,_=browser.get('/Home')
    actual=[int(n) for n in re.findall(r'class="(?:home-number|delivery-number)"\s*>\s*(\d+)',body)]
    assert actual==counts+[len(shipping)], 'Dashboard counts differ from CouchDB'
    body,_=browser.get('/Shipping')
    assert int(re.search(r'<strong>(\d+)</strong>',body).group(1))==len(shipping)
    assert all(d['_id'] in body for d in shipping)
    print('PASS Dashboard counts and Shipping IDs match real CouchDB')

def run():
    global stage
    c=Couch(); assert c.database.startswith('qlbanhang_test_')
    print('Test database: '+c.database)
    seed.seed(c)
    wait_ready()
    credentials={name:secrets.token_urlsafe(24) for name in ['qaadmin','qamanager','qashipper','qaother']}
    for name,role in [('qaadmin','Administrator'),('qamanager','OrderManager'),('qashipper','Shipper'),('qaother','Shipper')]:
        assert c.create(users.user_document(name,credentials[name],role))
    anon=Browser()
    assert anon.get('/Customer')[1]=='/Account/Login'
    assert anon.get('/Customer/Create',{'_id':'unauthorized','name':'Fake'})[1]=='/Account/Login'
    for name in credentials:
        # passwords and hashes must not appear in login or list HTML.
        assert credentials[name] not in anon.get('/Account/Login')[0]
    admin=Browser();admin.login('qaadmin',credentials['qaadmin'])
    manager=Browser();manager.login('qamanager',credentials['qamanager'])
    shipper=Browser();shipper.login('qashipper',credentials['qashipper'])
    other=Browser();other.login('qaother',credentials['qaother'])
    manager.denied('/Account/Users');shipper.denied('/Customer');shipper.denied('/Order/Create');shipper.denied('/Delivery/Create')
    shipper.denied('/Customer/Create',dict(_id='qa-forbidden',name='Fake'))
    manager.denied('/Account/Disable',dict(id='app-user-qaadmin',rev='1-fake'))
    stage='users'
    extra=secrets.token_urlsafe(24)
    body,path=admin.post('/Account/Users',dict(Username='qacreated',Password=extra,Role='OrderManager'))
    assert path=='/Account/Users'
    account=c.read('app-user-qacreated');assert account['passwordHash'] not in body and extra not in body
    new=Browser();new.login('qacreated',extra)
    print('PASS login/logout prerequisites, PBKDF2 interoperability, 3 roles, anonymous and forbidden routes, admin create user')
    admin.post('/Account/Disable',dict(id=account['_id'],rev=account['_rev']),'/Account/Users')
    new.denied('/Home')
    print('PASS disabled account loses access on next request')
    verify_dashboard(manager,c)
    stage='customer product CRUD'
    customer=dict(_id='qa-customer',name='Khách mô phỏng',email='qa@example.invalid',address='Địa chỉ giả')
    product=dict(_id='qa-product',name='Sản phẩm giả',price=15000,quantity=10,status='active')
    for entity,doc in [('Customer',customer),('Product',product)]:
        save(manager,entity,doc);old=c.read(doc['_id'])
        save(manager,entity,doc,valid=False)
        edited=dict(old,name=doc['name']+' sửa');save(manager,entity,edited,editing=True)
        assert c.read(doc['_id'])['name']==edited['name']
        save(manager,entity,edited,editing=True,valid=False)
    invalid=dict(product,_id='qa-invalid',price=-1);save(manager,'Product',invalid,valid=False)
    save(manager,'Customer',dict(customer,_id='qa-invalid-customer',email='invalid-email'),valid=False)
    for value in ['0.001','70318705647382105639.70318705']:
        invalid_body=save(manager,'Product',dict(product,_id='qa-invalid-money',price=value),valid=False)
        assert 'thập phân' in invalid_body, 'Vietnamese money validation encoding'
    save(manager,'Product',dict(product,_id='qa-decimal',price='7031870564.27'))
    assert str(c.read('qa-decimal')['price'])=='7031870564.27'
    order=dict(_id='qa-order',customerId='qa-customer',orderDate='2026-10-28',status='processing',total=1,
               items=[dict(productId='qa-product',quantity=2,price=15000)],address='Địa chỉ giả')
    stage='order transitions'
    save(manager,'Order',dict(order,status='delivered'),valid=False)
    save(manager,'Order',dict(order,customerId='missing'),valid=False)
    oversized=dict(order,_id='qa-oversized',items=[dict(productId='qa-product',quantity=2000000000,price='999999999999.99')])
    save(manager,'Order',oversized,valid=False)
    save(manager,'Order',order);assert c.read('qa-order')['total']==30000
    for entity,id in [('Customer','qa-customer'),('Product','qa-product')]:
        rev=c.read(id)['_rev'];manager.post('/'+entity+'/DeleteConfirm',dict(id=id,rev=rev),'/'+entity+'/Delete?id='+id)
        assert c.read(id)['_rev']==rev
    save(manager,'Order',dict(c.read('qa-order'),status='shipping'),True,False)
    for status in ['confirmed','preparing']:
        save(manager,'Order',dict(c.read('qa-order'),status=status),True)
    stage='delivery CRUD'
    delivery=dict(_id='qa-delivery',orderId='qa-order',status='pending',shipperId='qashipper',address='Địa chỉ giả')
    save(manager,'Delivery',dict(delivery,orderId='missing-order'),valid=False)
    save(manager,'Delivery',delivery)
    stale=c.read('qa-delivery')['_rev']
    # Only pending delivery can be deleted; current revision required.
    manager.post('/Delivery/DeleteConfirm',dict(id='qa-delivery',rev='1-stale'),'/Delivery/Delete?id=qa-delivery')
    assert c.read('qa-delivery')['_rev']==stale
    manager.post('/Delivery/DeleteConfirm',dict(id='qa-delivery',rev=stale),'/Delivery/Delete?id=qa-delivery')
    assert c.read('qa-order')['deliveryId'] is None
    delivery['_id']='qa-delivery2';save(manager,'Delivery',delivery)
    save(manager,'Delivery',dict(delivery,_id='qa-duplicate-delivery'),valid=False)
    save(manager,'Delivery',dict(c.read('qa-delivery2'),status='assigned',shipperId='qamanager'),True,False)
    save(manager,'Delivery',dict(c.read('qa-delivery2'),status='assigned'),True)
    other.denied('/Delivery/Edit?id=qa-delivery2')
    rev=c.read('qa-delivery2')['_rev']
    denied_body,_=other.post('/Delivery/Edit',flat(dict(c.read('qa-delivery2'),status='shipping')),'/Delivery')
    assert 'validation-summary-errors' in denied_body and c.read('qa-delivery2')['_rev']==rev
    assert 'qa-delivery2' not in other.get('/Delivery')[0]
    stage='shipping workflow'
    save(shipper,'Delivery',dict(c.read('qa-delivery2'),status='delivered'),True,False)
    save(shipper,'Delivery',dict(c.read('qa-delivery2'),status='shipping',shipperId='qaother'),True)
    assert c.read('qa-delivery2')['shipperId']=='qashipper' and c.read('qa-order')['status']=='shipping'
    assert 'qa-delivery2' in shipper.get('/Shipping')[0] and 'qa-delivery2' not in other.get('/Shipping')[0]
    save(shipper,'Delivery',dict(c.read('qa-delivery2'),status='failed'),True)
    assert c.read('qa-order')['status']=='preparing'
    save(manager,'Delivery',dict(c.read('qa-delivery2'),status='assigned'),True)
    save(shipper,'Delivery',dict(c.read('qa-delivery2'),status='shipping'),True)
    save(shipper,'Delivery',dict(c.read('qa-delivery2'),status='delivered'),True)
    assert c.read('qa-order')['status']=='delivered' and c.read('qa-order')['pendingDelivery'] is None
    assert 'qa-delivery2' not in shipper.get('/Shipping')[0]
    assert len(c.read('qa-order')['history'])>=6
    save(manager,'Order',dict(c.read('qa-order'),status='processing'),True,False)
    print('PASS CRUD, totals, invalid references/prices, deletion references, stale revisions, complete order/delivery and failure/retry workflow')
    stage='outbox recovery'
    recovery=dict(order,_id='qa-recovery',status='processing');save(manager,'Order',recovery)
    for status in ['confirmed','preparing']: save(manager,'Order',dict(c.read('qa-recovery'),status=status),True)
    snapshot=dict(_id='qa-projection',type='delivery',orderId='qa-recovery',status='pending',shipperId='qashipper')
    aggregate=c.read('qa-recovery');aggregate['deliveryId']='qa-projection'
    aggregate['pendingDelivery']=dict(target=snapshot,expectedRev=None,token=secrets.token_hex(16),delete=False)
    c.send('/qa-recovery','PUT',aggregate) # Simulated crash after durable order write, before projection.
    save(manager,'Order',dict(c.read('qa-recovery'),status='cancelled'),True,False)
    body,_=admin.get('/Sync');assert 'qa-recovery' in body
    admin.post('/Sync/Retry',dict(id='qa-recovery'),'/Sync')
    assert c.read('qa-projection')['status']=='pending' and c.read('qa-recovery')['pendingDelivery'] is None
    first=c.read('qa-projection')['_rev'];admin.post('/Sync/Retry',dict(id='qa-recovery'),'/Sync');assert c.read('qa-projection')['_rev']==first
    # Simulate a projection conflict; retry must retain intent and preserve external revision.
    aggregate=c.read('qa-recovery');aggregate['pendingDelivery']=dict(target=c.read('qa-projection'),expectedRev='1-stale',token=secrets.token_hex(16),delete=False)
    c.send('/qa-recovery','PUT',aggregate)
    admin.post('/Sync/Retry',dict(id='qa-recovery'),'/Sync')
    assert c.read('qa-recovery')['pendingDelivery'] is not None and c.read('qa-projection')['_rev']==first
    aggregate=c.read('qa-recovery');aggregate['pendingDelivery']['expectedRev']=first;c.send('/qa-recovery','PUT',aggregate)
    admin.post('/Sync/Retry',dict(id='qa-recovery'),'/Sync');assert c.read('qa-recovery')['pendingDelivery'] is None
    print('PASS durable outbox crash recovery, lock, idempotent retry, conflict preserves external revision')
    stage='standalone CRUD regression'
    # Retain the P1 delete/revision/unknown-field checks under authenticated access.
    for entity,doc in [('Customer',dict(_id='qa-delete-c',name='Fake customer')),('Product',dict(_id='qa-delete-p',name='Fake product',price=100,quantity=1)),
                       ('Order',dict(order,_id='qa-delete-o',status='processing'))]:
        save(manager,entity,doc)
        existing=c.read(doc['_id']);existing['custom']='preserved';c.send('/'+doc['_id'],'PUT',existing)
        existing=c.read(doc['_id']);edited=dict(existing)
        if entity in ['Customer','Product']: edited['name']='Fake edited'
        else: edited['status']='confirmed'
        save(manager,entity,edited,True)
        current=c.read(doc['_id']);assert current['custom']=='preserved'
        save(manager,entity,edited,True,False)
        path='/'+entity+'/Delete?id='+doc['_id']
        manager.post('/'+entity+'/DeleteConfirm',dict(id=doc['_id'],rev=existing['_rev']),path)
        assert c.read(doc['_id'])['_rev']==current['_rev']
        manager.post('/'+entity+'/DeleteConfirm',dict(id=doc['_id'],rev=current['_rev']),path)
        try: c.read(doc['_id']);raise AssertionError('delete failed')
        except CouchError as e: assert e.status==404
    print('PASS authenticated P1 delete/stale revision/custom-field CRUD regression')
    verify_dashboard(manager,c)
    stage='query pages csrf logout'
    body,view_path=manager.get('/Customer')
    assert 'bookmark' in body and view_path=='/Customer' and 'alert-danger' not in body, 'missing next page link or list error'
    assert 'Trang tiếp theo' in body, 'Vietnamese next-page label encoding'
    next_path=html.unescape(re.search(r'href="([^"]+bookmark[^"]+)"',body).group(1));next_body,_=manager.get(next_path)
    assert 'p2-customer-001' in body and 'p2-customer-001' not in next_body, 'bookmark repeated first page'
    assert 'qa-product' in manager.get('/Product?q=Sản%20phẩm%20giả&min=10000&max=20000')[0], 'filtered product missing'
    assert 'qa-product' not in manager.get('/Product?q=not-found')[0], 'filter ignored'
    try: manager.get('/Order/Create',flat(order));raise AssertionError('CSRF accepted')
    except urllib.error.HTTPError as e: assert e.code in [400,500]
    body,path=admin.post('/Account/Logout',{},'/Home');assert path=='/Account/Login' and admin.get('/Account/Users')[1]=='/Account/Login'
    print('PASS filtered Mango UI, bookmark pages, CSRF, logout')
    print('PASS P4 isolated real CouchDB/IIS integration; test DB retained')

if __name__=='__main__':
    try: run()
    except Exception as e:
        print('FAIL P4 stage='+stage+' '+type(e).__name__+' '+(str(e) if isinstance(e,(AssertionError,CouchError)) else ''))
        sys.exit(1)
