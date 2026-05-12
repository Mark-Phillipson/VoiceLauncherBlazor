$db='c:\Users\MPhil\source\repos\VoiceLauncherBlazor\VoiceAdmin\data\voicelauncher-azure.db'
$publish='c:\Users\MPhil\source\repos\VoiceLauncherBlazor\VoiceAdmin\bin\Release\net10.0\win-x64\publish\wwwroot\images'
$icons = & sqlite3 $db "SELECT DISTINCT Icon FROM Launcher WHERE Icon IS NOT NULL AND Icon <> '' ORDER BY Icon;"
$list = $icons -split "`n" | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object { [System.IO.Path]::GetFileName($_.Trim()) } | Select-Object -Unique
$missing=@()
foreach ($name in $list) { if (-not (Test-Path (Join-Path $publish $name))) { $missing += $name } }
Write-Host "Total distinct icons referenced: $($list.Count)"
Write-Host "Missing in publish images: $($missing.Count)"
if ($missing.Count -gt 0) { $missing | ForEach-Object { Write-Host "MISSING: $_" } }