"""Fail closed before committing; does not print matched values."""
import os
import pathlib
import re
import subprocess
import sys
files = subprocess.check_output(['git','diff','--cached','--name-only','-z']).decode().split('\0')
secret = os.environ.get('COUCHDB_PASSWORD')
if not secret:
    print('BLOCKED secret scan: thiếu cấu hình đối chiếu'); sys.exit(2)
bad = 0
for file in filter(None, files):
    content = subprocess.check_output(['git','show',':'+file])
    if (secret.encode() in content or re.search(rb'gh[pousr]_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,}|AKIA[0-9A-Z]{16}|-----BEGIN [A-Z ]*PRIVATE KEY-----|https?://[^\s/]+:[^\s/]+@',content)
        or any(p.lower() in ['bin','obj','.vs'] for p in pathlib.PurePath(file).parts)):
        bad += 1
print('Secret/artifact scan findings='+str(bad))
sys.exit(1 if bad else 0)
