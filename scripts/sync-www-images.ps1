<#
.SYNOPSIS
    Sync image files from the Blazor webapp to local project targets (WinForms/VoiceAdmin).

.DESCRIPTION
    if ($Dest.Count -eq 0) {
        # Default destinations are the WinForms host and VoiceAdmin image folders.
        $Dest = @(
            Join-Path $RepoRoot "WinFormsApp\wwwroot\images",
            Join-Path $RepoRoot "VoiceAdmin\wwwroot\images"
        )
    }
    Copies image files from a local `wwwroot/images` folder (default)
    into one or more destination folders. Supports a dry-run mode, a
    force-overwrite switch, and an HTTP download mode when a list of
    filenames is provided via `-ImageNamesFile`.

.EXAMPLES
    # Dry-run copy from VoiceAdmin images to WinForms images
    .\sync-www-images.ps1 -DryRun

    # Real copy from a local folder to two destinations
    .\sync-www-images.ps1 -Source "C:\dev\VoiceAdmin\wwwroot\images" -Dest "C:\dev\WinFormsApp\wwwroot\images","C:\dev\VoiceAdmin\wwwroot\images"

    

#>

param(
    [string]$Source = "",
    [string[]]$Dest = @(),
    [string]$Pattern = "*.*",
    [switch]$DryRun,
    [switch]$Force,
    [string]$ImageNamesFile = "",
    [string]$DbPath = "",
    [switch]$Published,
    [string]$PublishFolder = "",
    [switch]$OnlyNew,
    [string]$LogFile = "",
    [int]$PreviewLines = 20
)

function Write-Info($m){ Write-Host $m }
function Write-ErrorLog($m){ Write-Host $m -ForegroundColor Red }

# Resolve a path to an absolute filesystem path (expands ~, resolves relative to repo root)
function Resolve-FullPath([string]$p){
    if ([string]::IsNullOrWhiteSpace($p)) { return $p }
    $p = $p.Trim()
    if ($p -match '^https?://') { return $p.TrimEnd('/') }
    if ($p.StartsWith('~')) { $p = $p -replace '^~', $env:USERPROFILE }
    try {
        if ([System.IO.Path]::IsPathRooted($p)) { $abs = [System.IO.Path]::GetFullPath($p) }
        else { $abs = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $p)) }
    } catch {
        $abs = Join-Path $RepoRoot $p
    }
    try {
        $rp = Resolve-Path -Path $abs -ErrorAction Stop
        if ($rp) { return $rp.ProviderPath }
    } catch { }
    return $abs
}

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$RepoRoot = (Get-Item (Join-Path $ScriptDir "..")).FullName

# Prepare DryRun log file to avoid flooding the console (prevents OOM in editors)
if ($DryRun) {
    if ([string]::IsNullOrWhiteSpace($LogFile)) {
        $LogFile = Join-Path $ScriptDir "sync-www-images-dryrun.log"
    }
    if (Test-Path $LogFile) { Remove-Item $LogFile -Force }
    Write-Info "Dry-run detailed output will be written to: $LogFile"
}

# If a DB path was supplied, read distinct Icon values from the Launcher table
if (-not [string]::IsNullOrWhiteSpace($DbPath)) {
    if (Test-Path $DbPath) {
        try {
            $raw = & sqlite3 $DbPath "SELECT DISTINCT Icon FROM Launcher WHERE Icon IS NOT NULL AND Icon <> '' ORDER BY Icon;" 2>$null
            if ($raw) {
                $dbNames = $raw -split "`n" | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique
                if ($dbNames.Count -gt 0) {
                    Write-Info "Loaded $($dbNames.Count) distinct Icon names from DB: $DbPath"
                } else {
                    Write-Info "No icon names found in DB: $DbPath"
                }
            } else {
                Write-Info "No icon names found in DB: $DbPath"
            }
        } catch {
            Write-ErrorLog "ERROR reading DB $DbPath : $_"
        }
    } else {
        Write-ErrorLog "DB path not found: $DbPath"
    }
}

