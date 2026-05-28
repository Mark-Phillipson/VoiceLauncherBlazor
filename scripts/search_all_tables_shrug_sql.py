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
patterns = ["%shrug%", "%ascii%", "%asky%", "%¯%", "%ツ%"]

found = 0
for t in tables:
    try:
        cols_info = con.execute(f"PRAGMA table_info('{t}')").fetchall()
        col_names = [c[1] for c in cols_info]
    except Exception:
        continue
    for col in col_names:
        for pat in patterns:
            try:
                q = f"SELECT rowid, \"{col}\" FROM \"{t}\" WHERE lower(\"{col}\") LIKE ? ESCAPE '\\'"
                rows = con.execute(q, (pat.lower(),)).fetchall()
            except Exception:
                # try without lower() for binary/text mix
                try:
                    q2 = f"SELECT rowid, \"{col}\" FROM \"{t}\" WHERE \"{col}\" LIKE ? ESCAPE '\\'"
                    rows = con.execute(q2, (pat,)).fetchall()
                except Exception:
                    rows = []
            if rows:
                for r in rows:
                    rowid, val = r
                    try:
                        print(f"TABLE={t} ROWID={rowid} COLUMN={col}")
                        print(val)
                        print("----")
                    except Exception:
                        try:
                            sv = str(val).encode('utf-8', errors='replace').decode('utf-8')
                            print(f"TABLE={t} ROWID={rowid} COLUMN={col}")
                            print(sv)
                            print("----")
                        except Exception:
                            print(f"TABLE={t} ROWID={rowid} COLUMN={col} VALUE_UNPRINTABLE")
                    found += 1

if found == 0:
    print('NO_MATCHES')

con.close()
