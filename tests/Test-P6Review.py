"""Read-only regression check of the supplied Word audit, no network/DB access."""
import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess
import sys

root = Path(__file__).resolve().parents[1]
manifest = json.loads((root/'artifacts/p6-input/manifest.json').read_text(encoding='utf-8'))
files = sorted((root/'docs/review_input').glob('*.docx'), key=lambda p: -p.stat().st_size)
assert len(files) == len(manifest) == 2, 'requires the two local review inputs'
for file, item in zip(files, manifest):
    assert hashlib.sha256(file.read_bytes()).hexdigest() == item['sha256'], 'original changed'
    assert subprocess.run(['git','check-ignore','--quiet',str(file.relative_to(root))],cwd=root).returncode == 0
comparison = json.loads((root/'artifacts/p6-input/json-comparison.json').read_text(encoding='utf-8'))
conflicts = {v['id'] for v in comparison if v['variants'] > 1}
assert conflicts == {'KH001','SP001','DH001','GH001'}, 'Word discrepancy evidence changed'
assert sum(v['occurrences'] for v in comparison) == 16
assert next(v for v in comparison if v['id']=='SP002')['variants'] == 1

# The negative audit must stay red: do not silently ignore an unsuccessful result.
audit = subprocess.run([sys.executable,str(root/'scripts/P6-SeedConsistency.py')],capture_output=True,text=True,cwd=root)
assert audit.returncode == 1, 'known seed inconsistency must be reported as failure'
report = json.loads(audit.stdout)
assert report['documents'] == 100
assert len(report['projection_mismatches']) == 5
assert report['assigned_without_authenticated_shipper'] == 10
assert all(v['orderStatus']=='confirmed' and v['deliveryStatus']=='assigned' and v['expectedOrderStatus']=='preparing' for v in report['projection_mismatches'])

# Import only the extractor (comparison runs its local read audit at module level).
spec = importlib.util.spec_from_file_location('review',root/'scripts/Inspect-ReviewInputs.py')
module = importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
assert '[EMAIL_DA_AN]' in module.clean('student@example.invalid')
fake_url = 'https://' + 'sample:synthetic' + '@example.invalid/'
assert '[URL_XAC_THUC_DA_AN]' in module.clean(fake_url)
assert 'synthetic' not in module.clean('password' + '=synthetic')
print('PASS review regression: original hashes unchanged/ignored, four conflicting IDs, negative seed audit preserved, redaction')
print('NOTE: this PASS verifies detection; seed consistency itself remains FAIL (5 pairs).')
