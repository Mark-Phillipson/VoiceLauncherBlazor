# Plan: SQLite to Azure App Service with Sanitized Data

## TL;DR
Revive the Azure App Service (`voicelauncherblazor.azurewebsites.net`) - currently offline due to SQL Server costs - by switching to bundled SQLite. Create a sanitized SQLite database that removes sensitive categories/credentials and populates demo tables with Bogus-generated data. Deploy as a free public demo app.

---

## Steps

### Phase 1: Create Database Sanitization Tool
1. Create new console app: `DatabaseSanitizer` in workspace root
	- **Purpose**: Generates a clean SQLite database with sensitive data excluded
	- **Input**: Connection to source SQLite database (local development)
	- **Output**: `voicelauncher-azure.db` (clean) + sanitization report

2. Install Bogus NuGet package in `DatabaseSanitizer`
	```
	dotnet add package Bogus
	```

3. Implement sanitization logic:
	- Copy all schema from source SQLite
	- Filter: Exclude all rows where `Category.Sensitive == true`
	- For related data (CustomIntelliSense, Logins, etc.) tied to sensitive categories -> exclude
	- Generate dummy data for:
	  - `Transactions` table: 100-500 realistic rows (Bogus faker)
	  - `Values` table: 50-200 rows matching transaction patterns
	  - `Logins` table: Leave empty or 1 demo user (no real credentials)
	- Copy non-sensitive data for all other tables
	- Generate CSV sanitization report (rows excluded, dummy rows added)

4. Execution:
	- Usage: `dotnet run -- --source "path/to/voicelauncher.db" --output "voicelauncher-azure.db"`
	- Success criteria: Azure database runs locally without errors

---

### Phase 2: Configure VoiceAdmin for Azure Deployment
1. **Update [VoiceAdmin/Program.cs](VoiceAdmin/Program.cs)**
	- Add environment detection (Development vs. Production/Azure)
	- If Azure environment -> use bundled `voicelauncher-azure.db` from app folder
	- If Development -> use local user profile database path
	- Connection string selection logic must be explicit

2. **Add Azure-specific appsettings**
	- Create [VoiceAdmin/appsettings.Production.json](VoiceAdmin/appsettings.Production.json)
	- Set `ASPNETCORE_ENVIRONMENT=Production` in Azure App Service config
	- Connection string: `Data Source=voicelauncher-azure.db; Mode=ReadWrite`

3. **Update [DatabaseConfiguration.cs](DataAccessLibrary/Configuration/DatabaseConfiguration.cs)**
	- Add `GetConnectionString(string environment)` overload
	- Production path: Bundle the `.db` file in app root or wwwroot

4. **Publish Profile**
	- Create `.pubxml` file in `VoiceAdmin/Properties/PublishProfiles/` for Azure App Service
	- Self-contained deployment (include runtime)
	- Include `voicelauncher-azure.db` in published output

---

### Phase 3: Build & Test Sanitized Database
1. Run `DatabaseSanitizer` tool locally to generate `voicelauncher-azure.db`
	- Verify no sensitive credentials are accessible
	- Verify dummy data in Transactions/Values tables
	- Run queries to confirm sensitive categories excluded

2. Test locally:
	- Copy `voicelauncher-azure.db` to VoiceAdmin project root
	- Run VoiceAdmin in Production environment locally
	- Verify app starts, migrations run (if needed)
	- Verify data loads correctly (no sensitive data visible)
	- Test UI functionality with dummy data

3. Success criteria:
	- No passwords exposed
	- No sensitive custom commands visible
	- Dummy transaction data appears realistic
	- App is fully functional

---

### Phase 4: Configure & Deploy to Existing Azure App Service
1. **In Azure Portal:**
	- Navigate to existing App Service: `voicelauncherblazor.azurewebsites.net`
	- Update Application Settings:
	  - `ASPNETCORE_ENVIRONMENT=Production`
	  - Remove or clear any SQL Server connection strings
	- Verify no persistent databases linked (will use bundled SQLite)

2. **Publish to Existing Service:**
	- Right-click VoiceAdmin -> Publish -> Select Azure App Service
	- Choose existing `voicelauncherblazor` app
	- Configure deployment options:
	  - Self-contained deployment (include runtime)
	  - **Ensure** `voicelauncher-azure.db` is included in files to publish
	  - Remove old SQL Server references if present

3. **Post-deployment verification:**
	- Navigate to `https://voicelauncherblazor.azurewebsites.net`
	- Verify app loads without 500 errors (database migration should run automatically)
	- Test core features (search, filter, launch)
	- Confirm only non-sensitive data is accessible
	- Verify no credentials or sensitive commands are visible

---

