# Migration Plan: VoiceLauncher → VoiceAdmin (.NET 10)

**Created:** November 29, 2025  
**Target:** Migrate all VoiceLauncher functionality to VoiceAdmin (.NET 10 template)  
**Current Status:** Planning Phase

---

## Overview

This plan details the migration of the VoiceLauncher Blazor Server application to the new VoiceAdmin project created with the .NET 10 template. The goal is to preserve all functionality while modernizing the project structure to align with .NET 10 best practices.

---

## Phase 1: Project Configuration & Dependencies

### 1.1 Update VoiceAdmin.csproj
- [ ] Add all NuGet package references from VoiceLauncher.csproj
  - Blazored.Modal (7.3.1) - ✅ Compatible with .NET 10
  - Blazored.Toast (4.2.1) - ✅ Compatible with .NET 10
  - Humanizer.Core (3.0.0-beta.54)
  - InputSimulatorCore (1.0.5)
  - Markdig (0.38.0)
  - Microsoft.EntityFrameworkCore.SqlServer (10.0.0)
  - Microsoft.EntityFrameworkCore.Tools (10.0.0)
  - Microsoft.Identity.Client (4.73.1)
  - **Radzen.Blazor (8.3.4)** - ⚠️ **UPDATE from 5.6.1 to 8.3.4 for .NET 10 support**
  - SmartComponents.AspNetCore (0.1.0-preview10148)
  - SmartComponents.LocalEmbeddings (0.1.0-preview10148)
  - System.Linq.Dynamic.Core (1.6.0)
  - AutoMapper (13.0.1)

### 1.2 Add Project References
- [ ] Add reference to DataAccessLibrary project
- [ ] Add reference to RazorClassLibrary project
- [ ] Add reference to VoiceAdmin.ServiceDefaults project

### 1.3 Configure Project Properties
- [ ] Add UserSecretsId (use existing: a71df59a-e582-4e4f-9a75-14512ec2b2e6)
- [ ] Add platform configurations (AnyCPU;x64)
- [ ] Add publish settings (ReadyToRun, SelfContained, RuntimeIdentifier: win-x64)
- [ ] Set ErrorOnDuplicatePublishOutputFiles to false
- [ ] Update Nullable and ImplicitUsings (already set)

---

## Phase 2: Configuration Files

### 2.1 AppSettings Migration
- [ ] Copy appsettings.json from VoiceLauncher to VoiceAdmin
  - ConnectionStrings section
  - JsonRepository configuration
  - OpenAI/SmartComponents configuration (if present)
  - Logging configuration
  - Any custom configuration sections

### 2.2 Additional Configuration Files
- [ ] Copy/create .config directory contents (if any)
- [ ] Copy libman.json (library manager configuration)
- [ ] Review and migrate launcherCache.json handling
- [ ] Copy Prompts.json
- [ ] Verify User Secrets are properly configured

---

## Phase 3: Program.cs & Dependency Injection

### 3.1 Update Program.cs Services
- [ ] Replace default service registration with VoiceLauncher services
- [ ] Add Blazored.Modal service
- [ ] Add Blazored.Toast service
- [ ] Add ComponentCacheService (scoped)
- [ ] Configure DbContext with SQL Server connection
- [ ] Add SmartComponents configuration with OpenAI backend
- [ ] Add LocalEmbedder (singleton)
- [ ] Add Radzen components
- [ ] Configure AutoMapper with reflection handling

### 3.2 Register Business Services
- [ ] CategoryService
- [ ] CreateCommands
- [ ] AdditionalCommandService
- [ ] LanguageService
- [ ] LauncherService
- [ ] ComputerService
- [ ] CustomIntellisenseService
- [ ] GeneralLookupService
- [ ] AppointmentService
- [ ] VisualStudioCommandService
- [ ] CommandSetService
- [ ] LauncherMultipleLauncherBridgeDataService
- [ ] NotifierService (singleton)
- [ ] WindowsService (singleton)
- [ ] TalonAnalysisService

