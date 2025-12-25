param(
    [Parameter(Mandatory=$true)]
    [int]$PortNumber
)

try {
    $connection = Get-NetTCPConnection -LocalPort $PortNumber -ErrorAction SilentlyContinue
    if ($connection) {
        Write-Host "Port $PortNumber is already in use. Stopping processes..."
        $processes = $connection | Select-Object -ExpandProperty OwningProcess -Unique
        foreach ($pid in $processes) {
            $proc = Get-Process -Id $pid -ErrorAction SilentlyContinue
            if ($proc) {
                Write-Host "Stopping process $($proc.Name) (PID: $pid)"
                Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
            }
        }
        Start-Sleep -Seconds 2
        Write-Host "Port $PortNumber is now free. Proceeding with launch."
        exit 0
    } else {
        Write-Host "Port $PortNumber is free. Proceeding with launch."
        exit 0 # Task succeeds
    }
}
catch {
    Write-Error ("Error checking port ${PortNumber}: " + $_.Exception.Message)
    exit 1 # Assume failure on error
}