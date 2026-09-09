#!/usr/bin/env python3
"""Verify DataFiles/manifest.json against the actual source files.

Every manifest entry must exist and match its recorded SHA256, and every file
under DataFiles/ (except the manifest itself) must be listed in the manifest.
Exits non-zero on any mismatch, so it can run in CI.
"""

import hashlib
import json
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent.parent
DATA_DIR = REPO_ROOT / 'DataFiles'
MANIFEST = DATA_DIR / 'manifest.json'


def sha256(path):
    digest = hashlib.sha256()
    with open(path, 'rb') as source:
        for chunk in iter(lambda: source.read(1 << 20), b''):
            digest.update(chunk)
    return digest.hexdigest()


def main():
    entries = json.loads(MANIFEST.read_text())['files']
    listed = {}
    for entry in entries:
        listed[entry['path']] = entry

    failures = []

    for path, entry in sorted(listed.items()):
        file = DATA_DIR / path
        if not file.is_file():
            failures.append(f'missing: {path}')
            continue
        actual = sha256(file)
        if actual != entry['sha256']:
            failures.append(f'sha256 mismatch: {path} (manifest {entry["sha256"]}, actual {actual})')

    actual_files = {
        str(file.relative_to(DATA_DIR))
        for file in DATA_DIR.rglob('*')
        if file.is_file() and file != MANIFEST
    }
    for path in sorted(actual_files - set(listed)):
        failures.append(f'not listed in manifest: {path}')

    if failures:
        print(f'{len(failures)} manifest problem(s):')
        for failure in failures:
            print(f'  - {failure}')
        return 1

    print(f'Manifest OK: {len(listed)} files verified.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
