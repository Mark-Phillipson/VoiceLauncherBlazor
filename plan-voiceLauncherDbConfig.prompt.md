## SQLite-only environment config plan for VoiceAdmin

### Goal
Use different SQLite connection strings for local development vs Azure production in VoiceAdmin, without SQL Server complexity.

### Environment names
- Development: `ASPNETCORE_ENVIRONMENT=Development`
- Production: `ASPNETCORE_ENVIRONMENT=Production`
- Optional Local mode: `ASPNETCORE_ENVIRONMENT=Local` (explicit local override)

### Appsettings files
- `appsettings.json` (shared defaults)
- `appsettings.Development.json` (local dev SQLite path)
- `appsettings.Production.json` (Azure SQLite/SQL location or AzureSQL in cloud)
- Optional `appsettings.Local.json` (local-only explicit path)

### Recommended connection strings
- Local dev: `Data Source=.|DataDirectory|\voiceadmin-dev.db` or `%APPDATA%/VoiceLauncher/voicelauncher.db`
- Production: Azure App Setting override for `ConnectionStrings:DefaultConnection` (point to app-managed database file or Azure SQL connection)

### Code behavior
- In `VoiceAdmin/Program.cs` use `builder.Configuration.GetConnectionString("DefaultConnection")`.
- For production, use `builder.Environment.IsProduction()` path to choose cloud-ready path.
- For local, use local fallback path in `[non-production]` code branch.

### Azure deployment
- Set App Service setting `ASPNETCORE_ENVIRONMENT=Production`.
- Set Connection String `DefaultConnection` from Azure portal.
- Ensure `appsettings.Production.json` exists in publish output but not storing secrets.

### Validation
- Local: `dotnet run` with dev settings, checks SQLite file created in local folder.
- Prod: Deploy, verify endpoint and DB record persistence works.

### Notes
- Do not hardcode absolute user paths in production. Use relative or environment-specific storage.
- If using 2 separate DBs, keep separate `appsettings.*` and/or use environment variable overrides.
