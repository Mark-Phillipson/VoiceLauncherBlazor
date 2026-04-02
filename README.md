# VoiceLauncherBlazor

##Click to Install:

[![GitHub Release](https://img.shields.io/github/v/release/Mark-Phillipson/VoiceLauncherBlazor)](https://github.com/Mark-Phillipson/VoiceLauncherBlazor/releases/latest)

A Blazor Server application designed for voice-controlled development and accessibility, integrating with Talon Voice and Cursorless for hands-free coding. It includes a To-Do section and maintains speech recognition database tables for enhanced voice coding functionality, such as custom IntelliSense and application launching. It also supports Talon command search/application launcher workflows so users can trigger app actions via voice.

## Downloads

- Latest release: [View on GitHub Releases](https://github.com/Mark-Phillipson/VoiceLauncherBlazor/releases/latest)
- Latest Windows build: [VoiceLauncherBlazor-win-x64.zip](https://github.com/Mark-Phillipson/VoiceLauncherBlazor/releases/latest/download/VoiceLauncherBlazor-win-x64.zip)

## Install guide

- Full installation and startup instructions are in [docs/root-markdown/INSTALL.md](docs/root-markdown/INSTALL.md) (recommended for end users).

## Windows release run & database setup (no config editing)

1. Extract `VoiceLauncherBlazor-win-x64.zip`.
2. Make sure `voicelauncher-azure.db` is in the same folder as `VoiceAdmin.exe` (it is included in the published artifact).
3. Option A (recommended): run from the app folder with explicit absolute path:

```powershell
cd C:\Users\<you>\Downloads\VoiceLauncherBlazor-win-x64\VoiceAdmin
setx ASPNETCORE_ENVIRONMENT Production
setx ConnectionStrings__DefaultConnection "Data Source=C:\Users\<you>\Downloads\VoiceLauncherBlazor-win-x64\VoiceAdmin\voicelauncher-azure.db"
\VoiceAdmin.exe
```

4. Option B (alternate): set the path from command line without editing the JSON:

```powershell
cd C:\Users\<you>\Downloads\VoiceLauncherBlazor-win-x64\VoiceAdmin
$env:ASPNETCORE_ENVIRONMENT='Production'
$env:ConnectionStrings__DefaultConnection='Data Source=C:\Users\<you>\Downloads\VoiceLauncherBlazor-win-x64\VoiceAdmin\voicelauncher-azure.db'
.\VoiceAdmin.exe
```

5. If the database is in the same folder and the app is launched there, you can use the default `appsettings.json` entry:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=voicelauncher-azure.db"
}
```

No further editing of `appsettings` is required for standard ZIP-based release consumption.

## Features

- **Voice Commands Management**: Maintain database tables for speech recognition, enabling voice-triggered IntelliSense and commands in Visual Studio and VS Code.

## Talon Voice command launch (Windows)

If you use Talon Voice with custom handoff commands, you can configure an action to launch the WinForms app using an absolute path. Add this to your Talon action file (`open_application.py` or similar):

- Example Talon action file: [docs/open_application_example.py](docs/open_application_example.py)
- Example Talon command spec file: [docs/voice_admin_commands_example.talon](docs/voice_admin_commands_example.talon)

Add the action code:

```python
from talon import Module, ui
mod = Module()

@mod.action_class
class Actions:
    def run_application_voice_admin_windows_forms(self, searchTerm: str):
        """Runs VoiceAdmin WinForms app with the given search term"""
        commandline = r"C:\Users\<you>\path\to\VoiceLauncherBlazor\WinFormsApp\bin\Release\net10.0-windows\WinFormsApp.exe"
        args = [' /SearchIntelliSense', f'/{searchTerm}']
        ui.launch(path=commandline, args=args)
```

- Replace `C:\Users\<you>\path\to\...` with your actual install path.
- For different commands, define additional actions using your preferred argument form.
- Start Talon and invoke action by voice with: `run application voice admin windows forms "<term>"` (or your Talon phrase mapping).
- This can also be used as a quick way to search Talon commands and execute the corresponding VoiceAdmin launcher path in one step.

- For a shared path override (no code edit each release), use a user config variable or wrap command in a batch/PowerShell script that sets the path before launching.
- **Application Launcher**: Launch applications, folders, or websites by category or voice command.
- **Popular Commands**: Predefined voice commands for common coding tasks.
- **To-Do Section**: Basic task management.
- **Accessibility**: Optimized for hands-free use with Talon Voice and Cursorless.
- **Hybrid Support**: Includes a Blazor Hybrid WinForms project for desktop integration (Windows Only).

## Project Structure

- **VoiceAdmin**: Main Blazor Server application (replaces the deprecated `TalonVoiceCommandsServer`).
- **WinFormsApp**: Blazor Hybrid WinForms project (Windows-only).
- **DataAccessLibrary**: EF Core with SQL Server for data access.
- **TestProjectxUnit**: Unit tests using XUnit.

> **Note:** The `TalonVoiceCommandsServer` project is deprecated; its folder remains for historical reference and is marked with `TalonVoiceCommandsServer/DEPRECATED.md`.

## Technologies

- **Frontend**: Blazor Server with Bootstrap 5 for responsive UI.
- **Backend**: ASP.NET Core, Entity Framework Core.
- **Database**:  SQLite.
- **Tools**: Talon Voice, Cursorless (VS Code extension), Visual Studio / VS Code.
- **Testing**: XUnit.

## Development Setup

1. Clone the repository: `git clone https://github.com/Mark-Phillipson/VoiceLauncherBlazor`
2. Navigate to `VoiceAdmin/`.
3. Restore dependencies: `dotnet restore`
4. Build: `dotnet build --configuration Debug`
5. Run: `dotnet run` (runs on the configured port, check `appsettings.json`).

## Publishing

For release/publishing instructions, see [docs/root-markdown/PUBLISHING.md](docs/root-markdown/PUBLISHING.md).

## Database

- Uses SQLite by design for all release builds and local deploys (`voicelauncher-azure.db`, bundled in `VoiceAdmin/wwwroot`).
- Migrations: create scripts via EF Core commands (do not run `Update-Database` directly in production environments).
- Example: `dotnet ef migrations add MigrationName`

## Quick start (VoiceAdmin cross-platform release)

1. Download the artifact for your platform:
   - Windows: `VoiceLauncherBlazor-win-x64.zip`
   - Linux: `VoiceLauncherBlazor-linux-x64.tar.gz`
   - macOS (ARM64): `VoiceLauncherBlazor-osx-arm64.tar.gz`
2. Extract archive.
3. Ensure `voicelauncher-azure.db` is in the same folder as `VoiceAdmin` executable.
4. Run:
   - Windows: `VoiceAdmin.exe`
   - Linux/macOS: `chmod +x VoiceAdmin && ./VoiceAdmin`
5. Open browser to the configured app URL (as shown in logs from app startup).

## DB override

- Set environment variable `ConnectionStrings__DefaultConnection` to a filesystem path, e.g.:

  `ConnectionStrings__DefaultConnection="Data Source=/path/to/voicelauncher-azure.db"`

- Or edit `VoiceAdmin/appsettings.json` to override the default connection.

## Data Sanitizer (SQLite demo)

This repository includes a helper tool to generate a sanitized copy of `voicelauncher.db` called `voicelauncher-azure.db`.

Steps:
1. Ensure source DB exists and is current:
   - `C:\Users\MPhil\AppData\Roaming\VoiceLauncher\voicelauncher.db`
2. Run the sanitizer from repo root:
   - `dotnet run --project DatabaseSanitizer\DatabaseSanitizer.csproj -- --source "C:\Users\MPhil\AppData\Roaming\VoiceLauncher\voicelauncher.db" --output "C:\Users\MPhil\source\repos\VoiceLauncherBlazor\voicelauncher-azure.db"`
3. Verify report `DatabaseSanitizerReport.txt` includes:
   - `Total Categories in source: ...`
   - `Sensitive Categories: ...`
   - `Non-sensitive Categories: ...`
   - `Copied all categories (including sensitive): ...`
   - `Sensitive categories excluded from child entity copying: ...`
4. Sanity checks (SQL query):
   - `SELECT COUNT(*) FROM Categories;`
   - `SELECT COUNT(*) FROM Categories WHERE Sensitive=1;`
   - `SELECT COUNT(*) FROM Categories WHERE Sensitive=0;`

Rules enforced by sanitizer:
- keeps all categories but removes sensitive child records
- preserves FK relationships by filtering dependent rows before copy
- produces dummy transactions and values (fake data) for demo mode
- writes output DB and report for human review

## Testing

Run tests: `dotnet test` in the TestProjectxUnit directory.

## Contributing

- Follow component structure: Use `.razor` and `.razor.cs` files.
- Maintain accessibility with semantic HTML and `aria-` attributes.
- Test with Talon Voice and Cursorless.
- Use Bootstrap for styling.

## License

See LICENSE file.