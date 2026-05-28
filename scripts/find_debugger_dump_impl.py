#!/usr/bin/env python3
import os

ROOT = r"C:\Users\MPhil\AppData\Roaming\talon\user"
if not os.path.exists(ROOT):
    print("ROOT_NOT_FOUND:", ROOT)
    raise SystemExit(2)

matches = []
for dirpath, dirnames, filenames in os.walk(ROOT):
    for fn in filenames:
        path = os.path.join(dirpath, fn)
        try:
            with open(path, 'r', encoding='utf-8', errors='replace') as fh:
                for i, line in enumerate(fh, start=1):
                    if 'debugger_dump_ascii_string' in line:
                        print(f"{path}:{i}: {line.strip()}")
                        matches.append(path)
                        break
        except Exception:
            continue

if not matches:
    print('NO_IMPL_FOUND')
