# SQLite Migration Plan - Cross-Platform Database Support

## Overview
Migrate VoiceLauncherBlazor from SQL Server to SQLite for cross-platform deployment (Windows, Linux, macOS) with zero installation requirements for end users.

## Benefits
- ✅ Cross-platform: Works on Windows, Linux, and macOS
- ✅ Zero installation: No SQL Server or database engine required
- ✅ Single file database: Easy backup, migration, and distribution
- ✅ Lightweight: ~1MB database engine embedded in application
- ✅ Simple deployment: Database file auto-created on first run

## Migration Phases

### Phase 1: Add SQLite Support
**Tasks:**
1. Add NuGet packages to all projects using Entity Framework:
   - `Microsoft.EntityFrameworkCore.Sqlite` (version 10.0.0)
   - Remove `Microsoft.EntityFrameworkCore.SqlServer` dependency

**Projects to Update:**
- `DataAccessLibrary/DataAccessLibrary.csproj`
- `VoiceAdmin/VoiceAdmin.csproj`
- Any other projects with EF Core dependencies

**Commands:**
```bash
# In DataAccessLibrary
dotnet remove package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.0

# In VoiceAdmin
dotnet remove package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.0
```

---

### Phase 2: Update DbContext Configuration
**Files to Modify:**
- `VoiceAdmin/Program.cs`
- `WinFormsApp/Program.cs` (if applicable)
- Any other project with DbContext registration

**Change from:**
```csharp
builder.Services.AddDbContextFactory<ApplicationDbContext>(
    options => options.UseSqlServer(connectionString));
```

**Change to:**
```csharp
builder.Services.AddDbContextFactory<ApplicationDbContext>(
    options => options.UseSqlite(connectionString));
```

---

### Phase 3: Update Connection Strings
**Files to Modify:**
- `VoiceAdmin/appsettings.json`
- `VoiceAdmin/appsettings.Development.json`
- `TalonVoiceCommandsServer/appsettings.json`
- `WinFormsApp/appsettings.json`
- `BlazorAppTestingOnly/appsettings.json`
- `DataAccessLibrary/appsettings.json`

**SQL Server format:**
```json
"ConnectionStrings": {
  "VoiceLauncher": "Data Source=Localhost;Initial Catalog=VoiceLauncher;Integrated Security=True;..."
}
```

**SQLite format (development):**
```json
"ConnectionStrings": {
  "VoiceLauncher": "Data Source=voicelauncher.db"
}
```

**SQLite format (production - user AppData folder):**
```json
"ConnectionStrings": {
  "VoiceLauncher": "Data Source={AppDataPath}/VoiceLauncher/voicelauncher.db"
}
```

**Implementation:**
Create a helper method to resolve the database path:
```csharp
public static string GetDatabasePath()
{
    var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    var appFolder = Path.Combine(appDataPath, "VoiceLauncher");
    Directory.CreateDirectory(appFolder);
    return Path.Combine(appFolder, "voicelauncher.db");
}
```

---

### Phase 4: Convert T-SQL to SQLite Syntax
**Common Conversions Required:**

