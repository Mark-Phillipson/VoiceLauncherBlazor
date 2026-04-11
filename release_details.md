Run these commands in your repo (each block is copyable):

- Update local main and inspect recent tags:
```powershell
cd "C:\Users\MPhil\source\repos\VoiceLauncherBlazor"
git checkout main
git pull origin main
git tag --list --sort=-creatordate | Out-String -Width 4096
git describe --tags --abbrev=0
```

- Inspect VoiceAdmin project for a `<Version>` or `<PackageVersion>`:
```powershell
Select-String -Path "VoiceAdmin\VoiceAdmin.csproj" -Pattern "<Version>|<PackageVersion>" -SimpleMatch
# or open the file:
code "VoiceAdmin\VoiceAdmin.csproj"
```

- Example: once you know the next version (replace vX.Y.Z with the actual new tag), create and push an annotated tag:
```powershell
git tag -a v0.0.8 -m "chore(release): v0.0.8"
git push origin v0.0.8
```

- Alternative: push all local tags at once:
```powershell
git push --tags
```

- Optional: create a GitHub Release with the `gh` CLI (replace v0.0.8 and the title/body as needed):
```powershell
gh release create v0.0.8 --title "v0.0.8" --notes "Release notes here"
```






