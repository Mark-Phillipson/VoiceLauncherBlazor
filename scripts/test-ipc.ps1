# scripts/test-ipc.ps1
<#
Simple integration test for WinFormsApp IPC.
Starts the app, sends search and launcher invocations, and checks logs for expected tokens.

Usage:
  powershell -ExecutionPolicy Bypass -File .\scripts\test-ipc.ps1
#>

param(
    [switch]$CloseOnExit,
    [int]$MaxInvocations = 4
)

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
if (-not ([System.Management.Automation.PSTypeName]'Win32.NativeMethods').Type) {
    Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
namespace Win32 {
    public static class NativeMethods {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
"@ -Language CSharp
}

function Wait-ForForegroundProcess([string]$processName, [int]$timeoutSec) {
    $deadline = (Get-Date).AddSeconds($timeoutSec)
    while ((Get-Date) -lt $deadline) {
        try {
            $fg = [Win32.NativeMethods]::GetForegroundWindow()
            if ($fg -ne [IntPtr]::Zero) {
                $p = Get-Process -Name $processName -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -eq $fg } | Select-Object -First 1
                if ($p) { return $true }
            }
        } catch { }
        Start-Sleep -Milliseconds 250
    }
    return $false
}

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
# Default categories to exercise in sequence. You can pass fewer by changing / calling with -MaxInvocations.
$categories = @('code projects', 'access projects', 'productivity tools', 'documents')

if ($MaxInvocations -lt 1) { $MaxInvocations = 1 }
if ($MaxInvocations -gt $categories.Length) { $MaxInvocations = $categories.Length }

Write-Info "Starting WinFormsApp with initial Launcher category: $($categories[0])"
$proc = Start-Process -FilePath $exe -ArgumentList @('Launcher', $categories[0]) -PassThru
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

# Run additional invocations (1..MaxInvocations-1) with per-invocation focus-steal and checks
Write-Info "Preparing to run up to $MaxInvocations total invocations (categories: $($categories[0..($MaxInvocations-1)] -join ', '))"

# Verify initial startup parsed entry
$startupCategory = $categories[0]
Write-Info "Verifying initial Launcher category was applied: '$startupCategory'"
$startupOk = $false
if (Wait-ForLogToken 'Index.StartupParsedIPC' 10) {
    try {
        $content = Get-Content $logPath -Raw -ErrorAction Stop
        $matches = [regex]::Matches($content, 'Index\.StartupParsedIPC:\s*(.+)$', [System.Text.RegularExpressions.RegexOptions]::Multiline)
        if ($matches.Count -gt 0) {
            $argsStr = $matches[$matches.Count - 1].Groups[1].Value.Trim()
            $parts = $argsStr -split '\|'
            if ($parts.Length -ge 3) {
                $categoryParts = $parts[2..($parts.Length - 1)]
                $startupCategoryFound = ($categoryParts -join ' ').Trim()
                if ($startupCategoryFound -ieq $startupCategory) {
                    Write-Info "Initial Launcher token found in startup log as: '$startupCategoryFound'"
                    $startupOk = $true
                } else {
                    Write-Err "Startup category mismatch: '$startupCategoryFound' vs expected '$startupCategory'"
                }
            } else {
                Write-Err "Startup parsed args did not include a Launcher category"
            }
        } else {
            Write-Err "Could not parse Index.StartupParsedIPC line"
        }
    } catch {
        Write-Err "Error reading startup parsed log: $_"
    }
}
if (-not $startupOk) {
    if (Wait-ForLogToken $startupCategory 10) { Write-Info "Initial Launcher token found in logs."; $startupOk = $true } else { Write-Err "Initial Launcher token NOT found in logs." }
}
if (-not $startupOk) { $allPassed = $false }

# Start a Notepad instance we'll use as a focus-stealer for subsequent invocations (reuse across iterations)
$focusStealer = $null
try {
    Write-Info "Starting Notepad to act as focus-stealer for subsequent invocations..."
    $focusStealer = Start-Process -FilePath notepad.exe -PassThru
    $deadline = (Get-Date).AddSeconds(5)
    while ((Get-Date) -lt $deadline) {
        $focusStealer.Refresh()
        if ($focusStealer.MainWindowHandle -ne 0) { break }
        Start-Sleep -Milliseconds 200
    }
} catch {
    Write-Err "Failed to start Notepad focus stealer: $_"
}

for ($i = 1; $i -lt $MaxInvocations; $i++) {
    $category = $categories[$i]
    Write-Info "Iteration ${i}: will send Launcher '$category'"
    Start-Sleep -Seconds 2

    # Steal focus to Notepad (so the app must regain it)
    try {
        $stealerHandle = [IntPtr]::Zero
        if ($focusStealer -ne $null) {
            $focusStealer.Refresh()
            if ($focusStealer.MainWindowHandle -ne 0) { $stealerHandle = [IntPtr]$focusStealer.MainWindowHandle }
        }
        if ($stealerHandle -eq [IntPtr]::Zero) {
            Write-Info "Notepad handle not available; starting a new Notepad instance for focus steal."
            $tmp = Start-Process -FilePath notepad.exe -PassThru
            $deadline = (Get-Date).AddSeconds(5)
            while ((Get-Date) -lt $deadline) {
                $tmp.Refresh()
                if ($tmp.MainWindowHandle -ne 0) { $stealerHandle = [IntPtr]$tmp.MainWindowHandle; break }
                Start-Sleep -Milliseconds 200
            }
            if ($stealerHandle -ne [IntPtr]::Zero) { $focusStealer = $tmp }
        }
        if ($stealerHandle -ne [IntPtr]::Zero) { [Win32.NativeMethods]::SetForegroundWindow($stealerHandle) | Out-Null }
        if (Wait-ForForegroundProcess 'notepad' 5) { Write-Info "Focus moved away from WinForms to Notepad." } else { Write-Info "Could not verify focus moved to Notepad (Windows focus policy); continuing." }
    } catch {
        Write-Err "Failed to steal focus before invocation ${i}: $_"
        $allPassed = $false
    }

    # Simulate launcher invocation; include an extra arg on the second overall invocation to mimic Talon behavior
    $extraArg = $null
    if ($i -eq 1) { $extraArg = 'admin' }

    if ($extraArg) { Write-Info "Sending: Launcher $category $extraArg"; & $exe Launcher $category $extraArg } else { Write-Info "Sending: Launcher $category"; & $exe Launcher $category }

    if (Wait-ForLogToken $category 10) { Write-Info "Invocation ${i}: Launcher token '$category' found in logs." } else { Write-Err "Invocation ${i}: Launcher token '$category' NOT found in logs."; $allPassed = $false }

    if (Wait-ForForegroundProcess 'WinFormsApp' 10) { Write-Info "Invocation ${i}: WinForms regained foreground focus." } else { Write-Err "Invocation ${i}: WinForms did not regain foreground focus."; $allPassed = $false }

    Capture-WindowScreenshot ("invocation_$i")

    if (Test-Path $logPath) {
        $content = Get-Content $logPath -Raw
        $parsedMatch = [regex]::Match($content, ("Index\.ParsedIPC.*" + [regex]::Escape($category)))
        if ($parsedMatch.Success) {
            Write-Info "Invocation ${i}: Index parsed '$category'."
            $after = $content.Substring($parsedMatch.Index + $parsedMatch.Length)
            if ($after -match 'Index\.ViewChanged: Launcher') { Write-Info "Invocation ${i}: Launcher view shown after parsed IPC." } else { Write-Err "Invocation ${i}: Launcher view NOT shown after parsed IPC."; $allPassed = $false }
        } else { Write-Err "Invocation ${i}: Index did not parse launcher token for '$category'."; $allPassed = $false }

        if ($extraArg -and ($content -match [regex]::Escape("$category $extraArg"))) { Write-Err "Invocation ${i}: Extra arg '$extraArg' contaminated category name in logs."; $allPassed = $false } else { if ($extraArg) { Write-Info "Invocation ${i}: Extra arg excluded from category name as expected." } }
    }
}

# Cleanup
if (-not $CloseOnExit) {
    Write-Info "Keeping WinFormsApp open for manual inspection (KeepOpen=True)."
    Write-Info "When done, close it manually or run: Stop-Process -Name WinFormsApp -Force"
} else {
    Write-Info "Stopping app..."
    try {
        Get-Process -Name WinFormsApp -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    } catch { }
}

try {
    if ($focusStealer) {
        Stop-Process -Id $focusStealer.Id -Force -ErrorAction SilentlyContinue
    }
} catch { }

if ($allPassed) {
    Write-Host "`nRESULT: PASS" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`nRESULT: FAIL" -ForegroundColor Red
    exit 1
}
