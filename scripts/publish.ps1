param(
    [string[]]$runtimes = @("win-x64","linux-x64","osx-x64"),
    [string]$configuration = "Release"
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path "$scriptDir/.."
Push-Location $repoRoot

$projects = @(
    @{ Name = "VoiceAdmin"; Path = "VoiceAdmin" }
)

# WinForms project (windows-only)
$winForms = @{ Name = "WinFormsApp"; Path = "WinFormsApp" }

foreach ($rid in $runtimes) {
    Write-Host "\n--- Publishing runtime: $rid ---" -ForegroundColor Cyan
    $outDir = Join-Path -Path "artifacts" -ChildPath $rid
    if (-Not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir | Out-Null }

    $isWindows = $rid -like "win*"

    $toPublish = $projects.Clone()
    if ($isWindows) { $toPublish += $winForms }

    foreach ($p in $toPublish) {
        $projPath = Join-Path $repoRoot $p.Path
        if (-Not (Test-Path $projPath)) { Write-Warning "Project path not found: $projPath, skipping"; continue }

        $projFile = Get-ChildItem -Path $projPath -Filter "*.csproj" -Recurse | Select-Object -First 1
        if (-not $projFile) { Write-Warning "No csproj found in $projPath"; continue }

        $projectOut = Join-Path $outDir $p.Name
        if (Test-Path $projectOut) { Remove-Item -Recurse -Force $projectOut }
        New-Item -ItemType Directory -Path $projectOut | Out-Null

        Write-Host "Publishing $($p.Name) for $rid -> $projectOut"
        dotnet publish $projFile.FullName -c $configuration -r $rid --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $projectOut

        # Zip the published output
        $zipName = "$($p.Name)-$($rid).zip"
        $zipPath = Join-Path $outDir $zipName
        if (Test-Path $zipPath) { Remove-Item $zipPath }
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::CreateFromDirectory($projectOut, $zipPath)
        Write-Host "Created $zipPath"
    }
}

Pop-Location
Write-Host "Publish complete. Artifacts in ./artifacts" -ForegroundColor Green
