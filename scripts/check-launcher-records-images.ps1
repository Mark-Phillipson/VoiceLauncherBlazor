param(
    [string]$DbPath = "c:\Users\MPhil\source\repos\VoiceLauncherBlazor\VoiceAdmin\data\voicelauncher-azure.db",
    [string]$PublishImages = "c:\Users\MPhil\source\repos\VoiceLauncherBlazor\VoiceAdmin\bin\Release\net10.0\win-x64\publish\wwwroot\images",
    [string]$WebImages = "c:\Users\MPhil\source\repos\VoiceLauncherBlazor\VoiceAdmin\wwwroot\images",
    [string]$WinFormsImages = "c:\Users\MPhil\source\repos\VoiceLauncherBlazor\WinFormsApp\wwwroot\images"
)

$cmd = "SELECT ID || '|' || Name || '|' || IFNULL(Icon,'') || '|' || IFNULL(CommandLine,'') FROM Launcher WHERE Name LIKE '%face%';"
$rows = & sqlite3 $DbPath $cmd
if (-not $rows) { Write-Host "No matching launcher rows found."; exit 0 }

foreach ($row in $rows) {
    $parts = $row -split '\|',4
    $id = $parts[0].Trim()
    $name = $parts[1].Trim()
    $icon = $parts[2].Trim()
    $cmdline = $parts[3].Trim()
    $filename = [System.IO.Path]::GetFileName($icon)
    $pubPath = Join-Path $PublishImages $filename
    $webPath = Join-Path $WebImages $filename
    $winPath = Join-Path $WinFormsImages $filename

    Write-Host "ID: $id"
    Write-Host "  Name: $name"
    Write-Host "  Icon field: $icon"
    Write-Host "  CommandLine: $cmdline"
    Write-Host "  Filename: $filename"
    Write-Host "  Exists in publish images: " (Test-Path $pubPath)
    Write-Host "    $pubPath"
    Write-Host "  Exists in webapp images:  " (Test-Path $webPath)
    Write-Host "    $webPath"
    Write-Host "  Exists in WinForms images: " (Test-Path $winPath)
    Write-Host "    $winPath"
    Write-Host ""
}
