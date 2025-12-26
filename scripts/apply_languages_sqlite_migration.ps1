# Apply Languages migration to local SQLite DB
$db = Join-Path $env:APPDATA 'VoiceLauncher\voicelauncher.db'
if (-not (Test-Path $db)) { Write-Error "DB_NOT_FOUND: $db"; exit 1 }
$ts = Get-Date -Format 'yyyyMMddHHmmss'
$bak = "$db.bak_$ts"
Copy-Item -Path $db -Destination $bak -Force
Write-Output "Backed up DB to: $bak"

function RunSql($sql) {
    Write-Output "SQL> $sql"
    $out = & sqlite3 $db $sql 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Output "ERR: $out"
        return $null
    }
    return $out
}

# Ensure Language column exists and has no NULLs
$hasLanguage = (& sqlite3 $db "SELECT COUNT(*) FROM pragma_table_info('Languages') WHERE name='Language';").Trim()
if ($hasLanguage -eq '0') {
    RunSql "ALTER TABLE Languages ADD COLUMN Language TEXT;"
    RunSql "UPDATE Languages SET Language = '' WHERE Language IS NULL;"
    Write-Output "Added Language column and set NULLs to empty string."
} else {
    RunSql "UPDATE Languages SET Language = '' WHERE Language IS NULL;"
    Write-Output "Ensured no NULL Language values."
}

# Add Colour column if missing
$hasColour = (& sqlite3 $db "SELECT COUNT(*) FROM pragma_table_info('Languages') WHERE name='Colour';").Trim()
if ($hasColour -eq '0') {
    RunSql "ALTER TABLE Languages ADD COLUMN Colour TEXT;"
    Write-Output "Added Colour column."
} else { Write-Output "Colour column already exists." }

# Add ImageLink column if missing
$hasImageLink = (& sqlite3 $db "SELECT COUNT(*) FROM pragma_table_info('Languages') WHERE name='ImageLink';").Trim()
if ($hasImageLink -eq '0') {
    RunSql "ALTER TABLE Languages ADD COLUMN ImageLink TEXT;"
    Write-Output "Added ImageLink column."
} else { Write-Output "ImageLink column already exists." }

# Check duplicates
$dup = (& sqlite3 $db "SELECT Language || '|' || COUNT(*) FROM Languages GROUP BY Language HAVING COUNT(*) > 1;").Trim()
if ([string]::IsNullOrWhiteSpace($dup)) {
    RunSql "CREATE UNIQUE INDEX IF NOT EXISTS UX_Languages_Language ON Languages(Language);"
    Write-Output "Unique index UX_Languages_Language created (if not present)."
} else {
    Write-Output "Duplicates found (skipping unique index). Sample duplicates:"
    Write-Output $dup
}

# Summary checks
$total = (& sqlite3 $db "SELECT COUNT(*) FROM Languages;").Trim()
$emptyNames = (& sqlite3 $db "SELECT COUNT(*) FROM Languages WHERE Language = '';").Trim()
Write-Output "Languages total rows: $total"
Write-Output "Languages with empty Language value: $emptyNames"
Write-Output "Migration script completed."