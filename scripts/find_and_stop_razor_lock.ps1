$target = 'C:\Users\MPhil\source\repos\VoiceLauncherBlazor\VoiceAdmin\bin\Debug\net10.0\RazorClassLibrary.dll'
Write-Host "Looking for processes referencing: $target"
$found = @()

# 1) Search command lines via CIM
try {
    $cim = Get-CimInstance Win32_Process -ErrorAction SilentlyContinue
    foreach ($p in $cim) {
        if ($p.CommandLine -and $p.CommandLine -like "*$([System.IO.Path]::GetFileName($target))*") {
            $found += @{Pid=$p.ProcessId; Name=$p.Name; Reason='CommandLine'; Cmd=$p.CommandLine}
        }
    }
}
catch {
    Write-Host "CIM query failed: $_"
}

# 2) Search process modules (may require elevation; ignore errors)
try {
    foreach ($proc in Get-Process -ErrorAction SilentlyContinue) {
        try {
            foreach ($m in $proc.Modules) {
                if ($m.FileName -and $m.FileName -like "*$([System.IO.Path]::GetFileName($target))") {
                    $found += @{Pid=$proc.Id; Name=$proc.ProcessName; Reason='Module'; Cmd=$proc.Path}
                    break
                }
            }
        }
        catch {
            # skip processes we cannot inspect
        }
    }
}
catch {
    Write-Host "Module scan failed: $_"
}

$found = $found | Sort-Object -Property Pid -Unique
if (-not $found) {
    Write-Host "No processes found referencing the file."
    exit 0
}

Write-Host "Processes locking or referencing the file:"
foreach ($f in $found) {
    Write-Host "PID=$($f.Pid) Name=$($f.Name) Reason=$($f.Reason)"
    if ($f.Cmd) { Write-Host "  CmdLine/Path: $($f.Cmd)" }
}

# Stop each found process (force)
foreach ($f in $found) {
    try {
        Write-Host "Stopping PID=$($f.Pid) Name=$($f.Name)..."
        Stop-Process -Id $f.Pid -Force -ErrorAction Stop
        Write-Host "Stopped PID=$($f.Pid)"
    }
    catch {
        Write-Host "Failed to stop PID=$($f.Pid) : $_"
    }
}

Write-Host 'Done.'
