# Publishing Guide for Talon Voice Commands Server Project

This guide covers publishing the TalonVoiceCommandsServer (Blazor Server app) as a self-contained application for Windows, macOS, and Linux. It also explains how to configure a custom port.

## Prerequisites
- .NET SDK installed on the build machine.
- Project source code in `TalonVoiceCommandsServer/`.

## Publishing Steps

### Windows
1. Open terminal in `TalonVoiceCommandsServer/` directory.
2. Run: `dotnet publish --configuration Release --self-contained true --runtime win-x64 --output ./publish`
3. Navigate to `./publish/` and run: `.\TalonVoiceCommandsServer.exe`

### macOS
1. Open terminal in `TalonVoiceCommandsServer/` directory.
2. Run: `dotnet publish --configuration Release --self-contained true --runtime osx-x64 --output ./publish`
3. Navigate to `./publish/` and run: `./TalonVoiceCommandsServer`

### Linux
1. Open terminal in `TalonVoiceCommandsServer/` directory.
2. Run: `dotnet publish --configuration Release --self-contained true --runtime linux-x64 --output ./publish`
3. Navigate to `./publish/` and run: `./TalonVoiceCommandsServer`

## Configuring a Custom Port
To change the port (default is 5008 as configured):
1. Edit `appsettings.json` in the project root.
2. Update the `Kestrel.Endpoints.Http.Url` value, e.g., `"http://localhost:8080"`.
3. Republish the app.
4. The published app will use the new port.

For environment-specific overrides, set `ASPNETCORE_URLS` before running.

On macOS / Linux (bash/zsh):

```
ASPNETCORE_URLS=http://localhost:9090 ./TalonVoiceCommandsServer
```

On Windows PowerShell (recommended for published Windows builds):

```powershell
# Set the variable for the current PowerShell session
$env:ASPNETCORE_URLS = 'http://localhost:9090'
# Then run the executable from the publish folder
.\TalonVoiceCommandsServer.exe
```

Note: Environment variables (like `ASPNETCORE_URLS`) take precedence over values in `appsettings.json` and Kestrel configuration at runtime. Setting the environment variable is the recommended way to temporarily change the URL/port without republishing.

## GitHub Actions artifacts and Releases

When the CI workflow runs it will create per-run artifacts that can be downloaded from the Actions run page. Those artifacts are tied to that specific run and are not permanently discoverable to end users. To make downloads discoverable you should attach the built archives to a GitHub Release.

Where to download from the Actions run
- Repo → Actions → open the successful run → scroll to **Artifacts** and click the platform-named archive (e.g. `VoiceLauncherBlazor-win-x64`) to download.

Make a public, discoverable Release (recommended)
- Manual: Repo → Releases → Draft a new release → select the tag (or create a new one) → attach the artifacts under **Assets** → Publish release.
- CLI (GitHub CLI `gh`):

```pwsh
# authenticate first if needed
gh auth login

# create a release and attach files (adjust filenames as necessary)
gh release create v1.0.4 \
	./VoiceLauncherBlazor-win-x64.zip \
	./VoiceLauncherBlazor-linux-x64.tar.gz \
	--title "v1.0.4" --notes "Release v1.0.4"

# or upload a file to an existing release
gh release upload v1.0.4 ./VoiceLauncherBlazor-win-x64.zip
```

Direct download URL pattern for release assets

```
https://github.com/<owner>/<repo>/releases/download/<tag>/<asset-name>
```

Example:

```
https://github.com/Mark-Phillipson/VoiceLauncherBlazor/releases/download/v1.0.4/VoiceLauncherBlazor-win-x64.zip
```

Automating Releases from Actions
- The workflow already includes a `release` job that uses `softprops/action-gh-release@v1` to create a release and attach the built artifacts. If assets are not appearing on the Releases page, inspect the `release` job logs in the Actions run for errors (common issues: wrong artifact paths, release action failing to find files, or auth problems).

Troubleshooting tips
- If you see "No files were found with the provided path" in the upload step, the publish `--output` path and the `upload` `path:` must match. Adding a debug `ls` step after publishing helps locate the files.
- Actions artifacts are temporary (retention default 90 days). Use Releases for long-term distribution.

Triggering a release via tag (Remember to change the last number four to five for example for v1.0.5)

```pwsh
git tag v1.0.4 && git push origin v1.0.4
```
