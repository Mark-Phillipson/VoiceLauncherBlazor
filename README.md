# VoiceLauncherBlazor

A Blazor Server application designed for voice-controlled development and accessibility, integrating with Talon Voice and Cursorless for hands-free coding. It includes a To-Do section and maintains speech recognition database tables for enhanced voice coding functionality, such as custom IntelliSense and application launching.

## Features

- **Voice Commands Management**: Maintain database tables for speech recognition, enabling voice-triggered IntelliSense and commands in Visual Studio and VS Code.
- **Application Launcher**: Launch applications, folders, or websites by category or voice command.
- **Popular Commands**: Predefined voice commands for common coding tasks.
- **To-Do Section**: Basic task management.
- **Accessibility**: Optimized for hands-free use with Talon Voice and Cursorless.
- **Hybrid Support**: Includes a Blazor Hybrid WinForms project for desktop integration.

## Project Structure

- **VoiceAdmin**: Main Blazor Server application (replaces the deprecated `TalonVoiceCommandsServer`).
- **WinFormsApp**: Blazor Hybrid WinForms project (Windows-only).
- **DataAccessLibrary**: EF Core with SQL Server for data access.
- **TestProjectxUnit**: Unit tests using XUnit.

> **Note:** The `TalonVoiceCommandsServer` project is deprecated; its folder remains for historical reference and is marked with `TalonVoiceCommandsServer/DEPRECATED.md`.

## Technologies

- **Frontend**: Blazor Server with Bootstrap for responsive UI.
- **Backend**: ASP.NET Core, Entity Framework Core.
- **Database**: SQL Server (T-SQL syntax).
- **Tools**: Talon Voice, Cursorless (VS Code extension), Visual Studio / VS Code.
- **Testing**: XUnit.

## Development Setup

1. Clone the repository: `git clone https://github.com/Mark-Phillipson/VoiceLauncherBlazor`
2. Navigate to `VoiceAdmin/`.
3. Restore dependencies: `dotnet restore`
4. Build: `dotnet build --configuration Debug`
5. Run: `dotnet run` (runs on the configured port, check `appsettings.json`).

## Publishing

For release/publishing instructions, see `PUBLISHING.md` in the repository root.

## Database

- Uses SQLite by default for local deploys and GitHub artifact builds (`voicelauncher-azure.db`, bundled in `VoiceAdmin/wwwroot`).
- SQL Server is supported for existing production deployments, but the cross-platform release path uses SQLite.
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

## Data Sanitizer (SQLite for Azure demo)

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

## Screenshots

- Voice Command Main Form
- Action Voice Command Edit Form
- Launch by Category
- Social Links
- Popular Commands
- List of Languages

## Contributing

- Follow component structure: Use `.razor` and `.razor.cs` files.
- Maintain accessibility with semantic HTML and `aria-` attributes.
- Test with Talon Voice and Cursorless.
- Use Bootstrap for styling.

## License

See LICENSE file.