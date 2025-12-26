# Backup sqlite DB and export Languages and CustomIntelliSense to CSV
$db = Join-Path $env:APPDATA 'VoiceLauncher\voicelauncher.db'
if (-not (Test-Path $db)) { Write-Error "DB_NOT_FOUND: $db"; exit 1 }
$ts = Get-Date -Format 'yyyyMMddHHmmss'
$bak = "$db.bak3_$ts"
Copy-Item -Path $db -Destination $bak -Force
Write-Output "Backed up DB to: $bak"

$csv1 = Join-Path $env:TEMP "languages_backup_$ts.csv"
$csv2 = Join-Path $env:TEMP "customintellisense_backup_$ts.csv"

& sqlite3 $db "headers on; mode csv; .output $csv1; SELECT * FROM Languages; .output stdout;"
Write-Output "Exported Languages to: $csv1"
& sqlite3 $db "headers on; mode csv; .output $csv2; SELECT * FROM CustomIntelliSense; .output stdout;"
Write-Output "Exported CustomIntelliSense to: $csv2"

Write-Output "Done."