import importlib.util
from pathlib import Path
import secrets
import sys
spec=importlib.util.spec_from_file_location('workflow',Path(__file__).with_name('Test-Workflow.py'))
w=importlib.util.module_from_spec(spec);spec.loader.exec_module(w)
try:
    w.wait_ready()
    browser=w.Browser()
    assert browser.get('/Home')[1]=='/Account/Login'
    fake=secrets.token_urlsafe(24)
    body,path=browser.post('/Account/Login',dict(Username='qa-unavailable',Password=fake))
    assert path=='/Account/Login' and 'validation-summary-errors' in body
    assert fake not in body and 'Authorization' not in body and 'passwordHash' not in body
    assert 'Server Error' not in body and 'Exception Details' not in body
    print('PASS backend configuration/connection failure stays on Login with controlled error, no disclosure')
except Exception as e:
    print('FAIL login backend failure: '+type(e).__name__);sys.exit(1)
