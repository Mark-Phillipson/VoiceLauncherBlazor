# GitHub Copilot Instructions for VoiceLauncherBlazor

**CRITICAL: Always follow these instructions first and only search for additional context if the information here is incomplete or found to be in error.**

## Repository Overview
VoiceLauncherBlazor is a Blazor Server application designed for voice-controlled development and accessibility, integrating with Talon Voice and Cursorless for hands-free coding. It maintains speech recognition database tables for enhanced voice coding functionality and includes desktop integration via WinForms.

## Quick Start - Essential Commands
```bash
# Install .NET SDK 9.0.305 (REQUIRED - see global.json)
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 9.0.305
export PATH="$HOME/.dotnet:$PATH"

# Build and run main application (WORKS RELIABLY)
cd TalonVoiceCommandsServer
dotnet restore                    # ~1 second
dotnet build --configuration Debug  # ~1.6 seconds - NEVER CANCEL
dotnet run --configuration Debug    # Runs on http://localhost:5008
```

## Critical Build Information

### ✅ WORKING PROJECTS
- **TalonVoiceCommandsServer** - Main Blazor Server app - **Build time: 1.6 seconds - NEVER CANCEL**
- **VoiceAdmin.ServiceDefaults** - .NET Aspire components
- **Application fully functional** - All 4 UI tabs work correctly

### ❌ PROJECTS WITH KNOWN ISSUES
- **DataAccessLibrary** - FAILS due to network dependency (SmartComponents.LocalEmbeddings downloads from huggingface.co)
- **TestProjectxUnit** - CANNOT RUN due to DataAccessLibrary dependency  
- **WinFormsApp** - Windows-only, cannot build on Linux
- **Full solution build** - BLOCKED by above dependencies

### ⚠️ NEVER CANCEL BUILD COMMANDS
- Main app build: **1.6 seconds - Set timeout to 5+ minutes**
- Package restore: **~1 second - Set timeout to 5+ minutes**  
- Always wait for completion - builds may appear to hang but are working

## Validation Scenarios - ALWAYS TEST THESE

After making any changes to TalonVoiceCommandsServer, **ALWAYS** run this complete validation:

```bash
# 1. Build and start server
cd TalonVoiceCommandsServer
dotnet build --configuration Debug  # WAIT FOR COMPLETION
dotnet run --configuration Debug &  # Starts on http://localhost:5008
SERVER_PID=$!

# 2. Manual validation using browser or Playwright
# Test ALL four main tabs:
#   - Search Commands tab (http://localhost:5008)
#   - Import Scripts tab  
#   - Analysis Report tab
#   - Lists tab

# 3. Test search functionality
#   - Enter search term like "open file"
#   - Verify shows "No results found" (expected when no data imported)
#   - Verify all filters and buttons work

# 4. Clean shutdown
kill $SERVER_PID
```

### Critical UI Validation Points
- **Search Commands Tab**: Search form, scope dropdown, all filter buttons
- **Import Scripts Tab**: Directory path input, import buttons, lists import
- **Analysis Report Tab**: Should load without errors
- **Lists Tab**: Should show "No lists available" when no data imported
- **All tabs must be clickable and functional**

## Project Structure & Key Files

