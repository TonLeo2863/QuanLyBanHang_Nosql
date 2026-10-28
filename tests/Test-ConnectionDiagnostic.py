"""Read-only diagnostic tests with fake credentials and a loopback HTTP stub."""
import json
import os
from pathlib import Path
import subprocess
import threading
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

class Handler(BaseHTTPRequestHandler):
    status = 200
    def log_message(self, *args): pass
    def do_GET(self):
        self.send_response(self.status)
        self.send_header('Content-Type', 'application/json')
        self.end_headers()
        self.wfile.write(json.dumps({'indexes': [{'name': 'type-id', 'def': {'fields': [{'type': 'asc'}, {'_id': 'asc'}]}}]}).encode())

server = ThreadingHTTPServer(('127.0.0.1', 0), Handler)
threading.Thread(target=server.serve_forever, daemon=True).start()
root = Path(__file__).resolve().parent.parent
cmd = ['powershell', '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', str(root/'scripts/Test-CouchDbConnection.ps1')]
env = dict(os.environ)
for name in ['COUCHDB_URL', 'COUCHDB_DATABASE', 'COUCHDB_USERNAME', 'COUCHDB_PASSWORD']:
    env.pop(name, None)
try:
    result = subprocess.run(cmd, env=env, capture_output=True, timeout=20)
    assert result.returncode == 2, 'missing configuration must return NOT RUN'
    print('PASS diagnostic missing configuration: exit 2, NOT RUN')
    env.update(COUCHDB_URL=f'http://127.0.0.1:{server.server_port}', COUCHDB_DATABASE='test-demo', COUCHDB_USERNAME='fake-test-user', COUCHDB_PASSWORD='fake-test-value')
    for status, expected in [(200, 0), (401, 1)]:
        Handler.status = status
        result = subprocess.run(cmd, env=env, capture_output=True, timeout=20)
        assert result.returncode == expected, 'unexpected diagnostic exit status'
        assert b'fake-test-value' not in result.stdout + result.stderr
        assert b'fake-test-user' not in result.stdout + result.stderr
        print('PASS diagnostic HTTP stub', status, 'exit', expected, 'no credential output')
finally:
    server.shutdown()
    server.server_close()
