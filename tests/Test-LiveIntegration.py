"""Real IIS/CouchDB integration; only deletes UUID-prefixed documents it creates."""
import base64
import html
import http.cookiejar
import json
import os
import re
import sys
import subprocess
import uuid
import urllib.error
import urllib.parse
import urllib.request

base = os.environ.get('TEST_WEB_URL', 'http://localhost:52516')
db = os.environ['COUCHDB_URL'].rstrip('/') + '/' + urllib.parse.quote(os.environ['COUCHDB_DATABASE'], safe='')
auth = 'Basic ' + base64.b64encode((os.environ['COUCHDB_USERNAME'] + ':' + os.environ['COUCHDB_PASSWORD']).encode()).decode()
client = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
prefix = 'test-p01-' + uuid.uuid4().hex + '-'
created = []

def couch(path='', method='GET', data=None):
    req = urllib.request.Request(db + path, data=None if data is None else json.dumps(data).encode(),
                                 headers={'Authorization': auth, 'Content-Type': 'application/json'}, method=method)
    with urllib.request.urlopen(req, timeout=30) as r:
        return json.load(r)

def read(id):
    return couch('/' + urllib.parse.quote(id, safe=''))

def page(path, fields=None):
    data = None if fields is None else urllib.parse.urlencode(fields).encode()
    with client.open(base + path, data=data, timeout=90) as r:
        assert r.status == 200
        return html.unescape(r.read().decode('utf-8-sig'))

def token(path):
    return re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', page(path)).group(1)

def submit(entity, action, fields):
    path = '/' + entity + '/' + action
    result = page(path, dict(fields, __RequestVerificationToken=token(path + ('?id=' + fields['_id'] if action == 'Edit' else ''))))
    assert 'validation-summary-errors' not in result and 'alert-danger' not in result, entity + ' ' + action
    return result

def find(selector, **kwargs):
    return couch('/_find', 'POST', dict(selector=selector, **kwargs))

def verify_views():
    counts = [len(find({'type': t}, limit=10000)['docs']) for t in ['customer', 'product', 'order', 'delivery']]
    shipping = find({'type': 'delivery', 'status': 'shipping'}, limit=10000)['docs']
    body = page('/Home')
    assert 'alert-danger' not in body
    numbers = [int(n) for n in re.findall(r'class="(?:home-number|delivery-number)"\s*>\s*(\d+)', body)]
    assert numbers == counts + [len(shipping)], 'Dashboard count mismatch'
    body = page('/Shipping')
    assert 'alert-danger' not in body
    assert int(re.search(r'<strong>(\d+)</strong>', body).group(1)) == len(shipping)
    assert all(d['_id'] in body for d in shipping)
    print('PASS Dashboard counts and Shipping IDs match CouchDB')

