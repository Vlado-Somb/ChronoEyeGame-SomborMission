#!/usr/bin/env python3
"""Fail if tracked source text contains a known credential pattern.

Scans tracked working-tree text files, not Git history or archive contents.
Do not print matching values into public CI logs.
"""
from __future__ import annotations

import pathlib
import re
import subprocess
import sys

ROOT = pathlib.Path(__file__).resolve().parent.parent
KEY = re.compile(rb"AIza[0-9A-Za-z_-]{30,45}")
JWT = re.compile(rb"eyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+")
PRIVATE_KEY = re.compile(rb"-----BEGIN (?:RSA |EC |OPENSSH |DSA )?PRIVATE KEY-----")



def main() -> int:
    paths = subprocess.check_output(["git", "-C", str(ROOT), "ls-files", "-z"]).split(b"\0")
    found = []
    scanned = 0
    skipped = []
    for raw in paths:
        if not raw:
            continue
        rel = raw.decode("utf-8", "surrogateescape")
        path = ROOT / rel
        if not path.is_file():
            skipped.append(rel)
            continue
        if path.suffix.lower() in {'.apk', '.aab', '.zip', '.jar', '.aar'}:
            skipped.append(rel)
            continue
        # Do not exclude .env, extensionless files, backups, or large Unity scenes.
        with path.open('rb') as handle:
            initial = handle.read(8192)
            if b'\0' in initial:
                skipped.append(rel)
                continue
            tail = b''
            chunk = initial
            matched = False
            while chunk:
                data = tail + chunk
                if KEY.search(data) or JWT.search(data) or PRIVATE_KEY.search(data):
                    matched = True
                tail = data[-4096:]
                chunk = handle.read(65536)
        scanned += 1
        if matched:
            found.append(rel)
    print(f"Scanned {scanned} tracked text files; credential-pattern matches: {len(found)}.")
    print(f"Not inspected as text: {len(skipped)} files (binary/archive/missing); history and APK contents require a separate audit.")
    for rel in found:
        print(f"FAIL: credential pattern in {rel} (value redacted)")
    if found:
        print("Revoke/rotate exposed credentials at their provider; do not paste values into logs.")
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
