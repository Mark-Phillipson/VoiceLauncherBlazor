#!/usr/bin/env python3
import sqlite3
import os
import sys

DB_PATH = r"C:\Users\MPhil\AppData\Roaming\VoiceLauncher\voicelauncher.db"
ROWID = 427488

if not os.path.exists(DB_PATH):
    print("DB_NOT_FOUND:", DB_PATH)
    sys.exit(2)

con = sqlite3.connect(DB_PATH)
con.row_factory = sqlite3.Row
cur = con.cursor()

try:
    row = cur.execute("SELECT rowid, * FROM TalonVoiceCommands WHERE rowid = ?", (ROWID,)).fetchone()
    if not row:
        print("ROW_NOT_FOUND:", ROWID)
        sys.exit(1)
    for k in row.keys():
        v = row[k]
        if isinstance(v, (bytes, bytearray)):
            try:
                sv = v.decode('utf-8', errors='replace')
            except Exception:
                sv = repr(v)
        else:
            sv = "None" if v is None else str(v)
        print(f"{k}: {sv}")
except Exception as e:
    print("ERROR:", e)
finally:
    con.close()
