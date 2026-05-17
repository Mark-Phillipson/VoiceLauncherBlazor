$matches = Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -and $_.CommandLine -like '*VoiceAdmin*' }
if ($matches) {
    foreach ($m in $matches) {
        Write-Output "Found PID $($m.ProcessId) Name $($m.Name)"
        try {
            Stop-Process -Id $m.ProcessId -Force -ErrorAction SilentlyContinue
            Write-Output "Stopped PID $($m.ProcessId)"
        }
        catch {
            Write-Output "Failed to stop PID $($m.ProcessId): $_"
        }
    }
}
else {
    Write-Output 'No VoiceAdmin processes found by commandline'
}
