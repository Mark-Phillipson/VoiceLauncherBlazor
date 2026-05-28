#!/usr/bin/env python3
import sqlite3
import os
import sys

DB_PATH = r"C:\Users\MPhil\AppData\Roaming\VoiceLauncher\voicelauncher.db"

if not os.path.exists(DB_PATH):
    print("DB_NOT_FOUND:", DB_PATH)
    sys.exit(2)

con = sqlite3.connect(DB_PATH)
cur = con.cursor()

tables = [r[0] for r in cur.execute("SELECT name FROM sqlite_master WHERE type='table'").fetchall()]

# substrings to look for (case-insensitive checks for words, raw checks for symbols/backslashes)
substrs = [
    'shrug',    # explicit word
    'ascii',    # ascii art
    'asky',     # possible misspelling
    'ツ',       # Japanese 'tsu' used in shrug art
    '¯',        # macron used in shrug art
    '\\_(',    # literal backslash + underscore + '('
    '\\_\\(',# double-escaped variant
    '\\_/',    # backslash + underscore + slash (variant)
    '/¯',       # trailing part
    '_/¯',      # variant
]

found = []
for t in tables:
    try:
        cols_info = con.execute(f"PRAGMA table_info('{t}')").fetchall()
        col_names = [c[1] for c in cols_info]
        rows = con.execute(f"SELECT rowid, * FROM '{t}'").fetchall()
    except Exception:
        continue

    for row in rows:
        rowid = row[0]
        cols = row[1:]
        for i, val in enumerate(cols):
            if val is None:
                continue
            if isinstance(val, (bytes, bytearray)):
                try:
                    s = val.decode('utf-8', errors='replace')
                except Exception:
                    s = repr(val)
            else:
                s = str(val)

            s_lower = s.lower()
            for sub in substrs:
                try:
                    if (sub.lower() in s_lower) or (sub in s):
                        colname = col_names[i] if i < len(col_names) else f"col{i}"
                        print(f"TABLE={t} ROWID={rowid} COLUMN={colname} MATCH={sub}")
                        print(s)
                        print("----")
                        found.append((t, rowid, colname, sub, s))
                        break
                except Exception:
                    continue

if not found:
    print("NO_MATCHES")

con.close()
