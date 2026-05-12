# Checks HTTP accessibility of images listed in missing-images.txt
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\check-missing-http.ps1

param(
    [string]$Base = 'http://localhost:5008',
    [string]$ImagesFile = "$PSScriptRoot\\missing-images.txt",
    [string]$OutFile = "$PSScriptRoot\\check-missing-http-results.csv"
)

if (-not (Test-Path $ImagesFile)) {
    Write-Host "Missing images list not found: $ImagesFile" -ForegroundColor Red
    exit 2
}

$list = Get-Content -Path $ImagesFile | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object { $_.Trim() }

$results = @()
foreach ($n in $list) {
    $name = $n.Trim()
    if ([string]::IsNullOrWhiteSpace($name)) { continue }
    $url = "$Base/images/$name"
    $status = ''
    try {
        $req = [System.Net.WebRequest]::Create($url)
        $req.Method = 'HEAD'
        $resp = $req.GetResponse()
        $status = ([int]([System.Net.HttpWebResponse]$resp).StatusCode)
        $resp.Close()
    } catch [System.Net.WebException] {
        $we = $_.Exception
        if ($we.Response -and ($we.Response -is [System.Net.HttpWebResponse])) {
            try { $status = ([int]([System.Net.HttpWebResponse]$we.Response).StatusCode) } catch { $status = 'ERROR' }
            try { $we.Response.Close() } catch { }
        } else {
            # fallback: try GET
            try {
                $req2 = [System.Net.WebRequest]::Create($url)
                $req2.Method = 'GET'
                $resp2 = $req2.GetResponse()
                $status = ([int]([System.Net.HttpWebResponse]$resp2).StatusCode)
                $resp2.Close()
            } catch {
                $status = "ERROR: $($_.Exception.Message -replace '[\r\n]',' ')"
            }
        }
    } catch {
        $status = "ERROR: $($_.Exception.Message -replace '[\r\n]',' ')"
    }

    $line = "$name,$url,$status"
    Write-Host $line
    $results += $line
}

$results | Out-File -FilePath $OutFile -Encoding utf8
Write-Host "Wrote results to: $OutFile"