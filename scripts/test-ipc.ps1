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

# Screenshot helper: captures the WinFormsApp window if available, otherwise full screen
Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
namespace Win32 {
    public static class NativeMethods {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    }
}
"@ -Language CSharp

function Capture-WindowScreenshot([string]$label) {
    try {
        $screensDir = Join-Path $exeDir 'logs\screenshots'
        New-Item -ItemType Directory -Path $screensDir -Force | Out-Null
        $out = Join-Path $screensDir ("{0}_{1}.png" -f (Get-Date -Format 'yyyyMMdd_HHmmss'), $label)
        $p = Get-Process -Name WinFormsApp -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($p -and $p.MainWindowHandle -ne 0) {
            $h = $p.MainWindowHandle
            $rect = New-Object Win32.NativeMethods+RECT
            $ok = [Win32.NativeMethods]::GetWindowRect([IntPtr]$h, [ref]$rect)
            if ($ok) {
                Add-Type -AssemblyName System.Drawing
                $width = $rect.Right - $rect.Left
                $height = $rect.Bottom - $rect.Top
                if ($width -gt 0 -and $height -gt 0) {
                    $bmp = New-Object System.Drawing.Bitmap($width, $height)
                    $g = [System.Drawing.Graphics]::FromImage($bmp)
                    $g.CopyFromScreen($rect.Left, $rect.Top, 0, 0, $bmp.Size)
                    $bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
                    $g.Dispose(); $bmp.Dispose()
                    Write-Info "Captured window screenshot to $out"
                    return $out
                }
            }
        }

        # Fallback: capture full virtual screen
        Add-Type -AssemblyName System.Drawing
        Add-Type -AssemblyName System.Windows.Forms
        $bounds = [System.Windows.Forms.SystemInformation]::VirtualScreen
        $bmp = New-Object System.Drawing.Bitmap($bounds.Width, $bounds.Height)
        $g = [System.Drawing.Graphics]::FromImage($bmp)
        $g.CopyFromScreen($bounds.Left, $bounds.Top, 0, 0, $bmp.Size)
        $bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
        $g.Dispose(); $bmp.Dispose()
        Write-Info "Captured full-screen screenshot to $out"
        return $out
    } catch {
        Write-Err "Screenshot failed: $_"
        return $null
    }
}

# Start application instance with an initial Launcher command (simulates first cold-start voice)
$initialCategory = 'access projects'
$secondCategory = 'code projects'

# Simulate the real Talon call: args=['Launcher', 'code projects', 'admin']
# The extra 'admin' arg must NOT be merged into the category name
$secondCategoryWithExtra = $secondCategory
$secondExtraArg = 'admin'

Write-Info "Starting WinFormsApp with initial Launcher category: $initialCategory"
$proc = Start-Process -FilePath $exe -ArgumentList @('Launcher', $initialCategory) -PassThru
# Give Blazor some time to initialize
Start-Sleep -Seconds 1

# Take a screenshot of the form after startup
Write-Info "Capturing startup screenshot..."
Capture-WindowScreenshot 'startup'

# Wait until Index component subscribes (written as a persistent ipc.log entry by the app), up to 20s
Write-Info "Waiting for Index component to subscribe to LaunchArgumentsReceived..."
if (-not (Wait-ForLogToken 'Index.SubscribedToLaunchArguments' 20)) {
    Write-Err "Index did not signal subscription within timeout; continuing but parsed entries may be missed."
}

$allPassed = $true

Write-Info "Verifying initial Launcher category was applied: '$initialCategory'"
if (Wait-ForLogToken 'Index.StartupParsedIPC' 10) {
    # Read the latest startup parsed line and reconstruct the category (handles space-split tokens)
    try {
        $content = Get-Content $logPath -Raw -ErrorAction Stop
        $matches = [regex]::Matches($content, 'Index\.StartupParsedIPC:\s*(.+)$', [System.Text.RegularExpressions.RegexOptions]::Multiline)
        if ($matches.Count -gt 0) {
            $argsStr = $matches[$matches.Count - 1].Groups[1].Value.Trim()
            $parts = $argsStr -split '\|'
            if ($parts.Length -ge 3) {
                $categoryParts = $parts[2..($parts.Length - 1)]
                $startupCategory = ($categoryParts -join ' ').Trim()
                if ($startupCategory -ieq $initialCategory) {
                    Write-Info "Initial Launcher token found in startup log as: '$startupCategory'"
                } else {
                    Write-Err "Startup category mismatch: '$startupCategory' vs expected '$initialCategory'"
                    $allPassed = $false
                }
            } else {
                Write-Err "Startup parsed args did not include a Launcher category"
                $allPassed = $false
            }
        } else {
            Write-Err "Could not parse Index.StartupParsedIPC line"
            $allPassed = $false
        }
    } catch {
        Write-Err "Error reading startup parsed log: $_"
        $allPassed = $false
    }
} else {
    # Fallback: direct token match
    if (Wait-ForLogToken $initialCategory 10) {
        Write-Info "Initial Launcher token found in logs."
    } else {
        Write-Err "Initial Launcher token NOT found in logs."
        $allPassed = $false
    }
}

# Now simulate the second voice invocation while the app is running
Write-Info "Waiting 2s before re-launching to simulate user pause..."
Start-Sleep -Seconds 2

# Pass three args matching the real Talon call: Launcher 'code projects' admin
Write-Info "Re-launching app to send Launcher '$secondCategory' (+ extra arg '$secondExtraArg') - should be forwarded to running instance"
& $exe Launcher $secondCategoryWithExtra $secondExtraArg

if (Wait-ForLogToken $secondCategory 10) {
    Write-Info "Second Launcher token found in logs."
} else {
    Write-Err "Second Launcher token NOT found in logs."
    $allPassed = $false
}

# Capture a screenshot after the forwarded IPC arrives (or attempt regardless)
Write-Info "Capturing forwarded-invocation screenshot..."
Capture-WindowScreenshot 'forwarded'

# Additional checks: ensure Index recorded both launcher entries (startup and forwarded)
if (Test-Path $logPath) {
    $content = Get-Content $logPath -Raw
    if ($content -match 'Index\.StartupParsedIPC') { Write-Info "Index recorded a startup parsed line." } else { Write-Err "Index did not record a startup parsed line." ; $allPassed = $false }

    if ($content -match 'Index\.ViewChanged: Launcher') { Write-Info "Index changed view to Launcher on startup." } else { Write-Err "Index did NOT show Launcher view on startup." ; $allPassed = $false }

    if ($content -match ("Index\.ParsedIPC.*" + [regex]::Escape($secondCategory))) { Write-Info "Index parsed second launcher token." } else { Write-Err "Index did not parse second launcher token." ; $allPassed = $false }

    # Verify the extra 'admin' arg did NOT contaminate the category lookup
    if ($content -match 'Index\.ViewChanged: Launcher') { Write-Info "Launcher view shown after forwarded IPC." } else { Write-Err "Launcher view NOT shown after forwarded IPC." ; $allPassed = $false }

    # Confirm 'code projects admin' never appeared as a category (the bug we fixed)
    if ($content -notmatch [regex]::Escape("$secondCategory $secondExtraArg")) {
        Write-Info "Extra arg '$secondExtraArg' correctly excluded from category name."
    } else {
        Write-Err "Bug regressed: '$secondCategory $secondExtraArg' was used as category."
        $allPassed = $false
    }
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
