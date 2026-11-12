param()
$ErrorActionPreference = 'Stop'

$vsix = Join-Path $PSScriptRoot 'bin\Debug\net48\CursorlessVisualStudio.vsix'
if (-not (Test-Path -LiteralPath $vsix)) {
    throw 'Build CursorlessVisualStudio.csproj in Debug first. See README.md in this directory.'
}
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path -LiteralPath $vswhere)) { throw 'Visual Studio Installer (vswhere) was not found.' }
$instances = @(& $vswhere -latest -version '[18.0,19.0)' -products '*' -requires Microsoft.VisualStudio.Component.VSSDK -format json | ConvertFrom-Json)
if ($instances.Count -eq 0) { throw 'Visual Studio 2026 with the Visual Studio SDK is required.' }
$instance = $instances[0]
$suffix = 'CursorlessDemo'
$running = Get-CimInstance Win32_Process -Filter "Name='devenv.exe'" |
    Where-Object { $_.CommandLine -match '/RootSuffix\s+CursorlessDemo(?:\s|$)' }
if ($running) { throw 'Close the CursorlessDemo experimental IDE before installing this build. Leave normal IDE instances open.' }

$instanceName = "18.0_$($instance.instanceId)"
$profile = Join-Path $env:LOCALAPPDATA "Microsoft\VisualStudio\$instanceName$suffix"
if (-not (Test-Path -LiteralPath $profile)) {
    $createInstance = Join-Path $instance.installationPath 'VSSDK\VisualStudioIntegration\Tools\Bin\CreateExpInstance.exe'
    & $createInstance /Create "/VSInstance=$instanceName" "/RootSuffix=$suffix"
    if ($LASTEXITCODE -ne 0) { throw "Creating the experimental profile failed ($LASTEXITCODE)." }
}

$installer = Join-Path $instance.installationPath 'Common7\IDE\VSIXInstaller.exe'
$installation = Start-Process -FilePath $installer -ArgumentList '/quiet',"/instanceIds:$($instance.instanceId)",
    "/rootSuffix:$suffix",'/logFile:cursorless-demo-install.log',('"' + $vsix + '"') -Wait -PassThru
if ($installation.ExitCode -ne 0) {
    throw "VSIX installation failed ($($installation.ExitCode)). See $env:TEMP\cursorless-demo-install.log."
}
$devenvCommand = Join-Path $instance.installationPath 'Common7\IDE\devenv.com'
& $devenvCommand /RootSuffix $suffix /UpdateConfiguration
if ($LASTEXITCODE -ne 0) { throw "Experimental IDE registration failed ($LASTEXITCODE)." }
$devenv = Join-Path $instance.installationPath 'Common7\IDE\devenv.exe'
$demo = Join-Path $PSScriptRoot 'Demo.cs'
Start-Process -FilePath $devenv -ArgumentList "/RootSuffix $suffix /NoSplash /Edit `"$demo`""
