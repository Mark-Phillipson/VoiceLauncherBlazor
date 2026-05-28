$src = "C:\Users\MPhil\source\repos\VoiceLauncherBlazor\VoiceAdmin\wwwroot\quiz-packs\what-would-you-say-talon-20260526121200.json"
$dst = "C:\Users\MPhil\source\repos\VoiceLauncherBlazor\VoiceAdmin\wwwroot\quiz-packs\talon-pack-what-would-you-say-20260526121200.json"
if (-not (Test-Path $src)) {
    Write-Error "Source file missing: $src"
    exit 1
}
$raw = Get-Content -Raw -Path $src
# strip JS-style block comments /* ... */ which may appear in generated packs
$clean = [System.Text.RegularExpressions.Regex]::Replace($raw, '/\*.*?\*/', '', [System.Text.RegularExpressions.RegexOptions]::Singleline)
$json = $clean | ConvertFrom-Json
$out = [PSCustomObject]@{
  packId = "talon-what-would-you-say-20260526121200"
  packName = "what-would-you-say"
  createdAt = (Get-Date).ToUniversalTime().ToString("o")
  generator = "ConvertedFromQuizSchema"
  entries = @()
}
foreach ($e in $json.entries) {
  $entry = [PSCustomObject]@{
    listName = "what-would-you-say"
    spokenForm = $e.correctAnswer
    listValue = $e.listValue
    sourceFile = $e.sourceFile
  }
  $out.entries += $entry
}
$out | ConvertTo-Json -Depth 6 | Set-Content -Path $dst -Encoding utf8
Write-Output "Wrote: $dst"
