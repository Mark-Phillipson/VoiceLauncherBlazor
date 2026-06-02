$client = New-Object System.IO.Pipes.NamedPipeClientStream('.', 'VoiceLauncherBlazor_LaunchArgs', [System.IO.Pipes.PipeDirection]::Out)
$client.Connect(1000)
$sw = New-Object System.IO.StreamWriter($client, [System.Text.Encoding]::UTF8)
$sw.AutoFlush = $true
$sw.WriteLine('Talon|search|integration-probe')
$sw.Dispose()
$client.Dispose()
Write-Host 'sent'