### Phase 5: Periodic Redployment Workflow (Ongoing)
When you need to push fresh development data to the cloud:

- This is a recurring process: each deployment overwrites the cloud SQLite file with the latest sanitized database.

1. **Generate updated sanitized database:**
	- Run `DatabaseSanitizer --source "path/to/latest/voicelauncher.db" --output "voicelauncher-azure.db"`
	- Review sanitization report to confirm correct exclusions and dummy data generation

2. **Update the project:**
	- Replace `VoiceAdmin/voicelauncher-azure.db` with the newly generated version
	- Optionally commit: `git add voicelauncher-azure.db && git commit -m "Refresh sanitized cloud database"`

3. **Redeploy to Azure:**
	- Right-click VoiceAdmin -> Publish -> Azure App Service
	- Confirm `.db` file is included in deployment package
	- Publish (takes ~1-2 minutes)

4. **Verify:**
	- Navigate to `https://voicelauncherblazor.azurewebsites.net`
	- Confirm new data is present and no sensitive data visible

**Optional: Automate with GitHub Actions**
	- Create workflow to run `DatabaseSanitizer`, commit updated `.db`, and auto-publish on schedule or manual trigger
	- Requires Azure deploy credentials stored as GitHub secrets
	- Ideal for frequent demo data refreshes

---

## Relevant Files
- [VoiceAdmin/Program.cs](VoiceAdmin/Program.cs#L55) - Current SQLite configuration
- [DataAccessLibrary/Configuration/DatabaseConfiguration.cs](DataAccessLibrary/Configuration/DatabaseConfiguration.cs) - Connection string logic
- [ApplicationDbContext.cs](DataAccessLibrary/Models/ApplicationDbContext.cs#L88) - EF DbContext with fallback chain
- [Category.cs](DataAccessLibrary/Models/Category.cs#L31) - Sensitive property
- [appsettings.json](VoiceAdmin/appsettings.json) - Current config (update for Production)

**Files to Create:**
- `DatabaseSanitizer/Program.cs` - Sanitization tool entry point
- `DatabaseSanitizer/DatabaseSanitizer.csproj` - Project file
- `VoiceAdmin/Properties/PublishProfiles/AzureAppService.pubxml` - Publish profile
- `VoiceAdmin/appsettings.Production.json` - Production configuration
- `voicelauncher-azure.db` - Generated clean database

---

## Verification
1. **Local sanitization test:**
	- Run DatabaseSanitizer and inspect output report
	- Verify sensitive rows excluded count > 0
	- Verify dummy data row counts for Transactions/Values

2. **Local production test:**
	- Set `ASPNETCORE_ENVIRONMENT=Production` locally
	- Run VoiceAdmin with bundled database
	- Navigate UI, verify no sensitive data visible
	- Query SQL (if available in UI) -> no credentials returned

3. **Azure deployment test:**
	- App launches without 500 errors
	- First-time access initializes database correctly
	- UI is responsive and functional
	- No sensitive data accessible via UI search or queries

4. **Post-launch:**
	- Document Azure app URL for public sharing
	- Set up basic monitoring (Application Insights optional)
	- Test from multiple browsers/devices

---

## Decisions
- **Deployment Target**: Overwrite existing App Service at `voicelauncherblazor.azurewebsites.net` (currently SQL Server -> will become SQLite)
- **Database Isolation**: Separate `voicelauncher-azure.db` for cloud; development uses local user profile
- **Dummy Data**: Bogus-generated for Transactions/Values for realism
- **Sensitive Exclusion**: Filter by `Category.Sensitive == true` + related tables
- **Database Bundling**: Include `.db` file in app root, referenced in Production connection string
- **Frequency**: Initial deployment + periodic updates (redeploy sanitized `.db` whenever local development data is refreshed)
- **Cloud DB Update Pattern**: Overwrite the deployed SQLite database on each refresh cycle after sanitizer completes

---

## Further Considerations
1. **Cost savings achieved:**
	- SQL Server on Azure was expensive -> SQLite bundled in app = $0 database costs
	- Running on Free/B1 tier will keep hosting costs minimal
	- App can now remain live as a free public demo

2. **Database migration during deployment:**
	- EF Core migrations will run automatically on app startup via `Database.Migrate()`
	- First deployment will initialize schema from the bundled `.db` file
	- No manual migration commands needed post-deploy

3. **SQLite scalability path:**
	- Current setup works perfectly for single instance (Free/B1 tier)
	- If you need to scale to multiple instances in future -> migrate to Azure SQL Database (straightforward with EF Core)
	- SQLite file persists in app root; to preserve cloud data changes between deployments, consider git-tracking the `.db` or setting up periodic backups