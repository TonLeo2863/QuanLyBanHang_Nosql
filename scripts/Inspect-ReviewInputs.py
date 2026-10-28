"""Read DOCX OOXML without changing originals. Redacted extracts stay in artifacts."""
import hashlib
import json
import os
from pathlib import Path
import re
import sys
import zipfile
import xml.etree.ElementTree as E

N={'w':'http://schemas.openxmlformats.org/wordprocessingml/2006/main',
   'a':'http://schemas.openxmlformats.org/drawingml/2006/main',
   'r':'http://schemas.openxmlformats.org/officeDocument/2006/relationships'}
root=Path(__file__).resolve().parents[1]

def clean(text):
    secret=os.environ.get('COUCHDB_PASSWORD')
    if secret: text=text.replace(secret,'[BI_MAT_DA_AN]')
    text=re.sub(r'(?i)(password|passwd|pwd|mật khẩu|mat khau|token|api[_ -]?key)\s*[:=]\s*[^\s,;]+',r'\1=[BI_MAT_DA_AN]',text)
    text=re.sub(r'https?://[^\s/@]+:[^\s/@]+@','[URL_XAC_THUC_DA_AN]@',text)
    text=re.sub(r'\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b','[EMAIL_DA_AN]',text)
    text=re.sub(r'(?<!\d)\d{10,12}(?!\d)','[MA_HOAC_SDT_DA_AN]',text)
    return text

def text_of(node):
    return ''.join(t.text or '' for t in node.iter('{'+N['w']+'}t'))

def inspect(path,alias):
    blocks=[];paragraph=0;table=0
    with zipfile.ZipFile(path) as z:
        xml=E.fromstring(z.read('word/document.xml'))
        media=[n for n in z.namelist() if n.startswith('word/media/')]
        relationships={}
        if 'word/_rels/document.xml.rels' in z.namelist():
            for rel in E.fromstring(z.read('word/_rels/document.xml.rels')):
                relationships[rel.attrib['Id']]=rel.attrib.get('Target','')
        for node in xml.find('w:body',N):
            local=node.tag.rsplit('}',1)[-1]
            if local=='p':
                paragraph+=1
                style=node.find('w:pPr/w:pStyle',N)
                images=[relationships.get(x.attrib.get('{'+N['r']+'}embed'),'') for x in node.findall('.//a:blip',N)]
                blocks.append(dict(locator=f'{alias}:P{paragraph:04}',kind='paragraph',style='' if style is None else style.attrib.get('{'+N['w']+'}val',''),
                    text=clean(text_of(node)),images=images))
            elif local=='tbl':
                table+=1
                for rowno,row in enumerate(node.findall('w:tr',N),1):
                    cells=[]
                    for col,cell in enumerate(row.findall('w:tc',N),1):
                        cells.append(dict(locator=f'{alias}:T{table}R{rowno}C{col}',text=clean(text_of(cell))))
                    blocks.append(dict(locator=f'{alias}:T{table}R{rowno}',kind='table-row',cells=cells,text=' | '.join(c['text'] for c in cells)))
        extra=[]
        for name in z.namelist():
            if re.fullmatch(r'word/(comments|footnotes|endnotes|header\d+|footer\d+)\.xml',name):
                extra.append(dict(part=name,text=clean(text_of(E.fromstring(z.read(name))))))
        image_meta=[dict(part=name,bytes=len(z.read(name)),sha256=hashlib.sha256(z.read(name)).hexdigest()) for name in media]
        fonts={};sizes={};spacing={}
        for p in xml.findall('.//w:p',N):
            for el in p.findall('.//w:rFonts',N):
                font=el.attrib.get('{'+N['w']+'}ascii','');fonts[font]=fonts.get(font,0)+1
            for el in p.findall('.//w:sz',N):
                size=el.attrib.get('{'+N['w']+'}val','');sizes[size]=sizes.get(size,0)+1
            for el in p.findall('./w:pPr/w:spacing',N):
                line=el.attrib.get('{'+N['w']+'}line','');spacing[line]=spacing.get(line,0)+1
        stats=dict(alias=alias,file=path.name,bytes=path.stat().st_size,sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
                   paragraphs=paragraph,tables=table,images=len(media),fonts=fonts,half_point_sizes=sizes,line_spacing_twips=spacing,
                   tracked_deletions=len(xml.findall('.//w:del',N)),tracked_insertions=len(xml.findall('.//w:ins',N)))
    return dict(metadata=stats,blocks=blocks,extra_parts=extra,images=image_meta)

def run():
    files=sorted((root/'docs/review_input').glob('*.docx'),key=lambda p:-p.stat().st_size)
    if len(files)!=2: raise ValueError('Cần đúng hai DOCX đầu vào')
    out=root/'artifacts/p6-input';out.mkdir(parents=True,exist_ok=True)
    manifest=[]
    for path,alias in zip(files,['A','B']):
        result=inspect(path,alias);manifest.append(result['metadata'])
        (out/(alias+'.json')).write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
        (out/(alias+'.txt')).write_text('\n'.join(f"{b['locator']} [{b.get('style','')}] {b['text']}"+(' [IMAGE]' if b.get('images') else '') for b in result['blocks']),encoding='utf-8')
        print(alias+' paragraphs='+str(result['metadata']['paragraphs'])+' tables='+str(result['metadata']['tables'])+' images='+str(result['metadata']['images']))
    (out/'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
    print('PASS read-only OOXML extraction; original hashes recorded, redacted text local only')

if __name__=='__main__':
    try: run()
    except Exception as e: print('FAIL review extraction: '+type(e).__name__);sys.exit(1)