| SQL Server | SQLite |
|------------|--------|
| `SELECT TOP n` | `SELECT * LIMIT n` |
| `ISNULL(col, default)` | `COALESCE(col, default)` |
| `GETDATE()` | `datetime('now')` |
| `DATEADD()` | `datetime('now', '+1 day')` |
| `DATEDIFF()` | `julianday(date1) - julianday(date2)` |
| `NEWID()` | Not available (use C# `Guid.NewGuid()`) |
| `STRING_AGG()` | `GROUP_CONCAT()` |

**Files to Review:**
- All files in `DataAccessLibrary/` containing SQL queries
- Stored procedures (need to be converted to C# methods)
- Check for:
  - `*.sql` files
  - Raw SQL in repositories
  - LINQ queries using SQL Server-specific functions

**Search Command:**
```bash
# Find files with potential SQL Server syntax
grep -r "SELECT TOP\|ISNULL\|GETDATE\|DATEADD\|DATEDIFF\|NEWID\|STRING_AGG" DataAccessLibrary/ --include="*.cs"
```

---

### Phase 5: Delete and Recreate Migrations
**Reason:** Existing migrations contain SQL Server-specific syntax and annotations.

**Steps:**
1. **Backup current database** (if needed):
   ```bash
   # Export data from SQL Server to SQL scripts or CSV
   ```

2. **Delete existing migrations:**
   ```bash
   rm -rf DataAccessLibrary/Migrations
   ```

3. **Create initial SQLite migration:**
   ```bash
   cd DataAccessLibrary
   dotnet ef migrations add InitialSQLiteSchema --startup-project ../VoiceAdmin
   ```

4. **Review generated migration** for any issues

5. **Create database:**
   ```bash
   dotnet ef database update --startup-project ../VoiceAdmin
   ```

---

### Phase 6: Add Auto-Migration on Startup
**Files to Modify:**
- `VoiceAdmin/Program.cs`
- `WinFormsApp/Program.cs`

**Add before `app.Run()`:**
```csharp
// Auto-apply migrations and create database if needed
using (var scope = app.Services.CreateScope())
{
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    using var context = dbContextFactory.CreateDbContext();
    
    try
    {
        // Ensure database is created and migrations are applied
        context.Database.Migrate();
        Console.WriteLine("Database initialized successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing database: {ex.Message}");
        throw;
    }
}
```

---

### Phase 7: Update Configuration for Cross-Platform Paths
**Create configuration helper class:**

```csharp
// DataAccessLibrary/Configuration/DatabaseConfiguration.cs
public static class DatabaseConfiguration
{
    public static string GetDatabasePath()
    {
        // Use platform-appropriate path
        var appDataPath = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData,
            Environment.SpecialFolderOption.Create);
        
        var appFolder = Path.Combine(appDataPath, "VoiceLauncher");
        Directory.CreateDirectory(appFolder);
        
        return Path.Combine(appFolder, "voicelauncher.db");
    }
    
    public static string GetConnectionString()
    {
        return $"Data Source={GetDatabasePath()}";
    }
}
```

**Update Program.cs to use helper:**
```csharp
// Instead of reading from appsettings, use helper
var connectionString = DatabaseConfiguration.GetConnectionString();
builder.Services.AddDbContextFactory<ApplicationDbContext>(
    options => options.UseSqlite(connectionString));
```

---

### Phase 8: Create Publish Profiles
**Create publish profiles for each platform:**

**Windows (win-x64):**
```bash
dotnet publish VoiceAdmin/VoiceAdmin.csproj -c Release -r win-x64 --self-contained true -o ./publish/win-x64
```

**Linux (linux-x64):**
```bash
dotnet publish VoiceAdmin/VoiceAdmin.csproj -c Release -r linux-x64 --self-contained true -o ./publish/linux-x64
```

**macOS (osx-x64):**
```bash
dotnet publish VoiceAdmin/VoiceAdmin.csproj -c Release -r osx-x64 --self-contained true -o ./publish/osx-x64
```

**macOS Apple Silicon (osx-arm64):**
```bash
dotnet publish VoiceAdmin/VoiceAdmin.csproj -c Release -r osx-arm64 --self-contained true -o ./publish/osx-arm64
```

---

### Phase 9: Testing Plan

#### 9.1 Development Testing
- [ ] Test database creation on first run
- [ ] Verify all migrations apply successfully
- [ ] Test CRUD operations on all entities
- [ ] Verify data persistence across application restarts
- [ ] Test import/export functionality

#### 9.2 Cross-Platform Testing
- [ ] **Windows 10/11**: Test on clean machine
- [ ] **Ubuntu 22.04+**: Test on Linux VM
- [ ] **macOS 13+**: Test on Mac (Intel and Apple Silicon if possible)

#### 9.3 Performance Testing
- [ ] Compare query performance with SQL Server
- [ ] Test with large datasets (10k+ records)
- [ ] Monitor database file size growth

#### 9.4 Migration Testing (for existing users)
- [ ] Create data export script from SQL Server
- [ ] Create data import script to SQLite
- [ ] Test migration with production-like data

---

## Data Migration Script (For Existing Users)

```csharp
// DataAccessLibrary/Services/DataMigrationService.cs
public class DataMigrationService
{
    public async Task MigrateFromSqlServerToSqlite(
        string sqlServerConnectionString,
        string sqliteConnectionString)
    {
        // 1. Connect to SQL Server and export data
        // 2. Connect to SQLite and import data
        // 3. Verify data integrity
    }
}
```

---

## Rollback Plan
If migration fails or issues are discovered:
1. Keep SQL Server branch for fallback
2. Tag current commit before migration: `git tag pre-sqlite-migration`
3. Document any breaking changes
4. Maintain SQL Server migrations in separate branch if needed

---

## Post-Migration Tasks
- [ ] Update README.md with new database requirements
- [ ] Update installation documentation
- [ ] Create user migration guide (SQL Server → SQLite)
- [ ] Update CI/CD pipelines for multi-platform builds
- [ ] Create release packages for all platforms
- [ ] Update developer documentation

---

## Known Limitations of SQLite vs SQL Server

### Features Not Available in SQLite:
- ❌ Stored procedures (must be converted to C# methods)
- ❌ User-defined functions (must be implemented in C#)
- ❌ Full-text search (limited - use FTS5 extension or implement in code)
- ❌ Multiple simultaneous writers (SQLite uses file-level locking)
- ❌ Built-in data compression

### Workarounds:
- **Stored Procedures**: Convert to repository methods in C#
- **Concurrent Writes**: Use write-ahead logging (WAL) mode
- **Full-Text Search**: Use SQLite FTS5 or implement in-memory search

---

## Timeline Estimate

| Phase | Estimated Time | Priority |
|-------|---------------|----------|
| Phase 1: Add SQLite | 30 min | High |
| Phase 2: Update DbContext | 30 min | High |
| Phase 3: Connection Strings | 1 hour | High |
| Phase 4: T-SQL Conversion | 4-8 hours | High |
| Phase 5: Recreate Migrations | 2 hours | High |
| Phase 6: Auto-Migration | 1 hour | Medium |
| Phase 7: Cross-Platform Config | 2 hours | Medium |
| Phase 8: Publish Profiles | 1 hour | Medium |
| Phase 9: Testing | 4-6 hours | High |

**Total Estimated Time:** 15-22 hours

---

## Success Criteria
- ✅ Application runs on Windows without SQL Server installed
- ✅ Application runs on Ubuntu Linux
- ✅ Application runs on macOS
- ✅ Database auto-creates on first launch
- ✅ All existing features work correctly
- ✅ Performance is acceptable (within 20% of SQL Server)
- ✅ Data can be migrated from existing SQL Server instances

---

## Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|-----------|
| T-SQL conversion errors | High | Thorough testing, gradual rollout |
| Performance degradation | Medium | Benchmark before/after, optimize queries |
| Data loss during migration | High | Comprehensive backup strategy, test migration |
| Cross-platform bugs | Medium | Test on all target platforms |
| Concurrent write issues | Low | Use WAL mode, implement retry logic |

---

## Next Steps
1. Review and approve this plan
2. Create feature branch: `feature/sqlite-migration`
3. Begin Phase 1 implementation
4. Commit after each phase completion
5. Test incrementally throughout migration

---

## Resources
- [EF Core SQLite Provider Docs](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/)
- [SQLite Documentation](https://www.sqlite.org/docs.html)
- [SQL Server to SQLite Migration Guide](https://www.sqlite.org/cvstrac/wiki?p=SqlServer_vs_SQLite)
- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
