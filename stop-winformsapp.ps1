<#
.SYNOPSIS
Stops any running WinFormsApp processes before a build or publish.

.DESCRIPTION
This script attempts to close WinFormsApp gracefully first, then falls back
to terminating any remaining WinFormsApp processes.

.PARAMETER Force
If set, terminates matching processes immediately.

.EXAMPLE
.
\stop-winformsapp.ps1

.EXAMPLE
.
\stop-winformsapp.ps1 -Force
#>

param(
    [Parameter(Mandatory = $false)]
    [switch]$Force
)

$processes = Get-Process -Name WinFormsApp -ErrorAction SilentlyContinue

if (-not $processes) {
    Write-Host "No WinFormsApp processes found."
    exit 0
}

foreach ($proc in $processes) {
    Write-Host "Stopping PID $($proc.Id) : $($proc.ProcessName)"

    try {
        if ($Force) {
            Stop-Process -Id $proc.Id -Force -ErrorAction Stop
            continue
        }

        if ($proc.MainWindowHandle -ne 0) {
            $proc.CloseMainWindow() | Out-Null
            Start-Sleep -Seconds 3
            $proc.Refresh()
        }

        if (-not $proc.HasExited) {
            Stop-Process -Id $proc.Id -ErrorAction Stop
        }
    }
    catch {
        Write-Warning "Failed to stop PID $($proc.Id): $($_.Exception.Message)"
        if (-not $Force) {
            Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
        }
    }
}

Write-Host "WinFormsApp shutdown complete."
