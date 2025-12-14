# PowerShell script to run the Face Recognition migration
# This script executes the SQL migration script against the VoiceLauncher database

Write-Host "Face Recognition Migration Script" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
Write-Host ""

# Get the connection string from appsettings.json
$appsettingsPath = Join-Path $PSScriptRoot "VoiceAdmin\appsettings.json"
if (-not (Test-Path $appsettingsPath)) {
    Write-Host "Error: appsettings.json not found at $appsettingsPath" -ForegroundColor Red
    exit 1
}

$appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
$connectionString = $appsettings.ConnectionStrings.VoiceLauncher

if ([string]::IsNullOrEmpty($connectionString)) {
    Write-Host "Error: Connection string 'VoiceLauncher' not found in appsettings.json" -ForegroundColor Red
    exit 1
}

Write-Host "Found connection string" -ForegroundColor Green

# Path to the migration script
$migrationScriptPath = Join-Path $PSScriptRoot "DataAccessLibrary\Scripts\FaceRecognition-Migration-Script.sql"
if (-not (Test-Path $migrationScriptPath)) {
    Write-Host "Error: Migration script not found at $migrationScriptPath" -ForegroundColor Red
    exit 1
}

Write-Host "Found migration script" -ForegroundColor Green
Write-Host ""

# Ask for confirmation
Write-Host "This will execute the following script:" -ForegroundColor Yellow
Write-Host "  $migrationScriptPath" -ForegroundColor Yellow
Write-Host ""
Write-Host "Against the database specified in the connection string." -ForegroundColor Yellow
Write-Host ""
$confirmation = Read-Host "Do you want to continue? (Y/N)"

if ($confirmation -ne 'Y' -and $confirmation -ne 'y') {
    Write-Host "Migration cancelled." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Executing migration..." -ForegroundColor Green

# Execute the SQL script using Invoke-Sqlcmd
try {
    # Check if SqlServer module is available
    if (-not (Get-Module -ListAvailable -Name SqlServer)) {
        Write-Host "Error: SqlServer PowerShell module is not installed." -ForegroundColor Red
        Write-Host "Please install it using: Install-Module -Name SqlServer" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Alternative: Run the script manually in SQL Server Management Studio (SSMS)" -ForegroundColor Yellow
        Write-Host "Script location: $migrationScriptPath" -ForegroundColor Cyan
        exit 1
    }

    Import-Module SqlServer
    
    # Parse connection string to extract server and database
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($connectionString)
    $serverName = $builder['Data Source']
    $databaseName = $builder['Initial Catalog']
    
    Write-Host "Server: $serverName" -ForegroundColor Cyan
    Write-Host "Database: $databaseName" -ForegroundColor Cyan
    Write-Host ""
    
    # Execute the script
    Invoke-Sqlcmd -ServerInstance $serverName -Database $databaseName -InputFile $migrationScriptPath -Verbose
    
    Write-Host ""
    Write-Host "Migration completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "You can now use the Face Recognition feature in VoiceAdmin." -ForegroundColor Green
    Write-Host "Navigate to: http://localhost:5008/face-recognition" -ForegroundColor Cyan
}
catch {
    Write-Host ""
    Write-Host "Error executing migration:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Alternative: Run the script manually in SQL Server Management Studio (SSMS)" -ForegroundColor Yellow
    Write-Host "Script location: $migrationScriptPath" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Steps:" -ForegroundColor Yellow
    Write-Host "1. Open SQL Server Management Studio" -ForegroundColor Yellow
    Write-Host "2. Connect to your SQL Server instance" -ForegroundColor Yellow
    Write-Host "3. Select the VoiceLauncher database" -ForegroundColor Yellow
    Write-Host "4. Open the migration script file" -ForegroundColor Yellow
    Write-Host "5. Execute the script (F5)" -ForegroundColor Yellow
    exit 1
}