### 3.3 Register Repository & Data Services
- [ ] SavedMousePosition (Repository & DataService)
- [ ] CustomWindowsSpeechCommand (Repository & DataService)
- [ ] WindowsSpeechVoiceCommand (Repository & DataService)
- [ ] GrammarName (Repository & DataService)
- [ ] GrammarItem (Repository & DataService)
- [ ] HtmlTag (Repository & DataService)
- [ ] ApplicationDetail (Repository & DataService)
- [ ] Idiosyncrasy (Repository & DataService)
- [ ] PhraseListGrammar (Repository & DataService)
- [ ] Launcher (Repository & DataService)
- [ ] Category (Repository & DataService)
- [ ] ValueToInsert (Repository & DataService)
- [ ] SpokenForm (Repository & DataService)
- [ ] Microphone (Repository & DataService)
- [ ] CustomIntelliSense (Repository & DataService)
- [ ] TalonAlphabet (Repository & DataService)
- [ ] Prompt (Repository & DataService)
- [ ] Language (Repository & DataService)
- [ ] CursorlessCheatsheetItem (Repository & DataService)
- [ ] CssProperty (Repository & DataService)
- [ ] Transaction (Repository & DataService)
- [ ] TransactionTypeMapping (Repository & DataService)
- [ ] Example (Repository & DataService)
- [ ] QuickPrompt (Repository & DataService)
- [ ] TalonVoiceCommand (DataService)
- [ ] TalonList (Repository & DataService)
- [ ] CursorlessCheatsheetItemJsonRepository

### 3.4 Configure JsonRepository Options
- [ ] Register JsonRepositoryOptions configuration

### 3.5 Update Middleware Pipeline
- [ ] Replace default Blazor middleware with Blazor Server Hub
- [ ] Add UseStaticFiles or MapStaticAssets
- [ ] Add UseRouting
- [ ] Add MapBlazorHub
- [ ] Add MapFallbackToPage("/_Host")
- [ ] Add MapDefaultEndpoints (for service defaults)
- [ ] Configure SmartComboBox API endpoints
  - /api/cursorless-spokenforms
  - /api/suggestions/expense-category
  - /api/suggestions/issue-label
- [ ] Initialize LocalEmbedder embeddings on startup

---

## Phase 4: Component Migration

### 4.1 Update App.razor
- [ ] Copy VoiceLauncher App.razor to VoiceAdmin
- [ ] Verify routing configuration
- [ ] Update any component references

### 4.2 Update _Imports.razor
- [ ] Copy all using statements from VoiceLauncher
- [ ] Ensure Blazored.Modal and Toast imports
- [ ] Ensure Radzen imports
- [ ] Ensure RazorClassLibrary imports

### 4.3 Components from RazorClassLibrary
- [ ] Verify all RazorClassLibrary components are accessible
- [ ] No migration needed - they're in the referenced library
- [ ] Pages directory appears empty in VoiceLauncher (all in RazorClassLibrary)

### 4.4 Layout Components
- [ ] Verify layout components are in RazorClassLibrary
- [ ] No direct migration needed for VoiceAdmin

---

## Phase 5: Static Assets & wwwroot

### 5.1 CSS Files
- [ ] Copy css/animate.min.css
- [ ] Copy css/bootstrap-icons.min.css
- [ ] Copy css/site.css
- [ ] Copy css/prism.css

### 5.2 JavaScript Files
- [ ] Copy scripts/prism.js
- [ ] Copy exampleJsInterop.js

### 5.3 Images
- [ ] Copy entire images/ directory
  - All logo/icon files
  - All application screenshots
  - Background images

### 5.4 Data Files
- [ ] Copy CursorlessCheatsheetData.json
- [ ] Copy XML files (Productivity.xml, MyKBCommands_2016.xml, etc.)

### 5.5 Other Assets
- [ ] Copy favicon.ico
- [ ] Copy background.png

