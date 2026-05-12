<#
compare-images.ps1
Compares files between VoiceAdmin/wwwroot/images and WinFormsApp/wwwroot/images
Prints counts and sample paths for missing and differing files.
#>

param(
    [string]$RepoRoot = $(Get-Location).Path,
    [string]$Source = "",
    [string]$Dest = ""
)

if ([string]::IsNullOrWhiteSpace($Source)) { $Source = Join-Path $RepoRoot 'VoiceAdmin\wwwroot\images' }
if ([string]::IsNullOrWhiteSpace($Dest)) { $Dest = Join-Path $RepoRoot 'WinFormsApp\wwwroot\images' }

Write-Host "RepoRoot: $RepoRoot"
Write-Host "Source: $Source"
Write-Host "Dest: $Dest"

if (-not (Test-Path $Source)) { Write-Host "Source not found: $Source"; exit 1 }
if (-not (Test-Path $Dest)) { Write-Host "Dest not found: $Dest"; Write-Host "No destination exists; all files are missing"; exit 0 }

$srcFiles = Get-ChildItem -Path $Source -File -Recurse
$total = $srcFiles.Count
$missing = New-Object System.Collections.Generic.List[string]
$diffs = New-Object System.Collections.Generic.List[psobject]

foreach ($f in $srcFiles) {
    $rel = $f.FullName.Substring($Source.Length + 1)
    $dst = Join-Path $Dest $rel
    if (-not (Test-Path $dst)) {
        $missing.Add($dst)
    } else {
        try {
            $h1 = (Get-FileHash -Path $f.FullName -Algorithm MD5).Hash
            $h2 = (Get-FileHash -Path $dst -Algorithm MD5).Hash
            if ($h1 -ne $h2) {
                $diffs.Add([PSCustomObject]@{Src=$f.FullName;Dst=$dst})
            }
        } catch {
            Write-Host "Error hashing $($f.FullName) or $dst : $_"
        }
    }
}

Write-Host ""
Write-Host "Source files total: $total"
Write-Host "Missing at dest: $($missing.Count)"
Write-Host "Different content: $($diffs.Count)"
Write-Host ""
if ($missing.Count -gt 0) {
    Write-Host "Samples (missing, up to 10):"
    $missing | Select-Object -First 10 | ForEach-Object { Write-Host $_ }
}
if ($diffs.Count -gt 0) {
    Write-Host "Samples (different, up to 10):"
    $diffs | Select-Object -First 10 | ForEach-Object { Write-Host ($_.Src + ' -> ' + $_.Dst) }
}

Write-Host ""
Write-Host "Done."
