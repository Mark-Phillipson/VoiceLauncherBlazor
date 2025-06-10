param(
    [Parameter(Mandatory=$true)]
    [int]$PortNumber
)

try {
    $connection = Get-NetTCPConnection -LocalPort $PortNumber -ErrorAction SilentlyContinue
    if ($connection) {
        Write-Host "Port $PortNumber is already in use. The application might be running."
        Write-Host "To stop this check from preventing a launch, ensure no other instance is using this port."
        exit 1 # Cause the task to fail
    } else {
        Write-Host "Port $PortNumber is free. Proceeding with launch."
        exit 0 # Task succeeds
    }
}
catch {
    Write-Error ("Error checking port ${PortNumber}: " + $_.Exception.Message)
    exit 1 # Assume failure on error
}