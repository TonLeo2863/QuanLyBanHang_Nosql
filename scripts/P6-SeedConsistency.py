"""Offline audit of the canonical seed against P4 projection rules; never writes DB."""
import importlib.util
import json
from pathlib import Path
import sys
root=Path(__file__).resolve().parents[1]
spec=importlib.util.spec_from_file_location('seed',root/'scripts/Seed-Demo.py')
seed=importlib.util.module_from_spec(spec);spec.loader.exec_module(seed)
docs=seed.dataset();by_id={d['_id']:d for d in docs};findings=[]
for delivery in (d for d in docs if d['type']=='delivery'):
    order=by_id[delivery['orderId']]
    expected='shipping' if delivery['status']=='shipping' else 'delivered' if delivery['status']=='delivered' else 'preparing'
    if order['status']!=expected:
        findings.append(dict(orderId=order['_id'],deliveryId=delivery['_id'],orderStatus=order['status'],deliveryStatus=delivery['status'],expectedOrderStatus=expected))
report=dict(scope='source seed only, not live database',documents=len(docs),projection_mismatches=findings,
            assigned_without_authenticated_shipper=sum(d['type']=='delivery' and d['status']=='assigned' and not d.get('shipperId') for d in docs),
            note='Legacy rows: no automatic overwrite/migration. Prepare consistent demo records separately.')
print(json.dumps(report,indent=2))
sys.exit(1 if findings else 0)
