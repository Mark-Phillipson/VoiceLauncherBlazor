param(
  [string]$fileName = "what-would-you-say-talon-20260526121200.json"
)
$body = @{ fileName = $fileName } | ConvertTo-Json
try {
  $res = Invoke-RestMethod -Uri 'http://localhost:5008/api/quizpacks/import' -Method Post -ContentType 'application/json' -Body $body
  $res | ConvertTo-Json -Depth 5
} catch {
  Write-Error $_.Exception.Message
  if ($_.Exception.Response) {
    $stream = $_.Exception.Response.GetResponseStream()
    $sr = New-Object System.IO.StreamReader($stream)
    Write-Output $sr.ReadToEnd()
  }
  exit 1
}
