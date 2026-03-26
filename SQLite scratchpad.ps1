# 1) Path to generated DB
$db = 'C:\Users\MPhil\source\repos\VoiceLauncherBlazor\voicelauncher-azure.db'

# 2) Check existence
if (-not (Test-Path $db)) {
    Write-Host "Error: DB not found at $db"
    exit 1
}
Write-Host "DB found at $db"

# 3) List tables (using sqlite3 if installed)
if (Get-Command sqlite3 -ErrorAction SilentlyContinue) {
    sqlite3 $db "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;"
} else {
    Write-Host "sqlite3 not installed in path, skipping table list."
}

# 4) Show first 10 rows from key sample tables
if (Get-Command sqlite3 -ErrorAction SilentlyContinue) {
    Write-Host "`n== Sample: Transactions (10 rows) ==" 
    sqlite3 $db "SELECT Id, Date, Description, MoneyIn, MoneyOut, Balance FROM Transaction LIMIT 10;"
    Write-Host "`n== Sample: Categories (10 rows) =="
    sqlite3 $db "SELECT Id, CategoryName, CategoryType, Sensitive FROM Categories LIMIT 10;"
    Write-Host "`n== Sample: Logins (10 rows) =="
    sqlite3 $db "SELECT Id, Name, Username, Description FROM Logins LIMIT 10;"
} else {
    Write-Host "Install sqlite3 CLI to run these selects, or use a VS Code extension (instructions below)."
}# 1) Path to generated DB
$db = 'C:\Users\MPhil\source\repos\VoiceLauncherBlazor\voicelauncher-azure.db'

# 2) Check existence
if (-not (Test-Path $db)) {
    Write-Host "Error: DB not found at $db"
    exit 1
}
Write-Host "DB found at $db"

# 3) List tables (using sqlite3 if installed)
if (Get-Command sqlite3 -ErrorAction SilentlyContinue) {
    sqlite3 $db "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;"
} else {
    Write-Host "sqlite3 not installed in path, skipping table list."
}

# 4) Show first 10 rows from key sample tables
if (Get-Command sqlite3 -ErrorAction SilentlyContinue) {
    Write-Host "`n== Sample: Transactions (10 rows) ==" 
    sqlite3 $db "SELECT Id, Date, Description, MoneyIn, MoneyOut, Balance FROM Transaction LIMIT 10;"
    Write-Host "`n== Sample: Categories (10 rows) =="
    sqlite3 $db "SELECT Id, CategoryName, CategoryType, Sensitive FROM Categories LIMIT 10;"
    Write-Host "`n== Sample: Logins (10 rows) =="
    sqlite3 $db "SELECT Id, Name, Username, Description FROM Logins LIMIT 10;"
} else {
    Write-Host "Install sqlite3 CLI to run these selects, or use a VS Code extension (instructions below)."
}