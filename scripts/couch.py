"""Small CouchDB CLI client. Errors never include headers or server response bodies."""
import base64
import json
import os
import urllib.request
import urllib.error
import urllib.parse

class CouchError(Exception):
    def __init__(self, status):
        self.status = status
        super().__init__('CouchDB HTTP ' + str(status))

class Couch:
    def __init__(self, database=None):
        names = ['COUCHDB_URL', 'COUCHDB_DATABASE', 'COUCHDB_USERNAME', 'COUCHDB_PASSWORD']
        if any(not os.environ.get(n, '').strip() for n in names):
            raise ValueError('Thiếu biến môi trường CouchDB')
        server = os.environ['COUCHDB_URL'].rstrip('/')
        parsed = urllib.parse.urlsplit(server)
        if (parsed.scheme not in ['http', 'https'] or parsed.username or parsed.password or
            parsed.path not in ['', '/'] or parsed.query or parsed.fragment or
            (parsed.scheme == 'http' and parsed.hostname not in ['localhost', '127.0.0.1', '::1'])):
            raise ValueError('URL CouchDB không hợp lệ')
        self.database = database or os.environ['COUCHDB_DATABASE']
        import re
        if not re.fullmatch(r'[a-z][a-z0-9_$()+/\-]*', self.database):
            raise ValueError('Tên database không hợp lệ')
        self.url = server + '/' + urllib.parse.quote(self.database, safe='')
        self.auth = 'Basic ' + base64.b64encode((os.environ['COUCHDB_USERNAME'] + ':' + os.environ['COUCHDB_PASSWORD']).encode()).decode()

    def send(self, path='', method='GET', data=None, allowed=()):
        request = urllib.request.Request(self.url + path, method=method,
            data=None if data is None else json.dumps(data, ensure_ascii=False).encode('utf-8'),
            headers={'Authorization': self.auth, 'Content-Type': 'application/json'})
        try:
            with urllib.request.urlopen(request, timeout=30) as r:
                return json.load(r)
        except urllib.error.HTTPError as e:
            if e.code in allowed: return None
            raise CouchError(e.code) from None
        except (urllib.error.URLError, TimeoutError):
            raise CouchError(0) from None

    def read(self, id):
        return self.send('/' + urllib.parse.quote(id, safe=''))

    def create(self, document):
        return self.send('/' + urllib.parse.quote(document['_id'], safe=''), 'PUT', document, allowed=(409,))

    def find(self, selector, **args):
        return self.send('/_find', 'POST', dict(selector=selector, **args))

    def all_type(self, type):
        result, bookmark = [], None
        while True:
            args = dict(limit=200, sort=[{'type':'asc'}, {'_id':'asc'}])
            if bookmark: args['bookmark'] = bookmark
            page = self.find({'type': type}, **args)
            result.extend(page['docs'])
            if not page['docs']: return result
            if not page.get('bookmark') or page['bookmark'] == bookmark:
                raise ValueError('Bookmark không hợp lệ')
            bookmark = page['bookmark']
