try {
    $r = Invoke-RestMethod -Uri 'http://localhost:5008/api/images/unlinked' -Method Get -ErrorAction Stop
    Write-Host "Unlinked images count: $($r.Length)"
    if ($r.Length -gt 0) { $r | ForEach-Object { Write-Host $_ } }
} catch {
    Write-Host "ERROR calling API: $($_.Exception.Message)"
}