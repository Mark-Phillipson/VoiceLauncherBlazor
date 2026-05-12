$db='c:\Users\MPhil\source\repos\VoiceLauncherBlazor\VoiceAdmin\data\voicelauncher-azure.db'
$icons = & sqlite3 $db "SELECT DISTINCT Icon FROM Launcher WHERE Icon IS NOT NULL AND Icon <> '' ORDER BY Icon;"
$list = $icons -split "`n" | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object { $_.Trim() } | Select-Object -Unique
$problems = $list | Where-Object { $_ -match '/' -or $_ -match '\\' -or $_ -match '^http' -or $_ -match ':' }
Write-Host "Total distinct icons: $($list.Count)"
Write-Host "Problematic icon entries (contain '/', '\\', start with 'http', or contain ':') count: $($problems.Count)"
if ($problems.Count -gt 0) { $problems | ForEach-Object { Write-Host "PROBLEM: $_" } }