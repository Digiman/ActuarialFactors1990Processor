#!/usr/bin/env python3
"""Download/verify script for DataFiles/manifest.json.

Default mode verifies that every manifest entry exists and matches its
recorded SHA256, that every file under DataFiles/ (except the manifest
itself) is listed, and that the manifest schema is well-formed.

With --download, missing or mismatched files are fetched from their
recorded IRS URL first, then verified. Exits non-zero on any problem,
so it can run in CI.
"""

import argparse
import hashlib
import json
import re
import sys
import urllib.request
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent.parent
DATA_DIR = REPO_ROOT / 'DataFiles'
MANIFEST = DATA_DIR / 'manifest.json'
REQUIRED_FIELDS = ('path', 'sha256', 'url', 'downloaded')


def sha256(path):
    digest = hashlib.sha256()
    with open(path, 'rb') as source:
        for chunk in iter(lambda: source.read(1 << 20), b''):
            digest.update(chunk)
    return digest.hexdigest()


def schema_problems(entries):
    problems = []
    seen = set()
    for entry in entries:
        path = entry.get('path', '<missing path>')
        for field in REQUIRED_FIELDS:
            if not entry.get(field):
                problems.append(f'{path}: missing or empty field "{field}"')
        if not re.fullmatch(r'[0-9a-f]{64}', entry.get('sha256', '')):
            problems.append(f'{path}: sha256 is not a 64-char lowercase hex digest')
        if not re.fullmatch(r'\d{4}-\d{2}-\d{2}', entry.get('downloaded', '')):
            problems.append(f'{path}: downloaded is not an ISO date (YYYY-MM-DD)')
        if path in seen:
            problems.append(f'{path}: duplicate manifest entry')
        seen.add(path)
    return problems


def download(entry):
    target = DATA_DIR / entry['path']
    print(f'downloading {entry["url"]} -> {target}')
    with urllib.request.urlopen(entry['url'], timeout=60) as response:
        target.write_bytes(response.read())


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument('--download', action='store_true',
                        help='fetch missing or mismatched files from their recorded URL before verifying')
    arguments = parser.parse_args()

    entries = json.loads(MANIFEST.read_text())['files']
    failures = schema_problems(entries)
    listed = {entry['path']: entry for entry in entries}

    for path, entry in sorted(listed.items()):
        file = DATA_DIR / path
        if file.is_file() and sha256(file) == entry['sha256']:
            continue

        problem = (f'sha256 mismatch: {path} (manifest {entry["sha256"]}, actual {sha256(file)})'
                   if file.is_file() else f'missing: {path}')

        if not arguments.download:
            failures.append(problem)
            continue

        try:
            download(entry)
        except Exception as error:
            failures.append(f'download failed: {path}: {error}')
            continue
        if sha256(file) == entry['sha256']:
            print(f'  restored and verified: {path}')
        else:
            failures.append(f'{problem}; downloaded file still mismatches manifest')

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
