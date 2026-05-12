# sync-www-images.ps1

Small PowerShell helper to sync image assets from the Blazor webapp image folder into local targets (WinForms/VoiceAdmin).

Usage examples

Local dry-run (default source is `VoiceAdmin/wwwroot/images`):

```powershell
# preview what would be copied
.\scripts\sync-www-images.ps1 -DryRun
```

Copy from a specific local source into one or more destinations (do not include the source as a destination — the script will skip identical paths):

```powershell
.\scripts\sync-www-images.ps1 -Source ".\VoiceAdmin\wwwroot\images" -Dest ".\WinFormsApp\wwwroot\images"
```

Download mode (when the running site exposes images). Provide a file listing image names:

```powershell
.\scripts\sync-www-images.ps1 -Source "http://localhost:5008" -ImageNamesFile .\scripts\images-to-sync.txt -Dest .\WinFormsApp\wwwroot\images
```

Pull from a local `dotnet publish` output (published site files):

```powershell
.\scripts\sync-www-images.ps1 -Published -DryRun
```

Or specify a custom publish folder:

```powershell
.\scripts\sync-www-images.ps1 -PublishFolder ".\publish\VoiceAdmin\wwwroot\images" -Dest ".\WinFormsApp\wwwroot\images" -DryRun
```

Notes
- The script will compare MD5 file hashes and skip unchanged files unless `-Force` is supplied.
- By default the script targets: `WinFormsApp/wwwroot/images`.
- If a destination equals the source path, the script will skip copying that file.
- Consider running with `-DryRun` before applying changes.
- When running `-DryRun` in an editor terminal (like VS Code) that may crash from very large output, use the built-in `-LogFile` option (defaults to `scripts/sync-www-images-dryrun.log`) which writes detailed entries to a file and prints only a short preview. Example:

```powershell
.\scripts\sync-www-images.ps1 -DryRun -LogFile .\scripts\sync-www-images-dryrun.log
```

Copy only files that don't already exist in the destination:

```powershell
.\scripts\sync-www-images.ps1 -OnlyNew -Dest ".\WinFormsApp\wwwroot\images"
```
