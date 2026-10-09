#!/usr/bin/env python3
"""Fail if tracked source text contains a Google API key.

Only scans the present Git index and small text files, not Git history or APKs.
Do not print matching values into public CI logs.
"""
from __future__ import annotations

import pathlib
import re
import subprocess
import sys

ROOT = pathlib.Path(__file__).resolve().parent.parent
KEY = re.compile(rb"AIza[0-9A-Za-z_-]{30,45}")
EXTENSIONS = {
    ".asset", ".asmdef", ".cs", ".gradle", ".html", ".htm",
    ".java", ".js", ".json", ".kt", ".kts", ".md", ".plist",
    ".prefab", ".properties", ".py", ".shader", ".sh",
    ".ts", ".tsx", ".txt", ".unity", ".uss", ".uxml",
    ".xml", ".yaml", ".yml",
}
MAX_BYTES = 8 * 1024 * 1024


def main() -> int:
    paths = subprocess.check_output(["git", "-C", str(ROOT), "ls-files", "-z"]).split(b"\0")
    found = []
    scanned = 0
    for raw in paths:
        if not raw:
            continue
        rel = raw.decode("utf-8", "surrogateescape")
        path = ROOT / rel
        if path.suffix.lower() not in EXTENSIONS or not path.is_file():
            continue
        if path.stat().st_size > MAX_BYTES:
            continue
        scanned += 1
        if KEY.search(path.read_bytes()):
            found.append(rel)
    print(f"Scanned {scanned} tracked text files; Google API key matches: {len(found)}.")
    for rel in found:
        print(f"FAIL: exposed Google API key in {rel} (value redacted)")
    if found:
        print("Rotate exposed credentials in Google Cloud; do not paste keys into logs.")
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
