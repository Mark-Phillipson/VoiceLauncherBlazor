<#
.SYNOPSIS
Stops any processes using a specified port (defaults to 5008).

.DESCRIPTION
This script finds processes which have TCP and UDP endpoints bound to the specified local port
and attempts to stop them. It prefers a graceful shutdown where possible and falls back to
forceful termination if requested or needed.

.PARAMETER Port
The local port to check. Defaults to 5008.

.PARAMETER Force
If set, will use a forceful kill for each process. Otherwise it will attempt a graceful close first.

.EXAMPLE
# Stop processes using port 5008 (default)
.\stop-processes-on-port-5008.ps1

# Stop processes on a different port and force kill
.\stop-processes-on-port-5008.ps1 -Port 8080 -Force
#>

param(
    [Parameter(Mandatory=$false)]
    [int]
    $Port = 5008,

    [Parameter(Mandatory=$false)]
    [switch]
    $Force,

    [Parameter(Mandatory=$false)]
    [switch]
    $ListDotnet
)

# Check for Administrator privileges (not strictly required but recommended)
$IsAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $IsAdmin) {
    Write-Warning "Not running as Administrator. Some processes may fail to terminate. Run PowerShell as Administrator to ensure full privileges."
}

# If requested, list all running dotnet processes (basic + extended info)
if ($ListDotnet) {
    Write-Host "Listing running 'dotnet' processes..."
    $dotnetProcs = Get-Process -Name dotnet -ErrorAction SilentlyContinue
    if ($dotnetProcs) {
        foreach ($p in $dotnetProcs) {
            $start = $null
            try { $start = $p.StartTime } catch { $start = 'N/A' }
            Write-Host ("PID {0} - {1} (Start: {2})" -f $p.Id, $p.ProcessName, $start)
        }

        # Extended info (executable path and command line) via CIM
        $dotnetCim = Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" -ErrorAction SilentlyContinue
        if ($dotnetCim) {
            Write-Host "Detailed dotnet process info:"
            foreach ($c in $dotnetCim) {
                $exe = $c.ExecutablePath -or 'N/A'
                $cmd = $c.CommandLine -or 'N/A'
                Write-Host ("PID {0} - Path: {1} - CommandLine: {2}" -f $c.ProcessId, $exe, $cmd)
            }
        }
    }
    else {
        Write-Host "No 'dotnet' processes found."
    }

    # continue with the rest of the script after listing
}

$foundPids = @()

try {
    # Prefer modern NetTCP/NetUDP cmdlets when available
    if (Get-Command Get-NetTCPConnection -ErrorAction SilentlyContinue) {
        $tcp = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
        if ($tcp) { $foundPids += $tcp.OwningProcess }

        if (Get-Command Get-NetUDPEndpoint -ErrorAction SilentlyContinue) {
            $udp = Get-NetUDPEndpoint -LocalPort $Port -ErrorAction SilentlyContinue
            if ($udp) { $foundPids += $udp.OwningProcess }
        }
    }
    else {
        # Fallback: parse netstat output
        $netstat = netstat -aon 2>$null | Select-String ":${Port}\b"
        foreach ($line in $netstat) {
            # split by whitespace and get last token (PID)
            $parts = ($line -split '\s+') | Where-Object { $_ -ne '' }
            $pidStr = $parts[-1]
            if ($pidStr -match '^[0-9]+$') { $foundPids += [int]$pidStr }
        }
    }
}
catch {
    Write-Error "Failed to query network endpoints for port ${Port}: $_"
    exit 2
}

$foundPids = $foundPids | Where-Object { $_ -and $_ -ne 0 } | Sort-Object -Unique

if (-not $foundPids) {
    Write-Host "No processes found using port ${Port}."
    exit 0
}

Write-Host "Found process IDs listening/using port ${Port}: $($foundPids -join ', ')"

$killed = @()
$failed = @()

foreach ($procPid in $foundPids) {
    $proc = Get-Process -Id $procPid -ErrorAction SilentlyContinue
    if (-not $proc) {
        Write-Warning "PID $procPid not found (may have exited already)."
        continue
    }

    $procName = $proc.ProcessName
    $startTime = $null
    try { $startTime = $proc.StartTime } catch { $startTime = 'N/A' }

    Write-Host "Attempting to stop PID $procPid : $procName (StartTime: $startTime)"

    try {
        if ($Force) {
            Stop-Process -Id $procPid -Force -ErrorAction Stop
        }
        else {
            # If it has a main window try a polite close first
            try {
                if ($proc.MainWindowHandle -ne 0) {
                    $proc.CloseMainWindow() | Out-Null
                    Start-Sleep -Seconds 3
                    $proc.Refresh()
                    if (-not $proc.HasExited) {
                        Stop-Process -Id $procPid -ErrorAction Stop
                    }
                }
                else {
                    Stop-Process -Id $procPid -ErrorAction Stop
                }
            }
            catch {
                # fallback to force if polite stop fails
                Write-Verbose "Polite stop failed for PID $procPid, attempting force: $_"
                Stop-Process -Id $procPid -Force -ErrorAction Stop
            }
        }

        Write-Host "Stopped PID $procPid ($procName)"
        $killed += $procPid
    }
    catch {
        Write-Warning "Failed to stop PID $procPid ($procName): $($_.Exception.Message)"
        $failed += @{Pid=$procPid; Name=$procName; Error=$_.Exception.Message}
    }
}

Write-Host "`nSummary:`nStopped: $($killed -join ', ')"
if ($failed.Count -gt 0) {
    Write-Host "Failed to stop the following processes:"
    foreach ($f in $failed) { Write-Host " - PID $($f.Pid) : $($f.Name) -> $($f.Error)" }
    exit 1
}
else {
    exit 0
}
