"""Fetch unchanged upstream binary assets pinned by commit and SHA-256."""
from pathlib import Path
import hashlib, json, urllib.request
ROOT = Path(__file__).resolve().parent
BASE = 'https://raw.githubusercontent.com/google-ar/arcore-android-sdk/3abfeb18669c2cbb2d07057f135d117ee9d91826/samples/hello_ar_java/'
for item in json.loads((ROOT / 'upstream-assets.json').read_text()):
    target = ROOT / item['path']
    if target.is_file() and hashlib.sha256(target.read_bytes()).hexdigest() == item['sha256']:
        continue
    with urllib.request.urlopen(BASE + item['path'], timeout=90) as response:
        data = response.read()
    if hashlib.sha256(data).hexdigest() != item['sha256']:
        raise RuntimeError('Checksum mismatch: ' + item['path'])
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_bytes(data)
    print('Prepared', item['path'])
print('All pinned assets verified.')
