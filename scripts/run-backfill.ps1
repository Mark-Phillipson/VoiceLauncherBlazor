$uri = 'http://localhost:5008/admin/backfill-descriptions'
for ($i = 0; $i -lt 60; $i++) {
    try {
        $r = Invoke-RestMethod -Method Post -Uri $uri -TimeoutSec 5
        Write-Output 'BACKFILL_RESULT:'
        $r | ConvertTo-Json -Depth 5
        exit 0
    } catch {
        Start-Sleep -Seconds 1
    }
}
Write-Error 'Failed to reach endpoint after retries'
exit 1
