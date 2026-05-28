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

q = '''SELECT rowid, Command, Script, FilePath
FROM TalonVoiceCommands
WHERE lower(Command) LIKE '%shrug%'
   OR lower(Script) LIKE '%shrug%'
   OR Command LIKE '%¯%'
   OR Script LIKE '%¯%'
   OR Command LIKE '%ツ%'
   OR Script LIKE '%ツ%'
   OR lower(Command) LIKE '%ascii%'
   OR lower(Script) LIKE '%ascii%'
   OR lower(Command) LIKE '%asky%'
   OR lower(Script) LIKE '%asky%'
'''

rows = cur.execute(q).fetchall()
print('FOUND:', len(rows))
for r in rows:
    rowid, cmd, script, path = r
    print(f'ROWID={rowid}')
    print('Command:', cmd)
    print('Script:', script)
    print('FilePath:', path)
    print('----')

con.close()
