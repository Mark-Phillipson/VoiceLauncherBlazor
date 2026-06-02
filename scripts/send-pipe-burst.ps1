for ($i = 0; $i -lt 30; $i++) {
    try {
        $client = New-Object System.IO.Pipes.NamedPipeClientStream('.', 'VoiceLauncherBlazor_LaunchArgs', [System.IO.Pipes.PipeDirection]::Out)
        $client.Connect(1000)
        $sw = New-Object System.IO.StreamWriter($client, [System.Text.Encoding]::UTF8)
        $sw.AutoFlush = $true
        $sw.WriteLine("Talon|search|rapid-message-$i")
        $sw.Dispose()
        $client.Dispose()
    }
    catch {
        Write-Host "send failed: $_"
    }
    Start-Sleep -Milliseconds 20
}
Write-Host 'sent-burst'