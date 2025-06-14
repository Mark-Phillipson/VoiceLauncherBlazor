# Apply TalonList Migration Script
# This script applies the TalonList table migration to the database
# Run this from the DataAccessLibrary directory

Write-Host "Applying TalonList migration..." -ForegroundColor Green

# Change to the DataAccessLibrary directory
Set-Location "c:\Users\MPhil\source\repos\VoiceLauncherBlazor\DataAccessLibrary"

# Apply the migration
dotnet ef database update --configuration Debug

Write-Host "Migration applied successfully!" -ForegroundColor Green
Write-Host "The TalonList table is now available in the database." -ForegroundColor Yellow