# Load ImageNamesFile if provided (used by local copy flow)
$fileNames = @()
if (-not [string]::IsNullOrWhiteSpace($ImageNamesFile) -and (Test-Path $ImageNamesFile)) {
    try {
        $fileNames = Get-Content -Path $ImageNamesFile | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object { $_.Trim() } | Select-Object -Unique
        Write-Info "Loaded $($fileNames.Count) image names from file: $ImageNamesFile"
    } catch {
        Write-ErrorLog "ERROR reading image names file $ImageNamesFile : $_"
    }
}

# If the user requested the published output, map to the publish folder (or an override)
if ($Published) {
    if (-not [string]::IsNullOrWhiteSpace($PublishFolder)) {
        try {
            $pfItem = Get-Item -Path $PublishFolder -ErrorAction Stop
        } catch {
            throw "Publish folder not found: $PublishFolder"
        }

        $pf1 = Join-Path $pfItem.FullName "wwwroot\images"
        $pf2 = Join-Path $pfItem.FullName "images"
        if (Test-Path $pf1) { $Source = (Get-Item $pf1).FullName; Write-Info "Using publish folder as Source: $Source" }
        elseif (Test-Path $pf2) { $Source = (Get-Item $pf2).FullName; Write-Info "Using publish folder as Source: $Source" }
        elseif ($pfItem.PSIsContainer) { $Source = $pfItem.FullName; Write-Info "Using publish folder as Source: $Source" }
        else { throw "Publish folder not found or not a directory: $PublishFolder" }
    }
    else {
        # Search for publish outputs under VoiceAdmin/bin (produced by 'dotnet publish').
        $binPath = Join-Path $RepoRoot "VoiceAdmin\bin"
        $found = @()
        if (Test-Path $binPath) {
            $publishDirs = Get-ChildItem -Path $binPath -Directory -Recurse -ErrorAction SilentlyContinue | Where-Object { $_.Name -ieq 'publish' }
            foreach ($p in $publishDirs) {
                $p1 = Join-Path $p.FullName "wwwroot\images"
                $p2 = Join-Path $p.FullName "images"
                if (Test-Path $p1) { $found += Get-Item $p1 }
                elseif (Test-Path $p2) { $found += Get-Item $p2 }
            }
        }

        if ($found.Count -gt 0) {
            $best = $found | Sort-Object LastWriteTime -Descending | Select-Object -First 1
            $Source = $best.FullName
            Write-Info "Using publish folder as Source: $Source"
        } else {
            # Fallback: older style publish layout under repo 'publish' folder
            $candidate = Join-Path $RepoRoot "publish\VoiceAdmin\wwwroot\images"
            $candidateAlt = Join-Path $RepoRoot "publish\VoiceAdmin\images"
            if (Test-Path $candidate) { $Source = (Get-Item $candidate).FullName; Write-Info "Using publish folder as Source: $Source" }
            elseif (Test-Path $candidateAlt) { $Source = (Get-Item $candidateAlt).FullName; Write-Info "Using publish folder as Source: $Source" }
            else { throw "Publish images folder not found. Provide -PublishFolder or run 'dotnet publish'." }
        }
    }
}

if ([string]::IsNullOrWhiteSpace($Source)) {
    # Default to the Release publish images folder for VoiceAdmin (published output)
    $Source = Join-Path $RepoRoot "VoiceAdmin\bin\Release\net10.0\win-x64\publish\wwwroot\images"
}

