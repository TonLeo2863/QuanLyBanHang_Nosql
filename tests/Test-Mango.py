import json
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]/'scripts'))
from couch import Couch, CouchError
root = Path(__file__).resolve().parents[1]
try:
    c = Couch()
    for case in json.loads((root/'docs/mango-queries.json').read_text(encoding='utf-8')):
        q = case['query']
        response = c.send('/_find','POST',dict(q,execution_stats=True))
        seed = [d for d in response['docs'] if d['_id'].startswith('p2-')]
        assert len(seed)==case['expectedSeedCount'], case['id']+' count '+str(len(seed))
        plan = c.send('/_explain','POST',q)
        assert plan['index']['name']==q['use_index'].removeprefix('idx-'), case['id']+' index'
        print('PASS '+case['id']+' seedCount='+str(len(seed))+' index='+plan['index']['name'])
    seen, bookmark = [], None
    while True:
        q=dict(selector={'type':'product'},sort=[{'type':'asc'},{'_id':'asc'}],limit=7,use_index='idx-type-id')
        if bookmark: q['bookmark']=bookmark
        page=c.send('/_find','POST',q)
        if not page['docs']: break
        seen.extend(d['_id'] for d in page['docs'])
        assert page['bookmark']!=bookmark
        bookmark=page['bookmark']
    assert len(seen)==len(set(seen)) and seen==[d['_id'] for d in c.all_type('product')]
    print('PASS bookmark pages limit=7: complete, ordered, no duplicates')
except Exception as e:
    print('FAIL Mango: '+type(e).__name__+' '+(str(e) if isinstance(e,(AssertionError,CouchError)) else ''))
    sys.exit(1)
