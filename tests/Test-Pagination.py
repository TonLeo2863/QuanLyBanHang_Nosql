import importlib.util, pathlib, secrets
p=pathlib.Path(__file__).with_name("Test-Workflow.py")
spec=importlib.util.spec_from_file_location("workflow",p);w=importlib.util.module_from_spec(spec);spec.loader.exec_module(w)
c=w.Couch();w.seed.seed(c)
password=secrets.token_urlsafe(24);c.create(w.users.user_document("qapage",password,"Administrator"))
c.create(dict(_id="qa-extra",type="customer",name="Fake"))
b=w.Browser();b.login("qapage",password)
body,path=b.get("/Customer")
print("Page1 rows=",len(w.re.findall(r"/Customer/Edit",body)),"next=","bookmark" in body,"error=","alert-danger" in body)
print("Next link label:",[x.encode('unicode_escape').decode() for x in w.re.findall(r'href="[^"]+bookmark[^"]+">([^<]+)',body)])
next_path=w.html.unescape(w.re.search(r'href="([^"]+bookmark[^"]+)"',body).group(1))
page2,path=b.get(next_path)
print("Page2 rows=",len(w.re.findall(r"/Customer/Edit",page2)),"first repeated=","p2-customer-001" in page2,"extra=","qa-extra" in page2)
assert "p2-customer-001" in body and "p2-customer-001" not in page2
assert 'Trang ti\u1ebfp theo' in body, 'Vietnamese label encoding'
print("PASS pagination")