if ($Dest.Count -eq 0) {
    # Default destinations are the WinForms host and VoiceAdmin image folders.
    $Dest = @(
        "C:\Users\MPhil\source\repos\VoiceLauncherBlazor\VoiceAdmin\wwwroot\images",
        "C:\Users\MPhil\source\repos\VoiceLauncherBlazor\WinFormsApp\wwwroot\images"
    )
}

    # Deduplicate any supplied destinations
    $Dest = $Dest | Select-Object -Unique

    # Normalize Dest entries: allow a single comma-separated string, trim quotes,
    # expand relative paths relative to repo root, and return absolute paths.
    if ($Dest -and $Dest.Count -eq 1) {
        $maybe = $Dest[0]
        if ($maybe -and ($maybe -match ',')) {
            $Dest = ($maybe -split '\s*,\s*') | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
        }
    }

    $normDest = @()
    foreach ($d in $Dest) {
        if ([string]::IsNullOrWhiteSpace($d)) { continue }
        $dclean = $d.Trim()
        # remove surrounding single/double quotes if present
        if ((($dclean.StartsWith('"') -and $dclean.EndsWith('"')) -or ($dclean.StartsWith("'") -and $dclean.EndsWith("'")))) {
            $dclean = $dclean.Substring(1, $dclean.Length - 2)
        }
        # expand ~ to user profile
        if ($dclean.StartsWith('~')) { $dclean = $dclean -replace '^~', $env:USERPROFILE }

        try {
            if ([System.IO.Path]::IsPathRooted($dclean)) {
                $abs = [System.IO.Path]::GetFullPath($dclean)
            } else {
                $abs = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $dclean))
            }
        } catch {
            # Fallback: join with repo root and don't fail just because the path doesn't exist yet
            $abs = Join-Path $RepoRoot $dclean
        }

        $abs = $abs -replace '/','\'
        $normDest += $abs
    }
    $Dest = $normDest | Select-Object -Unique

    # Normalize Dest entries: accept comma-separated entries and arrays
    $flattened = @()
    foreach ($item in $Dest) {
        if ([string]::IsNullOrWhiteSpace($item)) { continue }
        if ($item -like '*,*') {
            $parts = $item -split ','
            foreach ($p in $parts) { if (-not [string]::IsNullOrWhiteSpace($p)) { $flattened += $p.Trim() } }
        } else {
            $flattened += $item
        }
    }

    $normalized = @()
    foreach ($dRaw in $flattened) {
        if ([string]::IsNullOrWhiteSpace($dRaw)) { continue }
        $dClean = $dRaw.Trim()
        if ($dClean.StartsWith('"') -and $dClean.EndsWith('"')) { $dClean = $dClean.Substring(1, $dClean.Length - 2) }
        elseif ($dClean.StartsWith("'") -and $dClean.EndsWith("'")) { $dClean = $dClean.Substring(1, $dClean.Length - 2) }
        $dClean = $dClean.Trim()

        # Resolve relative paths against the repo root
        if (-not [System.IO.Path]::IsPathRooted($dClean)) {
            $dClean = Join-Path $RepoRoot $dClean
        }

        # Normalize the path (if it exists) to a full path; otherwise keep as-is
        $resolved = $null
        try {
            $rp = Resolve-Path -Path $dClean -ErrorAction Stop
            if ($rp) { $resolved = $rp.ProviderPath }
        } catch { }
        if ($resolved) { $dClean = $resolved }

        $normalized += $dClean
    }
    $Dest = $normalized | Select-Object -Unique

    # Resolve Source and Dest to full absolute paths (unless Source is HTTP)
    if ($Source -notmatch '^https?://') {
        $Source = Resolve-FullPath $Source
    }

    $Dest = $Dest | ForEach-Object {
        if ($_ -match '^https?://') { $_ } else { Resolve-FullPath $_ }
    } | Select-Object -Unique

    # Show resolved paths and existence checks, ask for confirmation before proceeding
    Write-Host "`nSync summary:"
    if ($Source -match '^https?://') {
        Write-Host "  Source (HTTP): $Source"
    } else {
        Write-Host "  Source: $Source"
        Write-Host ("    Exists: {0}" -f (Test-Path $Source))
    }
    # Ensure $Dest is enumerated as an array and print each entry clearly
    $Dest = @($Dest)
    foreach ($d in $Dest) {
        $existsText = if ($d -match '^https?://') { 'HTTP' } elseif (Test-Path $d) { 'Exists' } else { 'Missing' }
        Write-Host "  Destination: $d  - $existsText"
    }
    if ($DryRun) { Write-Host "  Mode: Dry-run (no files will be written)" }

    $resp = Read-Host "Proceed with sync? (Y/N)"
    if ($resp -notin @('Y','y','Yes','yes')) {
        Write-Host "Aborted by user."
        exit 0
    }

    $stats = [PSCustomObject]@{Copied=0;Skipped=0;Errors=0;WouldCopy=0}

