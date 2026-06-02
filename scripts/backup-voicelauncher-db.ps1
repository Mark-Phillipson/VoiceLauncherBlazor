$ErrorActionPreference = 'Stop'

# Source DB in %APPDATA%\VoiceLauncher\voicelauncher.db
$src = Join-Path $env:APPDATA 'VoiceLauncher\voicelauncher.db'
if (-not (Test-Path $src)) {
    Write-Output "NOT_FOUND:$src"
    exit 2
}

# Timestamped destination in repository backups folder
$ts = Get-Date -Format 'yyyyMMdd-HHmmss'
$destDir = Join-Path $PSScriptRoot 'backups'
New-Item -ItemType Directory -Force -Path $destDir | Out-Null
$dest = Join-Path $destDir ("voicelauncher-$ts.db")

try {
    Copy-Item -Path $src -Destination $dest -Force -ErrorAction Stop
    Write-Output "COPIED:$dest"
    exit 0
} catch {
    Write-Output "ERROR:$($_.Exception.Message)"
    exit 1
}