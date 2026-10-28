"""Read pages and submit invalid forms only; does not create sample data."""
import html
import http.cookiejar
import re
import urllib.error
import urllib.parse
import urllib.request

base = 'http://localhost:52515'
client = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
for path in ['/Home','/Customer','/Product','/Order','/Delivery','/Shipping','/Order/Create']:
    with client.open(base + path, timeout=60) as response:
        body = html.unescape(response.read().decode('utf-8-sig'))
        assert response.status == 200
        assert 'alert-danger' in body or path == '/Order/Create', 'missing controlled warning: ' + path
        if path == '/Order/Create': assert '__RequestVerificationToken' in body and 'orderDate' in body
        print('PASS GET ' + path + ' HTTP 200')
for entity in ['Customer','Product','Order','Delivery']:
    with client.open(base + '/' + entity + '/Create', timeout=20) as response:
        body = response.read().decode('utf-8-sig')
    token = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', body).group(1)
    data = urllib.parse.urlencode({'__RequestVerificationToken':html.unescape(token), '_id':'', 'price':'-1', 'quantity':'-1'}).encode()
    with client.open(base + '/' + entity + '/Create', data=data, timeout=20) as response:
        result = response.read().decode('utf-8-sig')
        assert response.status == 200 and 'validation-summary-errors' in result
    print('PASS invalid POST ' + entity + ' shows validation, no write')
try:
    client.open(base + '/Customer/Create', data=b'_id=', timeout=20)
    raise AssertionError('missing CSRF token accepted')
except urllib.error.HTTPError as error:
    assert error.code in [400,500]
    print('PASS POST without CSRF token rejected HTTP ' + str(error.code))