try {
    if ($Source -match '^https?://') {
        # HTTP download flow - requires an images list file or a manifest
        # If DB-provided names exist, prefer them
        if ($dbNames -and $dbNames.Count -gt 0) {
            $names = $dbNames
        }

        if (-not (Test-Path $ImageNamesFile) -and -not $names) {
            # Try to find a manifest on the server
            $manifestUrls = @("$Source/images-manifest.json","$Source/images/manifest.json","$Source/images.json")
            $found = $null
            foreach ($u in $manifestUrls) {
                try {
                    $r = Invoke-RestMethod -Uri $u -Method Get -ErrorAction Stop
                    if ($r) { $found = $r; break }
                } catch { }
            }
            if (-not $found) {
                throw "No image list provided. Specify -ImageNamesFile when using HTTP Source."
            }
            $names = $found | ForEach-Object { $_ } 
        } else {
            $names = Get-Content -Path $ImageNamesFile | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object { $_.Trim() }
        }

        foreach ($name in $names) {
            $rel = (($name -as [string]) -replace '^[\\/]+','')
            foreach ($d in $Dest) {
                $destFile = Join-Path $d $rel
                $destDir = Split-Path $destFile -Parent
                if (-not $DryRun) { New-Item -ItemType Directory -Path $destDir -Force | Out-Null }

                $uri = ($Source.TrimEnd('/') + "/images/" + $rel)
                if ($DryRun) {
                    if ($OnlyNew -and (Test-Path $destFile)) {
                        $line = "[DRYRUN] Would skip (exists): $destFile"
                        Add-Content -Path $LogFile -Value $line
                        $stats.Skipped++
                    } else {
                        $line = "[DRYRUN] Would download: $uri -> $destFile"
                        Add-Content -Path $LogFile -Value $line
                        $stats.WouldCopy++
                    }
                    continue
                }

                try {
                    $tmp = Join-Path $env:TEMP ([System.IO.Path]::GetRandomFileName())
                    Invoke-WebRequest -Uri $uri -OutFile $tmp -UseBasicParsing -ErrorAction Stop
                    if (Test-Path $destFile) {
                        if ($OnlyNew) {
                            Remove-Item $tmp -Force
                            Write-Info "Skipped (exists): $rel"
                            $stats.Skipped++
                            continue
                        }
                        $h1 = (Get-FileHash -Path $tmp -Algorithm MD5).Hash
                        $h2 = (Get-FileHash -Path $destFile -Algorithm MD5).Hash
                        if ($h1 -eq $h2 -and -not $Force) {
                            Remove-Item $tmp -Force
                            Write-Info "Skipped (unchanged): $rel"
                            $stats.Skipped++
                            continue
                        }
                    }
                    Move-Item -Path $tmp -Destination $destFile -Force
                    Write-Info "Downloaded: $rel -> $destFile"
                    $stats.Copied++
                } catch {
                    Write-ErrorLog "ERROR downloading $uri : $_"
                    $stats.Errors++
                    if (Test-Path $tmp) { Remove-Item $tmp -Force }
                }
            }
        }
    }
    else {
        # Local copy flow
        if (-not (Test-Path $Source)) { throw "Source path not found: $Source" }
        $srcItem = Get-Item $Source
        $base = $srcItem.FullName.TrimEnd('\') + '\'

        # If DB-supplied names are available, use them as the list to copy
        if ($dbNames -and $dbNames.Count -gt 0) {
            foreach ($rawName in $dbNames) {
                $relName = ([System.IO.Path]::GetFileName($rawName) -as [string]) -replace '^[\\/]+' , ''
                $foundFiles = @()

                # Direct location in the source folder
                $direct = Join-Path $Source $relName
                if (Test-Path $direct) { $foundFiles += Get-Item $direct }

                # Recursive search under source for matching filename
                if ($foundFiles.Count -eq 0) {
                    $foundFiles += Get-ChildItem -Path $Source -File -Recurse -ErrorAction SilentlyContinue | Where-Object { $_.Name -ieq $relName }
                }

                # If not found in source, try common publish outputs under VoiceAdmin/bin and repo publish folder
                if ($foundFiles.Count -eq 0) {
                    $binPath = Join-Path $RepoRoot "VoiceAdmin\bin"
                    if (Test-Path $binPath) {
                        $publishDirs = Get-ChildItem -Path $binPath -Directory -Recurse -ErrorAction SilentlyContinue | Where-Object { $_.Name -ieq 'publish' }
                        foreach ($p in $publishDirs) {
                            $p1 = Join-Path $p.FullName "wwwroot\images\$relName"
                            $p2 = Join-Path $p.FullName "images\$relName"
                            if (Test-Path $p1) { $foundFiles += Get-Item $p1 }
                            elseif (Test-Path $p2) { $foundFiles += Get-Item $p2 }
                        }
                    }
                    $candidate = Join-Path $RepoRoot "publish\VoiceAdmin\wwwroot\images\$relName"
                    if (Test-Path $candidate) { $foundFiles += Get-Item $candidate }
                }

                if ($foundFiles.Count -eq 0) {
                    # Record missing sources
                    if ($DryRun) {
                        Add-Content -Path $LogFile -Value "[DRYRUN] Missing source for $relName"
                        $stats.Skipped++
                    } else {
                        Write-ErrorLog "Missing source for $relName"
                        $stats.Skipped++
                    }
                    continue
                }

                # Copy each located source file to destinations
                foreach ($f in $foundFiles | Select-Object -Unique) {
                    if ($f.FullName.StartsWith($base, [System.StringComparison]::InvariantCultureIgnoreCase)) {
                        $relative = $f.FullName.Substring($base.Length)
                    } else {
                        $relative = $f.Name
                    }

                    foreach ($d in $Dest) {
                        $destFull = Join-Path $d $relative
                        $destDir = Split-Path $destFull -Parent

                        if ($DryRun) {
                            if ([string]::Equals($destFull, $f.FullName, [System.StringComparison]::InvariantCultureIgnoreCase)) {
                                $line = "[DRYRUN] Skipped (destination equals source): $($f.FullName)"
                                Add-Content -Path $LogFile -Value $line
                                $stats.Skipped++
                            }
                            elseif ($OnlyNew -and (Test-Path $destFull)) {
                                $line = "[DRYRUN] Would skip (exists): $destFull"
                                Add-Content -Path $LogFile -Value $line
                                $stats.Skipped++
                            }
                            else {
                                $line = "[DRYRUN] Would copy: $($f.FullName) -> $destFull"
                                Add-Content -Path $LogFile -Value $line
                                $stats.WouldCopy++
                            }
                            continue
                        }

                        try {
                            # Skip copying a file onto itself when the destination file path equals the source file path
                            if ([string]::Equals($destFull, $f.FullName, [System.StringComparison]::InvariantCultureIgnoreCase)) {
                                Write-Info "Skipped (destination equals source): $relative"
                                $stats.Skipped++
                                continue
                            }

                            # If OnlyNew is set, skip existing destination files
                            if ($OnlyNew -and (Test-Path $destFull)) {
                                Write-Info "Skipped (exists): $relative"
                                $stats.Skipped++
                                continue
                            }

                            New-Item -ItemType Directory -Path $destDir -Force | Out-Null
                            if (Test-Path $destFull) {
                                $hSrc = (Get-FileHash -Path $f.FullName -Algorithm MD5).Hash
                                $hDst = (Get-FileHash -Path $destFull -Algorithm MD5).Hash
                                if ($hSrc -eq $hDst -and -not $Force) {
                                    Write-Info "Skipped (unchanged): $relative"
                                    $stats.Skipped++
                                    continue
                                }
                            }
                            Copy-Item -Path $f.FullName -Destination $destFull -Force
                            Write-Info "Copied: $relative -> $destFull"
                            $stats.Copied++
                        } catch {
                            Write-ErrorLog "ERROR copying $($f.FullName) -> $destFull : $_"
                            $stats.Errors++
                        }
                    }
                }
            }
        }
        else {
            # Fallback to previous behavior: copy all files matching pattern
            $files = Get-ChildItem -Path $Source -Include $Pattern -File -Recurse
            if ($files.Count -eq 0) { Write-Info "No files found matching pattern '$Pattern' in $Source" }

            foreach ($f in $files) {
                $relative = $f.FullName.Substring($base.Length)
                foreach ($d in $Dest) {
                    $destFull = Join-Path $d $relative
                    $destDir = Split-Path $destFull -Parent
                    if ($DryRun) {
                        if ([string]::Equals($destFull, $f.FullName, [System.StringComparison]::InvariantCultureIgnoreCase)) {
                            $line = "[DRYRUN] Skipped (destination equals source): $($f.FullName)"
                            Add-Content -Path $LogFile -Value $line
                            $stats.Skipped++
                        }
                        elseif ($OnlyNew -and (Test-Path $destFull)) {
                            $line = "[DRYRUN] Would skip (exists): $destFull"
                            Add-Content -Path $LogFile -Value $line
                            $stats.Skipped++
                        }
                        else {
                            $line = "[DRYRUN] Would copy: $($f.FullName) -> $destFull"
                            Add-Content -Path $LogFile -Value $line
                            $stats.WouldCopy++
                        }
                        continue
                    }

                    try {
                        # Skip copying a file onto itself when the destination file path equals the source file path
                        if ([string]::Equals($destFull, $f.FullName, [System.StringComparison]::InvariantCultureIgnoreCase)) {
                            Write-Info "Skipped (destination equals source): $relative"
                            $stats.Skipped++
                            continue
                        }

                        # If OnlyNew is set, skip existing destination files
                        if ($OnlyNew -and (Test-Path $destFull)) {
                            Write-Info "Skipped (exists): $relative"
                            $stats.Skipped++
                            continue
                        }

                        New-Item -ItemType Directory -Path $destDir -Force | Out-Null
                        if (Test-Path $destFull) {
                            $hSrc = (Get-FileHash -Path $f.FullName -Algorithm MD5).Hash
                            $hDst = (Get-FileHash -Path $destFull -Algorithm MD5).Hash
                            if ($hSrc -eq $hDst -and -not $Force) {
                                Write-Info "Skipped (unchanged): $relative"
                                $stats.Skipped++
                                continue
                            }
                        }
                        Copy-Item -Path $f.FullName -Destination $destFull -Force
                        Write-Info "Copied: $relative -> $destFull"
                        $stats.Copied++
                    } catch {
                        Write-ErrorLog "ERROR copying $($f.FullName) -> $destFull : $_"
                        $stats.Errors++
                    }
                }
            }
        }
    }
} catch {
    Write-ErrorLog "Fatal: $_"
    exit 1
}

if ($DryRun) {
    if (Test-Path $LogFile) {
        try {
            $totalLines = (Get-Content -Path $LogFile | Measure-Object -Line).Lines
        } catch {
            $totalLines = 0
        }
        Write-Host "`nDry-run details written to: $LogFile"
        $showCount = [Math]::Min($PreviewLines, $totalLines)
        if ($showCount -gt 0) {
            Write-Host "Showing first $showCount of $totalLines entries:"
            Get-Content -Path $LogFile -TotalCount $showCount | ForEach-Object { Write-Host $_ }
            if ($totalLines -gt $showCount) { Write-Host "... (showing $showCount of $totalLines entries, full log at $LogFile)" }
        } else {
            Write-Host "No dry-run entries were recorded."
        }
    } else {
        Write-Host "Dry-run produced no entries."
    }
}

Write-Host "\nSummary: Copied=$($stats.Copied) Skipped=$($stats.Skipped) Errors=$($stats.Errors) WouldCopy=$($stats.WouldCopy)"
