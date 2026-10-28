"""Anonymous, read-only permission probe; never authenticates with exposed secrets."""
import json
import os
import sys
import urllib.parse
import urllib.request
import urllib.error

class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self,*args,**kwargs): return None

base=os.environ.get('COUCHDB_URL','http://127.0.0.1:5984').rstrip('/')
parsed=urllib.parse.urlsplit(base)
if parsed.username or parsed.password or parsed.query or parsed.fragment or parsed.path not in ['', '/'] or parsed.scheme not in ['http','https']:
    print('FAIL invalid probe URL');sys.exit(2)
if parsed.scheme=='http' and parsed.hostname not in ['localhost','127.0.0.1','::1']:
    print('FAIL remote HTTP refused');sys.exit(2)
client=urllib.request.build_opener(NoRedirect())

def get(path):
    try:
        # Deliberately no Authorization, Cookie, or credential-bearing URL.
        with client.open(urllib.request.Request(base+path,headers={'Accept':'application/json'}),timeout=5) as r:
            return r.status,json.load(r)
    except urllib.error.HTTPError as e: return e.code,None
    except Exception: return 0,None

report={'authentication_sent':False,'writes_performed':False,'endpoints':{}}
for label,path in [('root','/'),('health','/_up'),('session','/_session'),('database','/qlbanhang'),('security','/qlbanhang/_security'),('membership','/_membership')]:
    status,data=get(path);entry={'http':status}
    if isinstance(data,dict):
        if label=='root':
            version=data.get('version','')
            if isinstance(version,str) and all(c.isdigit() or c=='.' for c in version): entry['server_version']=version
        elif label=='session':
            entry['anonymous']=data.get('userCtx',{}).get('name') is None
            entry['has_server_admin_role']='_admin' in data.get('userCtx',{}).get('roles',[])
        elif label=='database': entry['live_doc_count_including_design']=data.get('doc_count')
        elif label=='security':
            for level in ['admins','members']:
                item=data.get(level,{})
                entry[level]={'names_count':len(item.get('names',[])),'roles_count':len(item.get('roles',[])),'contains_admin_role':'_admin' in item.get('roles',[])}
    report['endpoints'][label]=entry
# Check a fake seed ID and one supplied legacy ID without printing document content.
for id in ['p2-order-001','DH001']:
    status,data=get('/qlbanhang/'+id)
    report['endpoints']['sample-'+id]={'http':status,'document_body_logged':False}
print(json.dumps(report,indent=2))
