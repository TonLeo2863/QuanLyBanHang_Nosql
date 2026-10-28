"""Deterministic fake dataset; create-only PUT, never overwrites existing IDs."""
from pathlib import Path
import json
import sys
from couch import Couch, CouchError

def dataset():
    docs = []
    for i in range(1, 21):
        docs.append(dict(_id=f'p2-customer-{i:03}', type='customer', name=f'Khách giả lập {i:02}',
                         email=f'customer{i}@example.invalid', phone='', address=f'Địa chỉ mô phỏng {i}'))
    for i in range(1, 31):
        docs.append(dict(_id=f'p2-product-{i:03}', type='product', name=f'Sản phẩm mô phỏng {i:02}',
                         category=['Đồ gia dụng', 'Văn phòng', 'Điện tử'][i % 3], price=i*10000,
                         quantity=100+i, status='active'))
    statuses = ['processing', 'confirmed', 'preparing', 'shipping', 'delivered', 'cancelled']
    for i in range(1, 31):
        items = [dict(productId=f'p2-product-{j:03}', quantity=n, price=j*10000) for j,n in [(i,2),(i%30+1,1)]]
        docs.append(dict(_id=f'p2-order-{i:03}', type='order', customerId=f'p2-customer-{(i-1)%20+1:03}',
                         orderDate=f'2026-10-{(i-1)%28+1:02}', status=statuses[(i-1)%6], items=items,
                         total=sum(x['quantity']*x['price'] for x in items), paymentMethod='COD', address='Địa chỉ mô phỏng'))
    eligible = [d for d in docs if d['type']=='order' and d['status'] in ['confirmed','preparing','shipping','delivered']]
    for i, order in enumerate(eligible, 1):
        status = {'confirmed':'assigned', 'preparing':'assigned', 'shipping':'shipping', 'delivered':'delivered'}[order['status']]
        docs.append(dict(_id=f'p2-delivery-{i:03}', type='delivery', orderId=order['_id'], status=status,
                         shipperId='', shipperName=f'Người giao mô phỏng {(i-1)%3+1}', shipperPhone='',
                         pickupDate=order['orderDate'], expectedDate='2026-11-01', address='Địa chỉ mô phỏng'))
    return docs

INDEXES = {
    'type-id':['type','_id'], 'type-status-id':['type','status','_id'],
    'type-customer-date':['type','customerId','orderDate','_id'],
    'type-date':['type','orderDate','_id'], 'type-category-price':['type','category','price','_id'],
    'type-price':['type','price','_id'], 'type-total':['type','total','_id'],
    'type-order':['type','orderId','_id'], 'type-shipper-status':['type','shipperId','status','_id']}

def seed(c):
    c.send('', 'PUT', allowed=(412,))
    for name, fields in INDEXES.items():
        c.send('/_index','POST',dict(index=dict(fields=fields), ddoc='idx-'+name, name=name, type='json'))
    inserted = 0
    for doc in dataset():
        if c.create(doc): inserted += 1
    print(f'PASS seed: {inserted} inserted; existing IDs preserved; 100 fake documents, 9 indexes')

if __name__ == '__main__':
    try: seed(Couch())
    except (CouchError, ValueError) as e:
        print('FAIL seed: ' + str(e)); sys.exit(1)
