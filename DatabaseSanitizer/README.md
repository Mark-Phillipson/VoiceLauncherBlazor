# DatabaseSanitizer

This tool copies a source VoiceLauncher SQLite database to a sanitized destination database, removing sensitive launcher entries and generating dummy transaction/test data.

## Behavior
- Copies all categories (including sensitive) but keeps only non-sensitive category child records.
- Preserves web URLs (`http://`, `https://`, etc.) in `Launcher.CommandLine`.
- Excludes local file path launchers:
  - `file://...`, `\\server\share...`, `C:\...`, `C:/...`, `./...`, `../...`, `/...`
  - also handles quoted values like `"C:\path\app.accdb"`.
- Copies related tables (`CustomIntelliSenses`, `LauncherCategoryBridges`, `AdditionalCommands`, etc.) with safe filters.
- Replaces financial data with fake transactions and values.
- Resets `Logins` to a demo account.

## Usage
From repo root:

```powershell
cd C:\Users\MPhil\source\repos\VoiceLauncherBlazor\DatabaseSanitizer
dotnet run -- --source "C:\Users\MPhil\AppData\Roaming\VoiceLauncher\voicelauncher.db" --output "C:\Users\MPhil\source\repos\VoiceLauncherBlazor\publish\sanitized.db"
```

- `--source` = path to existing source SQLite DB
- `--output` = target sanitized DB path

## Validation
- After run, check `DatabaseSanitizerReport.txt` in output folder for row counts:
  - Non-sensitive launchers copied
  - Local file launchers excluded

## Build
```powershell
cd C:\Users\MPhil\source\repos\VoiceLauncherBlazor
dotnet build --configuration Debug
```
