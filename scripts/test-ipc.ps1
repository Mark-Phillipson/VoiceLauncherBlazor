# scripts/test-ipc.ps1
<#
Simple integration test for WinFormsApp IPC.
Starts the app, sends search and launcher invocations, and checks logs for expected tokens.

Usage:
  powershell -ExecutionPolicy Bypass -File .\scripts\test-ipc.ps1
#>

$ErrorActionPreference = 'Stop'

function Write-Info($msg) { Write-Host "[INFO] $msg" }
function Write-Err($msg) { Write-Host "[ERROR] $msg" -ForegroundColor Red }

# Resolve paths relative to this script folder
$exeRel = '..\WinFormsApp\bin\Release\net10.0-windows\WinFormsApp.exe'
try { $exe = Resolve-Path (Join-Path $PSScriptRoot $exeRel) -ErrorAction Stop; $exe = $exe.Path } catch { Write-Err "Could not find WinForms executable at $exeRel"; exit 2 }
$exeDir = Split-Path $exe -Parent
$logPath = Join-Path $exeDir 'logs\ipc.log'

Write-Info "Executable: $exe"
Write-Info "Log: $logPath"

# Stop any running instance
Write-Info "Stopping existing WinFormsApp processes (if any)..."
Get-Process -Name WinFormsApp -ErrorAction SilentlyContinue | ForEach-Object {
    try { Stop-Process -Id $_.Id -Force -ErrorAction Stop; Write-Info "Stopped PID $($_.Id)" }
    catch { Write-Err "Failed to stop PID $($_.Id): $_" }
}

# Remove old log
if (Test-Path $logPath) {
    try { Remove-Item $logPath -Force; Write-Info "Removed old log" } catch { Write-Err "Failed to remove old log: $_" }
}

# Helper: wait up to timeoutSec for token to appear in ipc.log
function Wait-ForLogToken([string]$token, [int]$timeoutSec) {
    $deadline = (Get-Date).AddSeconds($timeoutSec)
    while ((Get-Date) -lt $deadline) {
        if (Test-Path $logPath) {
            try {
                $content = Get-Content $logPath -Raw -ErrorAction Stop
                if ($content -match [regex]::Escape($token)) { return $true }
            } catch { }
        }
        Start-Sleep -Milliseconds 500
    }
    return $false
}

# Start application instance
Write-Info "Starting WinFormsApp..."
$proc = Start-Process -FilePath $exe -PassThru
# Wait for UI/blazor to initialize so the Index component can subscribe to IPC events
Start-Sleep -Seconds 1

# Wait until Index component subscribes (written as a persistent ipc.log entry by the app), up to 20s
Write-Info "Waiting for Index component to subscribe to LaunchArgumentsReceived..."
if (-not (Wait-ForLogToken 'Index.SubscribedToLaunchArguments' 20)) {
    Write-Err "Index did not signal subscription within timeout; continuing but parsed entries may be missed."
}

$allPassed = $true

# Test A: Talon search dispatch
$token = "TEST_IPC_SEARCH_$(([int](Get-Random -Maximum 1000000)))"
Write-Info "Sending Talon search token: $token"
& $exe Talon $token

if (Wait-ForLogToken $token 10) {
    Write-Info "Search IPC token found in logs."
} else {
    Write-Err "Search IPC token NOT found in logs."
    $allPassed = $false
}

# Test B: Launcher dispatch
$category = "TestCategory$(([int](Get-Random -Maximum 1000000)))"
$token2 = $category
Write-Info "Sending Launcher category: $category"
& $exe Launcher $category

if (Wait-ForLogToken $token2 10) {
    Write-Info "Launcher token found in logs."
} else {
    Write-Err "Launcher token NOT found in logs."
    $allPassed = $false
}

# Additional checks: look for Index.ParsedIPC entries for tokens
if (Test-Path $logPath) {
    $content = Get-Content $logPath -Raw
    if ($content -match "Index.ParsedIPC.*$token") { Write-Info "Index parsed search token." } else { Write-Err "Index did not parse search token." ; $allPassed = $false }
    if ($content -match "Index.ParsedIPC.*$token2") { Write-Info "Index parsed launcher token." } else { Write-Err "Index did not parse launcher token." ; $allPassed = $false }
}

# Cleanup
Write-Info "Stopping app..."
try {
    Get-Process -Name WinFormsApp -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
} catch { }

if ($allPassed) {
    Write-Host "`nRESULT: PASS" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`nRESULT: FAIL" -ForegroundColor Red
    exit 1
}