---

## Phase 6: Root-Level Files & Models

### 6.1 Service/Repository Files
- [ ] Copy CursorlessCheatsheetItemJsonRepository.cs
- [ ] Verify all other repositories are in DataAccessLibrary or RazorClassLibrary

### 6.2 Utility/Helper Files
- [ ] Copy TestingOnly.cs (if needed for development)
- [ ] Review Scratchpad.sql (documentation/reference only)

### 6.3 Data Files
- [ ] Copy launcherCache.json (runtime file - may not need migration)
- [ ] Copy Prompts.json
- [ ] Copy Results.csv (if needed)

### 6.4 Solution File
- [ ] Update VoiceLauncherBlazor.sln to reference VoiceAdmin instead of VoiceLauncher
  - Or keep both during transition period

---

## Phase 7: Build & Test Configuration

### 7.1 Launch Settings
- [ ] Update Properties/launchSettings.json
- [ ] Set port to 5008 (per project standards)
- [ ] Configure HTTPS settings
- [ ] Configure environment variables

### 7.2 Build Verification
- [ ] Run `dotnet clean` on VoiceAdmin
- [ ] Run `dotnet build --configuration Debug` on VoiceAdmin
- [ ] Resolve any compilation errors
- [ ] Verify all dependencies resolve correctly

---

## Phase 8: Runtime Testing & Validation

### 8.1 Database Connection
- [ ] Verify connection string is correct
- [ ] Test database connectivity
- [ ] Verify EF migrations are accessible
- [ ] Test basic CRUD operations

### 8.2 Service Resolution
- [ ] Start application in Debug mode
- [ ] Verify all DI services resolve without errors
- [ ] Check for any missing service registrations
- [ ] Verify AutoMapper profiles load correctly

### 8.3 UI/Component Testing
- [ ] Test home page loads
- [ ] Test navigation to key pages
- [ ] Test Blazored.Modal functionality
- [ ] Test Blazored.Toast functionality
- [ ] Test Radzen component rendering
- [ ] Test SmartComponents functionality

### 8.4 Feature Testing
- [ ] Test Talon voice command search
- [ ] Test Cursorless cheatsheet functionality
- [ ] Test launcher functionality
- [ ] Test custom IntelliSense features
- [ ] Test Windows speech command features
- [ ] Test all CRUD operations for major entities

### 8.5 Use Playwright for UI Validation
- [ ] Use Playwright browser tools to inspect running app
- [ ] Take screenshots of key pages
- [ ] Verify accessibility/DOM structure

---

## Phase 9: Cleanup & Finalization

### 9.1 Remove VoiceLauncher References
- [ ] Update solution file to remove VoiceLauncher (or mark as obsolete)
- [ ] Update any documentation referencing VoiceLauncher
- [ ] Update README.md

### 9.2 Update Documentation
- [ ] Update .github/copilot-instructions.md with VoiceAdmin references
- [ ] Update any project-specific documentation
- [ ] Update deployment documentation

### 9.3 Git/Version Control
- [ ] Create feature branch for migration (upgrade-dotnet-10)
- [ ] Commit migration in logical chunks
- [ ] Update .gitignore if needed
- [ ] Create PR for review

---

## Phase 10: Deployment Preparation

### 10.1 Build Configuration
- [ ] Test Debug build
- [ ] Test Release build (even though Debug is standard)
- [ ] Verify publish configuration

### 10.2 Connection Strings & Secrets
- [ ] Verify User Secrets for development
- [ ] Document production connection string setup
- [ ] Review any API keys or sensitive data handling

### 10.3 Deployment Testing
- [ ] Run `dotnet publish` command
- [ ] Test published output
- [ ] Verify all assets are included in publish

---

## Notes & Considerations

