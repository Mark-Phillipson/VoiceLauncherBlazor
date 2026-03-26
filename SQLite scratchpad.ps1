# 1) Path to generated DB
$db = 'C:\Users\MPhil\source\repos\VoiceLauncherBlazor\voicelauncher-azure.db'

# 2) Check existence
if (-not (Test-Path $db)) {
    Write-Host "Error: DB not found at $db"
    exit 1
}
Write-Host "DB found at $db"

# 3) List tables and views (using sqlite3 if installed)
if (Get-Command sqlite3 -ErrorAction SilentlyContinue) {
    sqlite3 $db "SELECT name, type FROM sqlite_master WHERE type IN ('table','view') ORDER BY name;"
    Write-Host "`nNOTE: Use the exact table names from the list for your sample queries."
} else {
    Write-Host "sqlite3 not installed in path, skipping table list."
}

# 4) Show first 5 rows from sample table names (adjust names if needed)
if (Get-Command sqlite3 -ErrorAction SilentlyContinue) {
    Write-Host "`n== Sample: Transaction-like rows =="
    sqlite3 $db "SELECT Id, Date, Description, MoneyIn, MoneyOut, Balance FROM Transactions LIMIT 5;" 2>&1
    Write-Host "`n== Sample: Category-like rows =="
    sqlite3 $db "SELECT Id, CategoryName, CategoryType, Sensitive FROM Categories LIMIT 5;" 2>&1
    Write-Host "`n== Sample: Login-like rows =="
    sqlite3 $db "SELECT Id, Name, Username, Description FROM Logins LIMIT 5;" 2>&1
} else {
    Write-Host "Install sqlite3 CLI to run these selects, or use a VS Code extension (instructions below)."
}