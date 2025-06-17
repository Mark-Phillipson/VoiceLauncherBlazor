# PowerShell script to apply database migration for new Talon context header columns
# This script adds CodeLanguage, Language, and Hostname columns to the TalonVoiceCommands table

# Set the script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$migrationScript = Join-Path $scriptDir "DataAccessLibrary\Scripts\AddNewContextHeaderColumns.sql"

# Database connection details
$serverName = "(localdb)\mssqllocaldb"
$databaseName = "VoiceLauncherDb"

Write-Host "Applying migration: Adding new context header columns (CodeLanguage, Language, Hostname)" -ForegroundColor Green
Write-Host "Server: $serverName" -ForegroundColor Yellow
Write-Host "Database: $databaseName" -ForegroundColor Yellow
Write-Host "Migration Script: $migrationScript" -ForegroundColor Yellow

try {
    # Check if the migration script exists
    if (-not (Test-Path $migrationScript)) {
        throw "Migration script not found at: $migrationScript"
    }

    # Execute the migration script
    Write-Host "Executing migration script..." -ForegroundColor Cyan
    
    $command = "sqlcmd -S `"$serverName`" -d `"$databaseName`" -i `"$migrationScript`""
    Write-Host "Command: $command" -ForegroundColor Gray
    
    # Copy the command to clipboard for easy pasting
    $command | Set-Clipboard
    Write-Host "Command copied to clipboard! You can paste and run it manually." -ForegroundColor Green
    
    Write-Host ""
    Write-Host "MANUAL EXECUTION REQUIRED:" -ForegroundColor Red
    Write-Host "Please run the following command in a command prompt or SQL Server Management Studio:" -ForegroundColor Yellow
    Write-Host $command -ForegroundColor White
    Write-Host ""
    Write-Host "Or execute the SQL script directly in SSMS:" -ForegroundColor Yellow
    Write-Host $migrationScript -ForegroundColor White
}
catch {
    Write-Error "Migration failed: $($_.Exception.Message)"
    exit 1
}

Write-Host ""
Write-Host "After running the migration, restart the Blazor application to use the new columns." -ForegroundColor Green
