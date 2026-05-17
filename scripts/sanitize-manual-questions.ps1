# One-time manual sanitization script
# Redacts occurrences of the CorrectAnswer inside Prompt for manual questions

$file = "c:\Users\MPhil\source\repos\VoiceLauncherBlazor\VoiceAdmin\wwwroot\CursorlessManualQuestions.json"
if (-not (Test-Path $file)) {
    Write-Error "File not found: $file"
    exit 2
}

$text = Get-Content $file -Raw
try {
    $items = $text | ConvertFrom-Json
} catch {
    Write-Error "Failed to parse JSON: $_"
    exit 2
}

$modified = @()
for ($i = 0; $i -lt $items.Count; $i++) {
    $q = $items[$i]
    if ($null -eq $q.Prompt -or $null -eq $q.CorrectAnswer) { continue }
    $prompt = [string]$q.Prompt
    $answer = [string]$q.CorrectAnswer
    if ([string]::IsNullOrWhiteSpace($prompt) -or [string]::IsNullOrWhiteSpace($answer)) { continue }

    $escaped = [regex]::Escape($answer)
    # Use non-word boundaries to avoid accidental substring matches (e.g., 'wing' inside 'following')
    $pattern = '(?<!\w)' + $escaped + '(?!\w)'
    if ([regex]::IsMatch($prompt, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
        $newPrompt = [regex]::Replace($prompt, $pattern, '____', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        if (-not [string]::IsNullOrWhiteSpace($newPrompt)) {
            $items[$i].Prompt = $newPrompt
            $modified += $i
        }
    }
}

if ($modified.Count -gt 0) {
    $items | ConvertTo-Json -Depth 10 | Set-Content -Path $file -Encoding UTF8
    Write-Host "Sanitized prompts at indices: $($modified -join ', ')"
    exit 0
} else {
    Write-Host "No manual prompts required sanitization."
    exit 0
}
