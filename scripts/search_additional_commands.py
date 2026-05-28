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

try:
    cols_info = con.execute("PRAGMA table_info('AdditionalCommands')").fetchall()
    col_names = [c[1] for c in cols_info]
    print('COLUMNS:', col_names)
    rows = con.execute("SELECT rowid, * FROM AdditionalCommands WHERE SendKeys_Value LIKE '%ツ%' OR SendKeys_Value LIKE '%¯%' OR SendKeys_Value LIKE '%shrug%' OR Remarks LIKE '%shrug%' OR SendKeys_Value LIKE '%ascii%' OR Remarks LIKE '%ascii%' OR SendKeys_Value LIKE '%asky%' OR Remarks LIKE '%asky%'").fetchall()
    print('FOUND:', len(rows))
    for row in rows:
        rowid = row[0]
        vals = row[1:]
        print(f'ROWID={rowid}')
        for i, val in enumerate(vals):
            col = col_names[i] if i < len(col_names) else f'col{i}'
            print(f'  {col}: {val}')
        print('----')
except Exception as e:
    print('ERROR:', e)
finally:
    con.close()
