# Test script for Talon search with command line arguments
param(
    [string]$SearchTerm = "open file"
)

Write-Host "Testing Talon search with term: '$SearchTerm'" -ForegroundColor Green

# Build the project first
Write-Host "Building project..." -ForegroundColor Yellow
cd "c:\Users\MPhil\source\repos\VoiceLauncherBlazor"
dotnet build --configuration Debug --verbosity minimal

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful. Launching application..." -ForegroundColor Green
    
    # Launch the application with search term
    cd "WinFormsApp"
    dotnet run --configuration Debug -- search $SearchTerm
} else {
    Write-Host "Build failed!" -ForegroundColor Red
}
