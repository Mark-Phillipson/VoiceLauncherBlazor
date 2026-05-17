Param(
    [int]$Port = 5000
)

Write-Output "Stopping processes bound to port $Port..."
$connections = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
if ($connections) {
    $pids = $connections | Select-Object -ExpandProperty OwningProcess -Unique
    foreach ($pid in $pids) {
        try {
            $proc = Get-Process -Id $pid -ErrorAction SilentlyContinue
            Write-Output "Found process: $($proc.ProcessName) (PID $pid) bound to port $Port"
            if ($proc -ne $null) {
                if ($proc.ProcessName -match 'dotnet' -or $proc.ProcessName -match 'VoiceAdmin') {
                    Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
                    Write-Output "Stopped PID $pid ($($proc.ProcessName))"
                }
                else {
                    Write-Output "Skipping PID $pid ($($proc.ProcessName))"
                }
            }
        }
        catch {
            Write-Output "Error handling PID ${pid}: $_"
        }
    }
}
else {
    Write-Output "No process bound to port $Port"
}

Write-Output "Stopping VBCSCompiler processes if any..."
Get-Process VBCSCompiler -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Output "Stopping VBCSCompiler PID $($_.Id)"
    Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
}

Write-Output "Done."
