"""Test initializer against a local HTTP stub; never contacts CouchDB."""
import json
import os
import subprocess
import threading
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

state = {"db": False, "docs": {}, "indexes": set(), "deny": False}
class Handler(BaseHTTPRequestHandler):
    def log_message(self, *args): pass
    def send(self, status):
        self.send_response(status); self.end_headers(); self.wfile.write(b'{}')
    def do_PUT(self):
        if state['deny']: return self.send(401)
        if self.path == '/test-demo':
            existed = state['db']; state['db'] = True
            return self.send(412 if existed else 201)
        key = self.path.rsplit('/', 1)[-1]
        body = json.loads(self.rfile.read(int(self.headers.get('Content-Length', '0'))).decode('utf-8-sig'))
        if key in state['docs']: return self.send(409)
        state['docs'][key] = body
        self.send(201)
    def do_POST(self):
        body = json.loads(self.rfile.read(int(self.headers.get('Content-Length', '0'))).decode('utf-8-sig'))
        assert self.path == '/test-demo/_index'
        state['indexes'].add(body['name']); self.send(200)

root = Path(__file__).resolve().parent.parent
server = ThreadingHTTPServer(('127.0.0.1', 0), Handler)
threading.Thread(target=server.serve_forever, daemon=True).start()
env = dict(os.environ, COUCHDB_URL=f'http://127.0.0.1:{server.server_port}', COUCHDB_DATABASE='test-demo', COUCHDB_USERNAME='fake-test-user', COUCHDB_PASSWORD='fake-test-value')
cmd = ['powershell', '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', str(root/'scripts/Initialize-CouchDb.ps1')]
def run(): return subprocess.run(cmd, env=env, capture_output=True, timeout=45)
try:
    first=run(); assert first.returncode == 0, 'first initialization failed'
    assert state['db'] and len(state['docs']) == 4 and state['indexes'] == {'type-id'}
    docs = state['docs']
    order = docs['demo-order-001']; delivery = docs['demo-delivery-001']
    assert order['customerId'] in docs and all(i['productId'] in docs for i in order['items'])
    assert order['total'] == sum(i['quantity']*i['price'] for i in order['items'])
    assert delivery['orderId'] in docs and delivery['status'] == order['status']
    docs['demo-product-001']['custom'] = 'preserve-me'
    before = json.dumps(docs, sort_keys=True)
    second=run(); assert second.returncode == 0, 'second initialization failed'
    assert json.dumps(docs, sort_keys=True) == before, 'existing data changed'
    state['deny'] = True
    denied=run(); assert denied.returncode != 0, '401 must fail initialization'
    assert b'fake-test-value' not in denied.stdout + denied.stderr
    print('PASS initializer: 2 runs, 4 linked sample documents, index, preserved existing data, controlled 401 (HTTP stub).')
finally: server.shutdown(); server.server_close()
