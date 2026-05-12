Name: sync-www-images

Description:
This skill documents and exposes the `scripts/sync-www-images.ps1` helper
that synchronizes image assets from the Blazor webapp (`VoiceAdmin/wwwroot/images`)
into local project targets (for example `WinFormsApp/wwwroot/images`).

When to use:
- Add or refresh image assets in developer targets on-demand.
- Run a dry-run to preview changes before copying.
- Download named images from a running dev site when the server exposes a manifest.

Prerequisites:
- Windows PowerShell (PowerShell 5+) or PowerShell Core (`pwsh`).
- Script execution allowed: use `-ExecutionPolicy Bypass` when running the script.

Files:
- Script: `scripts/sync-www-images.ps1`
- README: `scripts/README-sync-images.md`
- Optional image-list: `scripts/images-to-sync.txt`

Parameters (script):
- `-Source` : local folder path or HTTP base URL (default: `VoiceAdmin/wwwroot/images`). When `-Published` is used the script will attempt to detect a local `dotnet publish` output instead.
- `-Dest` : array of destination folders. Defaults to `WinFormsApp/wwwroot/images`.
- `-Pattern` : glob for files to include (default `*.*`).
- `-DryRun` : preview actions without changing files.
- `-Force` : overwrite regardless of hash-match.
- `-ImageNamesFile` : newline list of image filenames for HTTP download mode.
- `-Published` : switch to pull files from a local `dotnet publish` output (searches `VoiceAdmin\bin` for `publish` folders and picks the most recent `wwwroot\images`).
- `-PublishFolder` : explicit folder to use as the publish source (can be a publish root or the images folder).
 - `-LogFile` : path to write detailed dry-run entries. When omitted the script defaults to `scripts/sync-www-images-dryrun.log` for `-DryRun` to avoid flooding the console.
 - `-PreviewLines` : number of dry-run lines to print from the start of the log (default `20`).
 - `-OnlyNew` : copy only files that do not already exist at the destination. When set, existing files are skipped regardless of content.

Usage examples:

Local dry-run (default source):

```powershell
.\scripts\sync-www-images.ps1 -DryRun
```

Note: When running `-DryRun` in an editor terminal, use `-LogFile` (or rely on the default) to write full details to a file and avoid crashing the editor from very large output.

Copy from a specific local source into destination(s):

```powershell
.\scripts\sync-www-images.ps1 -Source ".\VoiceAdmin\wwwroot\images" -Dest ".\WinFormsApp\wwwroot\images"
```

Pull assets from a local `dotnet publish` output (auto-detects latest publish):

```powershell
.\scripts\sync-www-images.ps1 -Published -DryRun
```

Download named images from a running site (requires an image list or server manifest):

```powershell
.\scripts\sync-www-images.ps1 -Source "http://localhost:5008" -ImageNamesFile .\scripts\images-to-sync.txt -Dest .\WinFormsApp\wwwroot\images
```

Operational notes:
- The script compares MD5 hashes and skips unchanged files by default.
- The script will not commit changes to source control; handle commits manually to avoid large binary diffs.
- Prefer `-DryRun` on first runs.

Suggested automation:
- Optional: wrap this script in a developer-tooling command or a simple task in `.vscode/tasks.json` for convenience (do not enable in CI builds that run automated commits).
