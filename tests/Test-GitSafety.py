"""Synthetic credentials generated in memory, never networked or printed."""
import importlib.util
from pathlib import Path
import secrets
spec=importlib.util.spec_from_file_location('audit',Path(__file__).resolve().parents[1]/'scripts/Audit-GitSafety.py')
a=importlib.util.module_from_spec(spec);spec.loader.exec_module(a)
secret=secrets.token_urlsafe(24).encode()
keyword=b'pass'+b'word';quote=bytes([34])
source=b'var '+keyword+b' = '+quote+secret+quote+b';'
rules,exact=a.inspect(source,'Services/Example.cs',secret)
assert 'literal-password' in rules and 'configured-secret-match' in rules and exact
source=b'{'+quote+keyword+quote+b':'+quote+secret+quote+b'}'
assert a.inspect(source,'example.json',secret)[1]
source=b'Get'+b'Bytes('+quote+b'synthetic-user:'+secret+quote+b')'
assert a.inspect(source,'Example.cs',secret)[1]
source=b'new Network'+b'Credential('+quote+b'synthetic-user'+quote+b', '+quote+secret+quote+b')'
assert a.inspect(source,'Example.cs',secret)[1]
fixture=keyword+b'='+quote+b'fake-forbidden'+quote
assert a.inspect(fixture,'tests/Test-Backup.py',secret)[0]==['reviewed-negative-test-fixture']
assert 'literal-password' in a.inspect(fixture,'Services/Example.cs',secret)[0]
assert a.inspect(fixture,'tests/Test-Backup.py',b'fake-forbidden')[1]
assert a.remote_check('https://github.com/TonLeo2863/QuanLyBanHang_Nosql.git')==(True,False)
assert a.remote_check('git@github.com:TonLeo2863/QuanLyBanHang_Nosql.git')==(True,False)
assert a.remote_check('https://github.com/another/repository.git')==(False,False)
assert a.remote_check('https://github.com.invalid/TonLeo2863/QuanLyBanHang_Nosql.git')==(False,False)
scheme='https'
url=scheme+'://'+'synthetic-user'+':'+secret.decode()+'@github.com/TonLeo2863/QuanLyBanHang_Nosql.git'
assert a.remote_check(url)==(False,True)
assert a.inspect(b'Environment.GetEnvironmentVariable("COUCHDB_PASSWORD")','Example.cs',secret)[0]==[]
print('PASS Git safety 13 assertions; no real credential output or authentication')
