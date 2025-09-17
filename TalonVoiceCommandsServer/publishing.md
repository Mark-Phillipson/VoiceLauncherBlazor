# Publishing Guide for VoiceLauncherBlazor

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

For environment-specific overrides, set `ASPNETCORE_URLS` variable before running, e.g., `ASPNETCORE_URLS=http://localhost:9090 ./TalonVoiceCommandsServer`.