# Test script to validate the talon import fix
Write-Host "Testing talon import fix..."

# Build the project first
Write-Host "Building project..."
dotnet build --configuration Debug

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Test the import with the global.talon file specifically
Write-Host "Testing import of global.talon file..."
$globalTalonPath = "C:\Users\MPhil\AppData\Roaming\talon\user\mystuff\talon_my_stuff\core\global.talon"

if (Test-Path $globalTalonPath) {
    Write-Host "Found global.talon at: $globalTalonPath" -ForegroundColor Green
    
    # Check if the file contains our target command
    $content = Get-Content $globalTalonPath
    $hasTargetCommand = $content | Where-Object { $_ -like "*talon lists show*" }
    
    if ($hasTargetCommand) {
        Write-Host "✓ Target command 'talon lists show' found in file" -ForegroundColor Green
        Write-Host "Command line: $hasTargetCommand" -ForegroundColor Cyan
    } else {
        Write-Host "✗ Target command 'talon lists show' not found in file" -ForegroundColor Red
    }
} else {
    Write-Host "Global.talon file not found at expected location" -ForegroundColor Red
}

Write-Host "Ready to test import. Run the following command to import and check:"
Write-Host "cd VoiceLauncher; dotnet run" -ForegroundColor Yellow