### Package Compatibility with .NET 10
- **Blazored.Toast 4.2.1**: ✅ Targets .NET 6.0+, fully compatible with .NET 10
- **Blazored.Modal 7.3.1**: ✅ Targets .NET 8.0+, fully compatible with .NET 10
- **Radzen.Blazor**: ⚠️ **Version 5.6.1 → 8.3.4 REQUIRED**
  - Version 8.3.0+ adds official .NET 10 support (released Nov 11, 2025)
  - Current latest: 8.3.4 (released Nov 26, 2025)
  - Major version jump (5.x → 8.x) may include breaking changes
  - Review [release notes](https://github.com/radzenhq/radzen-blazor/releases) for migration guidance
  - Active development: CI workflow updated to .NET 10, demos support .NET 10

### Key Differences: VoiceLauncher vs VoiceAdmin
- **VoiceLauncher**: .NET 10, Blazor Server with older template structure
- **VoiceAdmin**: .NET 10, new Blazor template with modern render modes
- VoiceAdmin uses `AddRazorComponents()` and `AddInteractiveServerComponents()`
- VoiceLauncher uses `AddServerSideBlazor()`

### Important Files NOT in VoiceLauncher Root
- All Razor pages/components are in RazorClassLibrary
- No Models directory in VoiceLauncher (all in DataAccessLibrary)
- Components/Pages folder is empty in VoiceLauncher

### Migration Strategy
1. **Configuration First**: Get project file and DI working
2. **Then Static Assets**: Copy wwwroot
3. **Finally Test**: Use Playwright to validate

### Port Configuration
- VoiceAdmin should run on port **5008** (per project standards)
- Update launchSettings.json accordingly

---

## Risk Assessment

### High Risk Items
- [ ] AutoMapper reflection configuration (has error handling)
- [ ] Database connection and EF Core context
- [ ] SmartComponents/OpenAI configuration
- [ ] LocalEmbedder initialization

### Medium Risk Items
- [ ] Service registration completeness
- [ ] Static file serving (MapStaticAssets vs UseStaticFiles)
- [ ] Routing configuration changes
- [ ] **Radzen.Blazor upgrade from 5.6.1 to 8.3.4** (review for breaking changes)

### Low Risk Items
- [ ] wwwroot asset copying
- [ ] Configuration file copying
- [ ] NuGet package restoration

---

## Rollback Plan

If migration encounters critical issues:
1. VoiceLauncher project remains intact and functional
2. Can continue using VoiceLauncher while troubleshooting VoiceAdmin
3. Git branch allows easy rollback
4. Both projects can coexist during transition

---

## Success Criteria

- [ ] VoiceAdmin builds without errors
- [ ] All services resolve via DI without exceptions
- [ ] Application starts and runs on port 5008
- [ ] Database connectivity works
- [ ] All key features functional:
  - Talon voice command management
  - Cursorless cheatsheet
  - Launcher functionality
  - Custom IntelliSense
  - Windows speech commands
- [ ] UI renders correctly with all styles/assets
- [ ] No console errors on startup
- [ ] Playwright tests can interact with the application

---

## Timeline Estimate

- **Phase 1-2**: 30-60 minutes (configuration)
- **Phase 3**: 60-90 minutes (DI and services)
- **Phase 4**: 15-30 minutes (components - minimal work)
- **Phase 5**: 30-45 minutes (static assets)
- **Phase 6**: 15-30 minutes (root files)
- **Phase 7**: 30-60 minutes (build testing)
- **Phase 8**: 60-120 minutes (runtime testing)
- **Phase 9**: 30-60 minutes (cleanup)
- **Phase 10**: 30-60 minutes (deployment prep)

**Total Estimated Time**: 5-9 hours (spread over multiple sessions)

---

## Next Steps

1. ✅ Review this plan
2. Begin Phase 1: Update VoiceAdmin.csproj
3. Proceed sequentially through phases
4. Update checkboxes as tasks complete
5. Document any issues encountered
6. Adjust plan as needed based on findings

---

**Migration Started**: [Date to be filled]  
**Migration Completed**: [Date to be filled]  
**Final Status**: [To be updated]
