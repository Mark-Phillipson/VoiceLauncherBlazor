#!/usr/bin/env python3
import sqlite3
import os
import re
import sys

DB_PATH = r"C:\Users\MPhil\AppData\Roaming\VoiceLauncher\voicelauncher.db"

if not os.path.exists(DB_PATH):
    print("DB_NOT_FOUND:", DB_PATH)
    sys.exit(2)

con = sqlite3.connect(DB_PATH)
cur = con.cursor()

tables = [r[0] for r in cur.execute("SELECT name FROM sqlite_master WHERE type='table'").fetchall()]
print("TABLES:", ','.join(tables))

patterns = [r"shrug", r"ascii", r"asky", r"¯", r"\\_\\(|\\_\\)", r"ツ"]
regex = re.compile("|".join(patterns), re.IGNORECASE)

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
            if isinstance(val, str) and regex.search(val):
                colname = col_names[i] if i < len(col_names) else f"col{i}"
                print(f"TABLE={t} ROWID={rowid} COLUMN={colname}")
                # Print safely to avoid console encoding errors
                try:
                    print(val)
                except UnicodeEncodeError:
                    safe_val = val.encode('utf-8', errors='replace').decode('utf-8')
                    print(safe_val)
                print("----")
                found.append((t, rowid, colname, val))

if not found:
    print("NO_MATCHES")

con.close()
