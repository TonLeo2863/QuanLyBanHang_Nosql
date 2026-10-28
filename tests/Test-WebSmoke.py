"""Public-route smoke for the authenticated app. Does not write business documents."""
import html
import http.cookiejar
import os
import re
import urllib.error
import urllib.parse
import urllib.request
base=os.environ.get('TEST_WEB_URL','http://localhost:52517')
client=urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
for path in ['/Home','/Customer','/Product','/Order','/Delivery','/Shipping','/Order/Create']:
    with client.open(base+path,timeout=90) as response:
        assert response.status==200 and urllib.parse.urlsplit(response.url).path=='/Account/Login'
    print('PASS anonymous GET '+path+' redirects to Login')
for entity in ['Customer','Product','Order','Delivery']:
    with client.open(base+'/'+entity+'/Create',data=b'_id=',timeout=20) as response:
        assert urllib.parse.urlsplit(response.url).path=='/Account/Login'
    print('PASS anonymous POST '+entity+' denied before write')
with client.open(base+'/Account/Login',timeout=20) as response:
    body=response.read().decode('utf-8-sig')
token=html.unescape(re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"',body).group(1))
data=urllib.parse.urlencode(dict(__RequestVerificationToken=token,Username='',Password='')).encode()
with client.open(base+'/Account/Login',data=data,timeout=20) as response:
    assert 'validation-summary-errors' in response.read().decode('utf-8-sig')
print('PASS login server validation')
try:
    client.open(base+'/Account/Login',data=b'Username=invalid',timeout=20)
    raise AssertionError('CSRF accepted')
except urllib.error.HTTPError as error:
    assert error.code in [400,500]
print('PASS Login POST without CSRF token rejected')
