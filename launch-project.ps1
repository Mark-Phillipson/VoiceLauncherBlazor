param(
    [string]$Category,
    [string]$Project,
    [switch]$Explorer
)

$configPath = Join-Path $PSScriptRoot 'project_launcher_config.json'
if (!(Test-Path $configPath)) {
    Write-Error "Config file not found: $configPath"
    exit 1
}
$config = Get-Content $configPath | ConvertFrom-Json

if (-not $Category) {
    Write-Host "Available categories:"
    $config.categories | ForEach-Object { Write-Host "- $($_.name)" }
    exit 0
}

$cat = $config.categories | Where-Object { $_.name -eq $Category }
if (-not $cat) {
    Write-Error "Category not found: $Category"
    exit 1
}

if (-not $Project) {
    Write-Host "Available projects in '$Category':"
    $cat.projects | ForEach-Object { Write-Host "- $($_.name)" }
    exit 0
}

$proj = $cat.projects | Where-Object { $_.name -eq $Project }
if (-not $proj) {
    Write-Error "Project not found: $Project in category $Category"
    exit 1
}

if ($Explorer -or $Category -eq "File Explorer Shortcuts") {
    Start-Process explorer.exe $proj.path
    Write-Host "Launched File Explorer at $($proj.path)"
} else {
    Start-Process code $proj.path
    Write-Host "Launched VS Code at $($proj.path)"
}