def run():
    session_url = os.environ['COUCHDB_URL'].rstrip('/') + '/_session'
    req = urllib.request.Request(session_url, headers={'Authorization': auth})
    with urllib.request.urlopen(req) as r:
        assert json.load(r)['userCtx']['name'] == os.environ['COUCHDB_USERNAME']
    print('PASS authenticated session identity')
    sample_ids = ['demo-' + t + '-001' for t in ['customer', 'product', 'order', 'delivery']]
    before = [read(id) for id in sample_ids]
    initialized = subprocess.run(['powershell', '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File',
                                  'scripts/Initialize-CouchDb.ps1'], capture_output=True)
    assert initialized.returncode == 0
    assert before == [read(id) for id in sample_ids]
    print('PASS initializer rerun preserves existing sample content and revisions')
    verify_views()
    ids = {e: prefix + e.lower() for e in ['Customer', 'Product', 'Order', 'Delivery']}
    docs = {
        'Customer': dict(name='Integration fake customer', email='test@example.invalid', address='Fake address'),
        'Product': dict(name='Integration fake product', price='12000', quantity='10', status='active'),
        'Order': dict(customerId=ids['Customer'], orderDate='2026-10-28', status='shipping', total='1',
                      **{'items[0].productId': ids['Product'], 'items[0].quantity': '2', 'items[0].price': '12000'}),
        'Delivery': dict(orderId=ids['Order'], status='shipping', shipperName='Fake shipper', address='Fake address')}
    for entity, fields in docs.items():
        fields['_id'] = ids[entity]
        submit(entity, 'Create', fields)
        created.append(ids[entity])
        current = read(ids[entity])
        assert current['type'] == entity.lower() and current['_rev']
        assert ids[entity] in page('/' + entity)
        assert ids[entity] in page('/' + entity + '/Edit?id=' + ids[entity])
        if entity == 'Order': assert current['total'] == 24000
        duplicate = page('/' + entity + '/Create', dict(fields, __RequestVerificationToken=token('/' + entity + '/Create')))
        assert 'validation-summary-errors' in duplicate and read(ids[entity])['_rev'] == current['_rev']
        invalid = dict(fields)
        if entity == 'Customer': invalid['email'] = 'invalid-email'
        if entity == 'Product': invalid['price'] = '-1'
        if entity == 'Order': invalid['customerId'] = prefix + 'missing'
        if entity == 'Delivery': invalid['orderId'] = prefix + 'missing'
        invalid['_id'] = prefix + 'invalid-' + entity
        rejected = page('/' + entity + '/Create', dict(invalid, __RequestVerificationToken=token('/' + entity + '/Create')))
        assert 'validation-summary-errors' in rejected
        try:
            read(invalid['_id'])
            raise AssertionError('invalid document written')
        except urllib.error.HTTPError as e:
            assert e.code == 404
        if entity == 'Delivery':
            assert ids[entity] in page('/Shipping')
            verify_views()
        # Add a field via CouchDB and verify app updates preserve it.
        current['integrationExtra'] = 'preserve'
        couch('/' + ids[entity], 'PUT', current)
        fields['_rev'] = read(ids[entity])['_rev']
        if entity in ['Customer', 'Product']: fields['name'] += ' edited'
        if entity == 'Order': fields['items[0].quantity'] = '3'
        if entity == 'Delivery': fields['status'] = 'delivered'
        submit(entity, 'Edit', fields)
        saved = read(ids[entity])
        assert saved['_rev'] != fields['_rev'] and saved['integrationExtra'] == 'preserve'
        if entity == 'Order': assert saved['total'] == 36000
        stale = page('/' + entity + '/Edit', dict(fields, __RequestVerificationToken=token('/' + entity + '/Edit?id=' + ids[entity])))
        assert 'validation-summary-errors' in stale and read(ids[entity])['_rev'] == saved['_rev']
        print('PASS IIS CRUD create/read/update, stale revision and extra-field preservation ' + entity)
        print('PASS duplicate ID and invalid data/reference rejected ' + entity)
    verify_views()
    basic = find({'type': 'product', '_id': ids['Product']})['docs']
    assert len(basic) == 1 and basic[0]['price'] == 12000
    advanced = find({'$and': [{'type': 'order'}, {'customerId': ids['Customer']}, {'total': {'$gte': 30000}},
                              {'status': {'$in': ['shipping', 'delivered']}}, {'items': {'$elemMatch': {'quantity': {'$gte': 3}}}}]})['docs']
    assert [d['_id'] for d in advanced] == [ids['Order']]
    expected = [d['_id'] for d in find({'type': 'product'}, sort=[{'type': 'asc'}, {'_id': 'asc'}], limit=10000)['docs']]
    seen, bookmark = [], None
    for _ in range(len(expected) + 2):
        args = dict(sort=[{'type': 'asc'}, {'_id': 'asc'}], limit=1, use_index='idx-type-id')
        if bookmark: args['bookmark'] = bookmark
        response = find({'type': 'product'}, **args)
        if not response['docs']: break
        seen.extend(d['_id'] for d in response['docs'])
        bookmark = response['bookmark']
    assert seen == expected and len(set(seen)) == len(seen)
    print('PASS Mango basic, advanced ($and/$in/$gte/$elemMatch), sorted bookmark pagination')
    for entity in reversed(list(docs)):
        id = ids[entity]
        current = read(id)
        # Stale delete must leave the document intact.
        page('/' + entity + '/DeleteConfirm', dict(id=id, rev='1-stale', __RequestVerificationToken=token('/' + entity + '/Delete?id=' + id)))
        assert read(id)['_rev'] == current['_rev']
        result = page('/' + entity + '/DeleteConfirm', dict(id=id, rev=current['_rev'], __RequestVerificationToken=token('/' + entity + '/Delete?id=' + id)))
        try:
            read(id)
            raise AssertionError('delete failed ' + entity)
        except urllib.error.HTTPError as e:
            assert e.code == 404
        created.remove(id)
        print('PASS IIS delete and stale delete rejected ' + entity)
    verify_views()
    for path in ['/Customer', '/Product', '/Order', '/Delivery', '/Order/Create']:
        assert 'alert-danger' not in page(path)
    print('PASS seven IIS GET pages with real CouchDB')
    try:
        page('/Customer/Create', {'_id': prefix + 'csrf', 'name': 'Fake'})
        raise AssertionError('CSRF accepted')
    except urllib.error.HTTPError as e:
        assert e.code in [400, 500]
    print('PASS IIS POST without anti-forgery token rejected')

try:
    run()
except Exception as e:
    # Never print request headers, environment values or raw server responses.
    print('FAIL integration: ' + type(e).__name__ + (' HTTP ' + str(e.code) if isinstance(e, urllib.error.HTTPError) else ''))
    sys.exit(1)
finally:
    for id in reversed(created):
        try:
            assert id.startswith(prefix)
            couch('/' + id + '?rev=' + urllib.parse.quote(read(id)['_rev']), 'DELETE')
            print('PASS cleanup owned test document')
        except Exception:
            print('FAIL cleanup owned test document; inspect test-p01 prefix')
