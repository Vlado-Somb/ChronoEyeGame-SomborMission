#!/usr/bin/env python3
"""Read-only verification of a fresh archive clone; does not authorize deletion."""
import argparse
import json
import os
import subprocess
from pathlib import Path


def verify(repo, inventory):
    def git(*args):
        return subprocess.check_output(
            ['git', '-C', str(repo), *args],
            env={**os.environ, 'GIT_NO_LAZY_FETCH': '1'}, stderr=subprocess.PIPE)

    # Reject partial clones even if their currently cached subset looks complete.
    configs = git('config', '--list').decode().lower()
    if 'partialclone' in configs or '.promisor=true' in configs:
        raise ValueError('A full fresh clone is required; partial/promisor clone detected')
    if git('rev-parse', '--is-shallow-repository').strip() != b'false':
        raise ValueError('Shallow archive is incomplete')
    heads = []
    for ref in inventory['refs']:
        branch, sha = ref['branch'], ref['commit']
        candidates = ['refs/heads/'+branch, 'refs/remotes/origin/'+branch]
        actual = []
        for candidate in candidates:
            try:
                actual.append(git('rev-parse', '--verify', candidate).decode().strip())
            except subprocess.CalledProcessError:
                pass
        if sha not in actual:
            raise ValueError('Missing or changed archived ref: '+branch)
        heads.append(sha)
        entries = [inventory['fileRecords'][i] for i in inventory['trees'][ref['inventorySha256']]]
        tree = {}
        for entry in git('ls-tree', '-r', '-z', '--full-tree', sha).split(b'\0'):
            if entry:
                meta, name = entry.split(b'\t', 1)
                mode, kind, oid = meta.decode().split()
                tree[name.decode()] = (mode, kind, oid)
        for entry in entries:
            if tree.get(entry['path']) != (entry['mode'],entry['type'],entry['oid']):
                raise ValueError('Legacy tree differs: '+branch+' / '+entry['path'])
    objects = git('rev-list', '--objects', '--missing=print', *heads)
    if any(line.startswith(b'?') for line in objects.splitlines()):
        raise ValueError('Missing historical Git objects')
    git('fsck', '--full', '--no-reflogs')
    return {'status':'GIT_OBJECTS_VERIFIED_ONLY', 'refs':len(heads),
            'privateVisibilityVerified':False, 'externalObjectsVerified':False,
            'deletionAuthorized':False}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('archive_clone', type=Path)
    parser.add_argument('--inventory', type=Path, default=Path(__file__).with_name('unity-source-inventory.json'))
    args = parser.parse_args()
    try:
        print(json.dumps(verify(args.archive_clone, json.loads(args.inventory.read_text())), indent=2))
    except (ValueError, subprocess.CalledProcessError) as error:
        # Never expose arbitrary process output or secret-bearing object contents.
        print(json.dumps({'status':'NOT_VERIFIED','reason':str(error) if isinstance(error,ValueError) else 'Git object verification failed'}))
        raise SystemExit(1)
