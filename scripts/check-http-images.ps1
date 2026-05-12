$base='http://localhost:5008'
$pageUrl = "$base/launcherstable/0"
try {
    $r = Invoke-WebRequest -Uri $pageUrl -UseBasicParsing -ErrorAction Stop
    $content = $r.Content
} catch {
    Write-Host "ERROR: Could not fetch page: $($_.Exception.Message)"
    exit 2
}
$matches = [regex]::Matches($content, '/images/([^"\s>]+)')
$names = $matches | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
Write-Host "Found image refs on page: $($names.Count)"
$bad = @()
foreach ($n in $names) {
    $url = "$base/images/$n"
    try {
        $h = Invoke-WebRequest -Uri $url -Method Head -UseBasicParsing -TimeoutSec 10 -ErrorAction Stop
        if ($h.StatusCode -ne 200) { $bad += [PSCustomObject]@{Name=$n;Status=$h.StatusCode} }
    } catch {
        $bad += [PSCustomObject]@{Name=$n;Status='ERROR'}
    }
}
if ($names.Count -gt 0) {
    Write-Host 'Sample image refs:'
    $names | Select-Object -First 20 | ForEach-Object { Write-Host $_ }
}
Write-Host "`nBroken image responses:"
if ($bad.Count -eq 0) { Write-Host 'None' } else { $bad | ForEach-Object { Write-Host "$($_.Name) -> $($_.Status)" } }
