"""Private read of original JSON examples; outputs IDs, changed field names only."""
import importlib.util
import json
from pathlib import Path
import sys
import zipfile
import xml.etree.ElementTree as E

root=Path(__file__).resolve().parents[1]
spec=importlib.util.spec_from_file_location('review',root/'scripts/Inspect-ReviewInputs.py')
r=importlib.util.module_from_spec(spec);spec.loader.exec_module(r)
seed_spec=importlib.util.spec_from_file_location('seed',root/'scripts/Seed-Demo.py')
seed=importlib.util.module_from_spec(seed_spec);seed_spec.loader.exec_module(seed)

def examples(file,alias):
    with zipfile.ZipFile(file) as z: body=E.fromstring(z.read('word/document.xml')).find('w:body',r.N)
    chunks=[];paragraph=table=0;buffer=[];start=None
    for node in body:
        kind=node.tag.rsplit('}',1)[-1]
        if kind=='p':
            paragraph+=1;text=r.text_of(node).strip();loc=f'{alias}:P{paragraph:04}'
            if text=='{' and not buffer: buffer=[text];start=loc
            elif buffer:
                buffer.append(text)
                if text=='}':
                    try:
                        doc=json.loads('\n'.join(buffer))
                        if isinstance(doc,dict) and '_id' in doc: chunks.append((start,doc))
                        buffer=[]
                    except json.JSONDecodeError: pass
        elif kind=='tbl':
            table+=1
            for rownum,row in enumerate(node.findall('w:tr',r.N),1):
                for colnum,cell in enumerate(row.findall('w:tc',r.N),1):
                    text=r.text_of(cell).strip()
                    try:
                        doc=json.loads(text)
                        if isinstance(doc,dict) and '_id' in doc: chunks.append((f'{alias}:T{table}R{rownum}C{colnum}',doc))
                    except json.JSONDecodeError: pass
    return chunks

files=sorted((root/'docs/review_input').glob('*.docx'),key=lambda p:-p.stat().st_size)
groups={}
for alias,file in zip(['A','B'],files):
    docs=examples(file,alias)
    print(alias+' JSON examples='+str(len(docs)))
    for loc,doc in docs: groups.setdefault(doc['_id'],[]).append((loc,doc))
result=[]
for id,entries in groups.items():
    variants=[]
    for loc,doc in entries:
        found=next((v for v in variants if v['doc']==doc),None)
        if found: found['locations'].append(loc)
        else: variants.append(dict(doc=doc,locations=[loc]))
    keys=set().union(*(set(v['doc']) for v in variants))
    changed=sorted(k for k in keys if any(v['doc'].get(k)!=variants[0]['doc'].get(k) for v in variants[1:]))
    result.append(dict(id=id,occurrences=len(entries),variants=len(variants),locations=[v['locations'] for v in variants],changed_fields=changed))
print(json.dumps(result,ensure_ascii=True,indent=2))
seed_ids={d['_id'] for d in seed.dataset()}
print('Word sample IDs present in canonical P2 seed: '+str(len(set(groups)&seed_ids)))
for alias,file in zip(['A','B'],files):
    docs=examples(file,alias);ids={d['_id'] for _,d in docs}
    missing=set()
    for loc,doc in docs:
        if doc.get('type')=='order':
            for item in doc['items']:
                if item['productId'] not in ids: missing.add(item['productId'])
    print(alias+' product references absent from supplied JSON examples: '+','.join(sorted(missing)))
(root/'artifacts/p6-input/json-comparison.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