### Main Application
- **TalonVoiceCommandsServer/** - Main Blazor Server app (PORT: 5008/5269)
  - `Components/` - Blazor components (.razor + .razor.cs files)
  - `Program.cs` - Application startup
  - `appsettings.json` - Configuration
  - `Properties/launchSettings.json` - Port configuration (5269 for dev, 5008 for production)

### Supporting Projects  
- **VoiceLauncher/** - Original startup logic and DI config
- **DataAccessLibrary/** - EF Core with SQL Server (**BUILD FAILS - network dependency**)
- **RazorClassLibrary/** - Shared Razor components
- **TestProjectxUnit/** - XUnit tests (**CANNOT RUN - depends on DataAccessLibrary**)
- **WinFormsApp/** - Blazor Hybrid desktop app (**Windows-only**)

### Important Configuration Files
- `global.json` - Requires .NET SDK 9.0.305 EXACTLY
- `VoiceLauncherBlazor.sln` - Main solution file
- `.github/workflows/publish.yml` - CI/CD pipeline

## Database & Entity Framework

- **Uses SQL Server** (not SQLite)
- **Connection**: Entity Framework Core with SQL Server provider
- **Syntax**: Use SQL Server T-SQL syntax (`SELECT TOP 10`, `ISNULL()`, `GETDATE()`)
- **Migrations**: Create command-line scripts - DO NOT run `Update-Database` directly
  ```bash
  dotnet ef migrations add MigrationName
  ```

## Development Guidelines

### Component Structure
- Use `.razor` (markup/UI) and `.razor.cs` (code-behind) files
- Use `.razor.css` for component styles (NO inline `<style>`)
- Bootstrap classes for layout and controls
- Semantic HTML with `aria-` attributes for accessibility

### Build Configuration
- **ALWAYS use Debug builds** for development and testing
- **DO NOT use Release builds** for development
- Build command: `dotnet build --configuration Debug`

### Testing
- **Primary tests**: TestProjectxUnit (XUnit framework)
- **Cannot run tests** due to DataAccessLibrary network dependency
- **Manual testing required** - use Playwright for UI validation
- **DO NOT create console apps for tests**

## Accessibility & Voice Integration

- **Optimized for hands-free use** (Talon Voice, Cursorless)
- **Keyboard navigation** - all controls accessible via keyboard
- **Voice commands** - integrates with Talon Voice speech recognition
- **Screen reader compatible** - semantic HTML structure

## Common Commands & Scripts

### Application Management
```bash
# Start main application
cd TalonVoiceCommandsServer && dotnet run

# Check if app is running (kills existing processes on port 5008)
./stop-processes-on-port-5008.ps1  # PowerShell script

# Test Talon import functionality  
./test-talon-import.ps1

# Test search functionality
./test-talon-search.ps1
```

### Port Configuration
- **Development**: http://localhost:5269 (launchSettings.json)
- **Production**: http://localhost:5008 (default Kestrel)
- **Playwright tests**: Use http://localhost:5008

## Known Working Patterns

### If Build Fails
```bash
# Only try to build main project - avoid full solution
cd TalonVoiceCommandsServer
dotnet clean
dotnet restore  
dotnet build --configuration Debug
```

### If Network Issues
- DataAccessLibrary WILL FAIL due to SmartComponents.LocalEmbeddings
- This is expected and does not affect main application functionality
- **Continue with TalonVoiceCommandsServer only**

### Testing Without Full Test Suite
```bash
# Use Playwright for manual UI testing
cd PlaywrightTests
# Note: Playwright installation may fail in restricted environments
# Use manual browser testing as fallback
```

## Publishing & Deployment

### Development
```bash
cd TalonVoiceCommandsServer
dotnet publish --configuration Release --self-contained true --runtime linux-x64 --output ./publish
```

### Windows
```bash
dotnet publish --configuration Release --self-contained true --runtime win-x64 --output ./publish
```

See `TalonVoiceCommandsServer/publishing.md` for complete deployment instructions.

## Troubleshooting

### "SmartComponents.LocalEmbeddings" Error
- **Expected behavior** - network restricted environment
- **Solution**: Work with TalonVoiceCommandsServer project only
- **Impact**: Main application still works perfectly

### ".NET SDK not found" Error  
- **Required**: .NET SDK 9.0.305 exactly (see global.json)
- **Install**: Use dotnet-install script above

### "Windows API" Errors on Linux
- **Fixed** - TalonVoiceCommandsServer includes cross-platform compatibility
- Application works on both Windows and Linux

### Port Conflicts
- Check if port 5008 is in use: `netstat -tulpn | grep 5008`
- Kill existing processes: `./stop-processes-on-port-5008.ps1`

## File Locations Quick Reference

```
📁 Key Directories
├── TalonVoiceCommandsServer/     # Main app - ALWAYS WORKS
├── WinFormsApp/                  # Windows desktop app
├── DataAccessLibrary/            # EF Core - BUILD FAILS  
├── TestProjectxUnit/             # Tests - CANNOT RUN
├── .github/workflows/            # CI/CD pipeline
└── TalonVoiceCommandsServer/publishing.md  # Deployment guide

🔧 Configuration Files
├── global.json                   # .NET SDK version requirement
├── VoiceLauncherBlazor.sln      # Main solution
└── TalonVoiceCommandsServer/Properties/launchSettings.json  # Port config
```

## Legacy Context (Preserved for Reference)

### Terminal Interactions
- If you're going to run a dotnet build always check to see if the current application is running and then shut it down first.
- If you are asked to use playwright tools to demonstrate the application working always make sure the application is running first.

### Talon Lists and Captures
- In talon files, captures are `{}` and lists are `<>`.

### Important Current Process
- Only change the Talon Voice Commands Server project.
- Do not change other projects (e.g., Razor Class Library).

### Chat Messaging
- Messages may contain dictation errors.
- Interpret context and similar-sounding words as needed.
- When returning a response always please to keep it down to a minimum brevity and summarization is the key.

### Github Repository
- Repo: https://github.com/Mark-Phillipson/VoiceLauncherBlazor

## Success Criteria
When following these instructions, you should be able to:
1. ✅ Build TalonVoiceCommandsServer in ~1.6 seconds
2. ✅ Run application on http://localhost:5008  
3. ✅ Navigate all 4 UI tabs successfully
4. ✅ Perform search operations (shows "No results found" when no data)
5. ✅ Make changes and rebuild without errors

**Remember**: Focus on TalonVoiceCommandsServer project - it contains the core functionality and builds reliably. Ignore build failures in DataAccessLibrary and testing dependencies.