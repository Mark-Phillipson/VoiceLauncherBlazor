# VoiceLauncherBlazor Installation Guide

This guide covers cross-platform installation for the VoiceAdmin Blazor Server app via GitHub release artifacts.

## Supported platforms

- Windows x64 (win-x64)
- Linux x64 (linux-x64)
- macOS ARM64 (osx-arm64)

## Download artifact

1. On the release page, download:
   - `VoiceLauncherBlazor-win-x64.zip`
   - `VoiceLauncherBlazor-linux-x64.tar.gz`
   - `VoiceLauncherBlazor-osx-arm64.tar.gz`

2. Extract:
   - Windows: right-click → Extract All (or `tar -xf` from PowerShell)
   - Linux/macOS: `tar -xzf VoiceLauncherBlazor-<rid>.tar.gz`

## Run

- Windows:
  - `cd <extracted-folder>`
  - `VoiceAdmin.exe`

- Linux/macOS:
  - `cd <extracted-folder>`
  - `chmod +x VoiceAdmin`
  - `./VoiceAdmin`

## Validate DB bundle

- Ensure `voicelauncher-azure.db` is next to `VoiceAdmin` executable.
- The app uses this file by default with `DefaultConnection` in `VoiceAdmin/appsettings.json`.

## Optional DB path override

Use an environment variable:

- Windows (PowerShell):
  - `$env:ConnectionStrings__DefaultConnection = "Data Source=C:\\path\\to\\voicelauncher-azure.db"`
  - `VoiceAdmin.exe`

- Linux/macOS:
  - `export ConnectionStrings__DefaultConnection="Data Source=/path/to/voicelauncher-azure.db"`
  - `./VoiceAdmin`

## WinForms host note

- WinFormsApp is only built on Windows (`windows-latest`).
- If running on macOS/Linux, skip WinForms.

## Troubleshooting

- Check logs for startup errors and database path.
- Confirm port is not blocked; default host URL appears in the console.
