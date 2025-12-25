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

For deployment instructions, see [publishing.md](TalonVoiceCommandsServer/publishing.md).

## Database

- Uses SQL Server.
- Migrations: Create scripts via EF Core commands (do not run `Update-Database` directly).
- Example: `dotnet ef migrations add MigrationName`

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