using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DataAccessLibrary.Models;
using DataAccessLibrary.Configuration;

// Simple argument parsing
string GetArg(string name)
{
    for (int i = 0; i < args.Length; i++)
    {
        if (args[i] == name && i + 1 < args.Length) return args[i + 1];
        if (args[i].StartsWith(name + "=", StringComparison.OrdinalIgnoreCase)) return args[i].Substring(name.Length + 1);
    }
    return string.Empty;
}

string sourceConnection = GetArg("--source-connection");
string targetDbArg = GetArg("--target-db");
bool dryRun = Array.Exists(args, a => a == "--dry-run");
bool verifyOnly = Array.Exists(args, a => a == "--verify-only");
bool verify = Array.Exists(args, a => a == "--verify");
string tablesArg = GetArg("--tables");
int batchSize = 1000;
var batchSizeArg = GetArg("--batch-size");
if (!string.IsNullOrWhiteSpace(batchSizeArg) && int.TryParse(batchSizeArg, out var _bs)) batchSize = _bs; 


HashSet<string>? tablesSet = null;
if (!string.IsNullOrWhiteSpace(tablesArg))
{
    tablesSet = new HashSet<string>(tablesArg.Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.Trim().ToLowerInvariant()));
}

var loggerFactory = LoggerFactory.Create(lb => lb.AddConsole());
var logger = loggerFactory.CreateLogger("Migrator");

// Parse sensitive-related flags now that logger exists (safe to emit messages)
bool includeSensitive = Array.Exists(args, a => a == "--include-sensitive");
if (includeSensitive) logger.LogWarning("Including sensitive rows in migration; migrated sensitive rows will be tracked in the target DB table 'MigratedSensitiveRows'.");

bool generateDeleteScript = Array.Exists(args, a => a == "--generate-delete-script");
bool purgeSensitive = Array.Exists(args, a => a == "--purge-sensitive");

// Log selected tables and flags if provided
if (tablesSet != null)
{
    logger.LogInformation("Selected tables to migrate/verify: {t}", string.Join(",", tablesSet));
}
if (verifyOnly) logger.LogInformation("Running verification only (no migration will be performed)");
if (verify) logger.LogInformation("Verification will run after migration");

if (string.IsNullOrWhiteSpace(sourceConnection))
{
    logger.LogError("--source-connection is required");
    return 1;
}

var targetConn = string.IsNullOrWhiteSpace(targetDbArg) ? DatabaseConfiguration.GetConnectionString() : targetDbArg;

logger.LogInformation("Source: {src}", sourceConnection);
logger.LogInformation("Target: {tgt}", targetConn);
logger.LogInformation("DryRun: {dry}", dryRun);

// Build source context (SQL Server)
var sourceOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer(sourceConnection)
    .Options;

var sourceConfig = new ConfigurationBuilder().AddInMemoryCollection().Build();
using var sourceDb = new ApplicationDbContext(sourceOptions, sourceConfig);

// Build target context (SQLite)
var targetOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlite(targetConn)
    .Options;

var targetConfig = new ConfigurationBuilder().AddInMemoryCollection().Build();
using var targetDb = new ApplicationDbContext(targetOptions, targetConfig);

logger.LogInformation("Counting source rows for TalonVoiceCommands and TalonLists...");

// Gather sensitive category IDs and determine allowed categories based on flags
var sensitiveCategoryIds = await sourceDb.Categories.Where(c => c.Sensitive).Select(c => c.Id).ToListAsync();
var allowedCategoryIds = includeSensitive
    ? await sourceDb.Categories.Select(c => c.Id).ToListAsync()
    : await sourceDb.Categories.Where(c => !c.Sensitive).Select(c => c.Id).ToListAsync();
logger.LogInformation(includeSensitive ? "Including sensitive categories: total categories {c}" : "Non-sensitive categories found: {c}", allowedCategoryIds.Count);

int commandsCount = 0, listsCount = 0;
int computersCount = 0, languagesCount = 0, categoriesCount = allowedCategoryIds.Count, launchersCount = 0;
int applicationsCount = 0, intellisenseCount = 0, additionalCommandsCount = 0;
int valuesToInsertCount = 0, promptsCount = 0, quickPromptsCount = 0, vsCommandsCount = 0, transactionsCount = 0;

if (tablesSet == null || tablesSet.Contains("commands") || tablesSet.Contains("talonvoicecommands"))
    commandsCount = await sourceDb.TalonVoiceCommands.CountAsync();
if (tablesSet == null || tablesSet.Contains("lists") || tablesSet.Contains("talonlists"))
    listsCount = await sourceDb.TalonLists.CountAsync();
if (tablesSet == null || tablesSet.Contains("computers"))
    computersCount = await sourceDb.Computers.CountAsync();
if (tablesSet == null || tablesSet.Contains("languages"))
    languagesCount = await sourceDb.Languages.CountAsync();
if (tablesSet == null || tablesSet.Contains("categories"))
{
    var rawCategoriesCount = await sourceDb.Categories.CountAsync();
    categoriesCount = allowedCategoryIds.Count;
    logger.LogInformation("Categories - raw:{raw}, non-sensitive:{count}, excluded:{excluded}", rawCategoriesCount, categoriesCount, rawCategoriesCount - categoriesCount);
}
if (tablesSet == null || tablesSet.Contains("launchers"))
    launchersCount = await sourceDb.Launcher.CountAsync(l => allowedCategoryIds.Contains(l.CategoryId));
if (tablesSet == null || tablesSet.Contains("applications") || tablesSet.Contains("applicationdetails"))
    applicationsCount = await sourceDb.ApplicationDetails.CountAsync();
if (tablesSet == null || tablesSet.Contains("intellisense") || tablesSet.Contains("customintellisenses"))
    intellisenseCount = await sourceDb.CustomIntelliSenses.CountAsync(i => allowedCategoryIds.Contains(i.CategoryId));
if (tablesSet == null || tablesSet.Contains("additionalcommands"))
    additionalCommandsCount = await sourceDb.AdditionalCommands.CountAsync();
if (tablesSet == null || tablesSet.Contains("values") || tablesSet.Contains("valuestoinsert"))
    valuesToInsertCount = await sourceDb.ValuesToInserts.CountAsync();
if (tablesSet == null || tablesSet.Contains("prompts"))
    promptsCount = await sourceDb.Prompts.CountAsync();
if (tablesSet == null || tablesSet.Contains("quickprompts"))
    quickPromptsCount = await sourceDb.QuickPrompts.CountAsync();
if (tablesSet == null || tablesSet.Contains("vscommands") || tablesSet.Contains("visualstudiocommands"))
    vsCommandsCount = await sourceDb.VisualStudioCommands.CountAsync();
if (tablesSet == null || tablesSet.Contains("transactions"))
    transactionsCount = await sourceDb.Transactions.CountAsync();

logger.LogInformation("Source counts: commands={c}, lists={l}, computers={cmp}, languages={lang}, categories={cat}, launchers={lnc}, apps={app}, intellisense={int}, addl={addl}, values={vals}, prompts={p}, quickPrompts={qp}, vsCommands={vs}, transactions={tx}",
    commandsCount, listsCount, computersCount, languagesCount, categoriesCount, launchersCount, applicationsCount, intellisenseCount, additionalCommandsCount, valuesToInsertCount, promptsCount, quickPromptsCount, vsCommandsCount, transactionsCount);

// Special dry-run for remaining unmigrated tables (only when --dry-run is provided)
if (tablesSet != null && tablesSet.Contains("remaining") && dryRun)
{
    logger.LogInformation("Counting remaining (unmigrated) entity tables in source DB...");

    // Known tables already handled by the migrator (use lowercase entity names)
    var knownHandled = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "talonvoicecommand","talonlist","computer","language","category","launcher","applicationdetail",
        "customintellisense","additionalcommand","valuestoinsert","prompt","quickprompt","visualstudiocommand","transaction"
    };

    var entityTypes = sourceDb.Model.GetEntityTypes();
    foreach (var et in entityTypes.OrderBy(e => e.ClrType.Name))
    {
        var typeName = et.ClrType.Name;
        if (knownHandled.Contains(typeName)) continue;

        try
        {
            var tableName = et.GetTableName();
            if (string.IsNullOrWhiteSpace(tableName))
            {
                logger.LogInformation("Skipping {t} (no table name)", typeName);
                continue;
            }

            var conn = sourceDb.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(1) FROM [{tableName}]";
            var scalar = await cmd.ExecuteScalarAsync();
            long count = 0;
            if (scalar != null && long.TryParse(scalar.ToString(), out var parsed)) count = parsed;
            logger.LogInformation("{t} - {c}", typeName, count);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to count {t} (may be a view or unsupported type)", typeName);
        }
    }

    logger.LogInformation("Remaining counts complete (dry run)");
    return 0;
}

if (dryRun)
{
    logger.LogInformation("Dry run complete");
    return 0;
}

// If verify-only was requested, run verification checks now and exit
if (verifyOnly)
{
    logger.LogInformation("Starting verification-only checks...");

    if (tablesSet == null || tablesSet.Contains("commands") || tablesSet.Contains("talonvoicecommands"))
    {
        var srcIds = await sourceDb.TalonVoiceCommands.Select(t => t.Id).ToListAsync();
        var tgtIds = await targetDb.TalonVoiceCommands.Select(t => t.Id).ToListAsync();
        var missing = srcIds.Except(tgtIds).Take(5).ToList();
        logger.LogInformation("TalonVoiceCommands - source:{s}, target:{t}, missingInTargetSample:{m}", srcIds.Count, tgtIds.Count, string.Join(',', missing));
    }

    if (tablesSet == null || tablesSet.Contains("lists") || tablesSet.Contains("talonlists"))
    {
        var srcIds = await sourceDb.TalonLists.Select(t => t.Id).ToListAsync();
        var tgtIds = await targetDb.TalonLists.Select(t => t.Id).ToListAsync();
        var missing = srcIds.Except(tgtIds).Take(5).ToList();
        logger.LogInformation("TalonLists - source:{s}, target:{t}, missingInTargetSample:{m}", srcIds.Count, tgtIds.Count, string.Join(',', missing));
    }

    if (tablesSet == null || tablesSet.Contains("computers"))
    {
        var srcIds = await sourceDb.Computers.Select(t => t.Id).ToListAsync();
        var tgtIds = await targetDb.Computers.Select(t => t.Id).ToListAsync();
        var missing = srcIds.Except(tgtIds).Take(5).ToList();
        logger.LogInformation("Computers - source:{s}, target:{t}, missingInTargetSample:{m}", srcIds.Count, tgtIds.Count, string.Join(',', missing));
    }

    if (tablesSet == null || tablesSet.Contains("categories"))
    {
        var srcIds = await sourceDb.Categories.Where(c => !c.Sensitive).Select(t => t.Id).ToListAsync();
        var tgtIds = await targetDb.Categories.Select(t => t.Id).ToListAsync();
        var missing = srcIds.Except(tgtIds).Take(5).ToList();
        logger.LogInformation("Categories - source (non-sensitive):{s}, target:{t}, missingInTargetSample:{m}", srcIds.Count, tgtIds.Count, string.Join(',', missing));
    }

    if (tablesSet == null || tablesSet.Contains("languages"))
    {
        var srcIds = await sourceDb.Languages.Select(t => t.Id).ToListAsync();
        var tgtIds = await targetDb.Languages.Select(t => t.Id).ToListAsync();
        var missing = srcIds.Except(tgtIds).Take(5).ToList();
        logger.LogInformation("Languages - source:{s}, target:{t}, missingInTargetSample:{m}", srcIds.Count, tgtIds.Count, string.Join(',', missing));
    }

    if (tablesSet == null || tablesSet.Contains("launchers"))
    {
        var srcIds = await sourceDb.Launcher.Where(l => allowedCategoryIds.Contains(l.CategoryId)).Select(t => t.Id).ToListAsync();
        var tgtIds = await targetDb.Launcher.Select(t => t.Id).ToListAsync();
        var missing = srcIds.Except(tgtIds).Take(5).ToList();
        logger.LogInformation("Launchers - source (non-sensitive categories):{s}, target:{t}, missingInTargetSample:{m}", srcIds.Count, tgtIds.Count, string.Join(',', missing));
    }

    if (tablesSet == null || tablesSet.Contains("applications") || tablesSet.Contains("applicationdetails"))
    {
        var srcIds = await sourceDb.ApplicationDetails.Select(t => t.Id).ToListAsync();
        var tgtIds = await targetDb.ApplicationDetails.Select(t => t.Id).ToListAsync();
        var missing = srcIds.Except(tgtIds).Take(5).ToList();
        logger.LogInformation("ApplicationDetails - source:{s}, target:{t}, missingInTargetSample:{m}", srcIds.Count, tgtIds.Count, string.Join(',', missing));
    }

    if (tablesSet == null || tablesSet.Contains("intellisense") || tablesSet.Contains("customintellisenses"))
    {
        var srcIds = await sourceDb.CustomIntelliSenses.Where(i => allowedCategoryIds.Contains(i.CategoryId)).Select(t => t.Id).ToListAsync();
        var tgtIds = await targetDb.CustomIntelliSenses.Select(t => t.Id).ToListAsync();
        var missing = srcIds.Except(tgtIds).Take(5).ToList();
        logger.LogInformation("CustomIntelliSenses - source (non-sensitive categories):{s}, target:{t}, missingInTargetSample:{m}", srcIds.Count, tgtIds.Count, string.Join(',', missing));
    }

    if (tablesSet == null || tablesSet.Contains("additionalcommands"))
    {
        var srcIds = await sourceDb.AdditionalCommands.Select(t => t.Id).ToListAsync();
        var tgtIds = await targetDb.AdditionalCommands.Select(t => t.Id).ToListAsync();
        var missing = srcIds.Except(tgtIds).Take(5).ToList();
        logger.LogInformation("AdditionalCommands - source:{s}, target:{t}, missingInTargetSample:{m}", srcIds.Count, tgtIds.Count, string.Join(',', missing));
    }

    if (tablesSet == null || tablesSet.Contains("values") || tablesSet.Contains("valuestoinsert"))
    {
        var srcIds = await sourceDb.ValuesToInserts.Select(t => t.Id).ToListAsync();
        var tgtIds = await targetDb.ValuesToInserts.Select(t => t.Id).ToListAsync();
        var missing = srcIds.Except(tgtIds).Take(5).ToList();
        logger.LogInformation("ValuesToInserts - source:{s}, target:{t}, missingInTargetSample:{m}", srcIds.Count, tgtIds.Count, string.Join(',', missing));
    }

    if (tablesSet == null || tablesSet.Contains("prompts"))
    {
        var srcIds = await sourceDb.Prompts.Select(t => t.Id).ToListAsync();
        var tgtIds = await targetDb.Prompts.Select(t => t.Id).ToListAsync();
        var missing = srcIds.Except(tgtIds).Take(5).ToList();
        logger.LogInformation("Prompts - source:{s}, target:{t}, missingInTargetSample:{m}", srcIds.Count, tgtIds.Count, string.Join(',', missing));
    }

    if (tablesSet == null || tablesSet.Contains("quickprompts"))
    {
        var srcIds = await sourceDb.QuickPrompts.Select(t => t.Id).ToListAsync();
        var tgtIds = await targetDb.QuickPrompts.Select(t => t.Id).ToListAsync();
        var missing = srcIds.Except(tgtIds).Take(5).ToList();
        logger.LogInformation("QuickPrompts - source:{s}, target:{t}, missingInTargetSample:{m}", srcIds.Count, tgtIds.Count, string.Join(',', missing));
    }

    if (tablesSet == null || tablesSet.Contains("vscommands") || tablesSet.Contains("visualstudiocommands"))
    {
        var srcIds = await sourceDb.VisualStudioCommands.Select(t => t.Id).ToListAsync();
        var tgtIds = await targetDb.VisualStudioCommands.Select(t => t.Id).ToListAsync();
        var missing = srcIds.Except(tgtIds).Take(5).ToList();
        logger.LogInformation("VisualStudioCommands - source:{s}, target:{t}, missingInTargetSample:{m}", srcIds.Count, tgtIds.Count, string.Join(',', missing));
    }

    if (tablesSet == null || tablesSet.Contains("transactions"))
    {
        var srcIds = await sourceDb.Transactions.Select(t => t.Id).ToListAsync();
        var tgtIds = await targetDb.Transactions.Select(t => t.Id).ToListAsync();
        var missing = srcIds.Except(tgtIds).Take(5).ToList();
        logger.LogInformation("Transactions - source:{s}, target:{t}, missingInTargetSample:{m}", srcIds.Count, tgtIds.Count, string.Join(',', missing));
    }

    logger.LogInformation("Verification-only checks complete");
    return 0;
}

// Backup target DB file if present
try
{
    var targetPath = DatabaseConfiguration.GetDatabasePath();
    if (System.IO.File.Exists(targetPath))
    {
        var bak = targetPath + ".bak." + DateTime.Now.ToString("yyyyMMddHHmmss");
        System.IO.File.Copy(targetPath, bak);
        logger.LogInformation("Backed up {t} to {b}", targetPath, bak);
    }
}
catch (Exception ex)
{
    logger.LogWarning(ex, "Failed to backup target DB");
}

// If including sensitive rows, ensure a tracking table exists in the target DB so we can generate delete scripts later
if (includeSensitive)
{
    logger.LogInformation("Creating tracking table 'MigratedSensitiveRows' in target DB");
    await targetDb.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS MigratedSensitiveRows (Id INTEGER PRIMARY KEY AUTOINCREMENT, TableName TEXT NOT NULL, SourceId TEXT NOT NULL, MigratedAt TEXT NOT NULL)");
}


// Copy tables (simple approach): read all commands and lists from source and insert to target.
// Read each table only if no --tables filter or if the table is selected
List<DataAccessLibrary.Models.TalonVoiceCommand> commands = new();
List<DataAccessLibrary.Models.TalonList> lists = new();
List<DataAccessLibrary.Models.Computer> computers = new();
List<DataAccessLibrary.Models.Language> languages = new();
List<DataAccessLibrary.Models.Category> categories = new();
List<DataAccessLibrary.Models.Launcher> launchers = new();
List<DataAccessLibrary.Models.ApplicationDetail> applications = new();
List<DataAccessLibrary.Models.CustomIntelliSense> intellisenses = new();
List<DataAccessLibrary.Models.AdditionalCommand> additionalCommands = new();
List<DataAccessLibrary.Models.ValuesToInsert> valuesToInsert = new();
List<DataAccessLibrary.Models.Prompt> prompts = new();
List<DataAccessLibrary.Models.QuickPrompt> quickPrompts = new();
List<DataAccessLibrary.Models.VisualStudioCommand> vsCommands = new();
List<DataAccessLibrary.Models.Transaction> transactions = new();

if (tablesSet == null || tablesSet.Contains("commands") || tablesSet.Contains("talonvoicecommands"))
{
    logger.LogInformation("Commands will be streamed from source in batches of {b}", batchSize);
} 

if (tablesSet == null || tablesSet.Contains("lists") || tablesSet.Contains("talonlists"))
{
    logger.LogInformation("Lists will be streamed from source in batches of {b}", batchSize);
} 

if (tablesSet == null || tablesSet.Contains("computers"))
{
    logger.LogInformation("Computers will be streamed from source in batches of {b}", batchSize);
} 

if (tablesSet == null || tablesSet.Contains("languages"))
{
    logger.LogInformation("Languages will be streamed from source in batches of {b}", batchSize);
} 

if (tablesSet == null || tablesSet.Contains("categories"))
{
    logger.LogInformation(includeSensitive ? "Categories (including sensitive) will be streamed from source in batches of {b}" : "Categories (non-sensitive) will be streamed from source in batches of {b}", batchSize);
} 

if (tablesSet == null || tablesSet.Contains("launchers"))
{
    logger.LogInformation(includeSensitive ? "Launchers (including sensitive categories) will be streamed from source in batches of {b}" : "Launchers (non-sensitive categories) will be streamed from source in batches of {b}", batchSize);
} 

if (tablesSet == null || tablesSet.Contains("applications") || tablesSet.Contains("applicationdetails"))
{
    logger.LogInformation("ApplicationDetails will be streamed from source in batches of {b}", batchSize);
} 

if (tablesSet == null || tablesSet.Contains("intellisense") || tablesSet.Contains("customintellisenses"))
{
    logger.LogInformation(includeSensitive ? "CustomIntelliSenses (including sensitive categories) will be streamed from source in batches of {b}" : "CustomIntelliSenses (non-sensitive categories) will be streamed from source in batches of {b}", batchSize);
} 

if (tablesSet == null || tablesSet.Contains("additionalcommands"))
{
    logger.LogInformation(includeSensitive ? "AdditionalCommands (including sensitive intellisense) will be streamed from source in batches of {b}" : "AdditionalCommands (for non-sensitive intellisense) will be streamed from source in batches of {b}", batchSize);
} 

if (tablesSet == null || tablesSet.Contains("values") || tablesSet.Contains("valuestoinsert"))
{
    logger.LogInformation("ValuesToInserts will be streamed from source in batches of {b}", batchSize);
} 

if (tablesSet == null || tablesSet.Contains("prompts"))
{
    logger.LogInformation("Prompts will be streamed from source in batches of {b}", batchSize);
} 

if (tablesSet == null || tablesSet.Contains("quickprompts"))
{
    logger.LogInformation("QuickPrompts will be streamed from source in batches of {b}", batchSize);
} 

if (tablesSet == null || tablesSet.Contains("vscommands") || tablesSet.Contains("visualstudiocommands"))
{
    logger.LogInformation("VisualStudioCommands will be streamed from source in batches of {b}", batchSize);
} 

if (tablesSet == null || tablesSet.Contains("transactions"))
{
    logger.LogInformation("Transactions will be streamed from source in batches of {b}", batchSize);
} 

// Enumerate remaining unmigrated entity types for streaming migration
List<Type> remainingTypes = new();
if (tablesSet != null && tablesSet.Contains("remaining"))
{
    logger.LogInformation("Enumerating remaining entity types for streaming migration...");

    var knownHandled = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "TalonVoiceCommand","TalonList","Computer","Language","Category","Launcher","ApplicationDetail",
        "CustomIntelliSense","AdditionalCommand","ValuesToInsert","Prompt","QuickPrompt","VisualStudioCommand","Transaction"
    };

    var entityTypes = sourceDb.Model.GetEntityTypes().OrderBy(e => e.ClrType.Name);
    foreach (var et in entityTypes)
    {
        var typeName = et.ClrType.Name;
        if (knownHandled.Contains(typeName)) continue;

        var tableName = et.GetTableName();
        if (string.IsNullOrWhiteSpace(tableName))
        {
            logger.LogInformation("Skipping {t} (no table name)", typeName);
            continue;
        }

        remainingTypes.Add(et.ClrType);
        logger.LogInformation("Scheduled remaining entity type for streaming: {t}", typeName);
    }

    logger.LogInformation("Finished enumerating remaining entity types");
}

// Helper: sanitize tracked WindowsSpeechVoiceCommand entities (fix null SpokenCommand) and stream helper
void SanitizeWindowsSpeechVoiceCommands()
{
    var entries = targetDb.ChangeTracker.Entries<WindowsSpeechVoiceCommand>()
        .Where(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added || e.State == Microsoft.EntityFrameworkCore.EntityState.Modified)
        .ToList();
    foreach (var ent in entries)
    {
        if (string.IsNullOrEmpty(ent.Entity.SpokenCommand)) ent.Entity.SpokenCommand = string.Empty;
    }
}

// Helper: stream using raw SQL reader when EF streaming fails (avoids MARS/open-reader issues)
async Task<bool> RawSqlStreamInsert(Type clrType, string tableName, int batchSize, List<int> sensitiveCatIds, bool includeSensitive)
{
    logger.LogInformation("Attempting raw-SQL streaming for {t}", clrType.Name);
    // Helper to insert a batch of rows into SQLite using parameterized statements
    async Task InsertBatchToSqlite(string tbl, string[] cols, List<object?[]> rows)
    {
        using var conn2 = new Microsoft.Data.Sqlite.SqliteConnection(targetConn);
        await conn2.OpenAsync();
        using var tx = conn2.BeginTransaction();

        var colList = string.Join(",", cols.Select(c => '"' + c + '"'));
        var paramList = string.Join(",", Enumerable.Range(0, cols.Length).Select(i => "$p" + i));

        var cmd = conn2.CreateCommand();
        // Use INSERT OR IGNORE to avoid UNIQUE constraint failures on repeated runs
        cmd.CommandText = $"INSERT OR IGNORE INTO [{tbl}] ({colList}) VALUES ({paramList})";
        for (int i = 0; i < cols.Length; i++) cmd.Parameters.Add(new Microsoft.Data.Sqlite.SqliteParameter("$p" + i, null));

        foreach (var r in rows)
        {
            for (int i = 0; i < cols.Length; i++) cmd.Parameters[i].Value = r[i] ?? (object)DBNull.Value;
            await cmd.ExecuteNonQueryAsync();
        }

        tx.Commit();
    }

    try
    {
        using var conn = new Microsoft.Data.SqlClient.SqlConnection(sourceConnection);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT * FROM [{tableName}]";
        using var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SequentialAccess);

        // Build list of source columns
        var sourceColumns = Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)).ToArray();

        // Determine target table columns so we only insert columns that exist in the SQLite target
        var targetColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var tgtConn = new Microsoft.Data.Sqlite.SqliteConnection(targetConn);
            await tgtConn.OpenAsync();
            using var pragmaCmd = tgtConn.CreateCommand();
            pragmaCmd.CommandText = $"PRAGMA table_info('{tableName}')";
            using var pragmaReader = await pragmaCmd.ExecuteReaderAsync();
            while (await pragmaReader.ReadAsync())
            {
                var col = pragmaReader.GetString(1);
                targetColumns.Add(col);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get target table schema for {t}; will attempt to insert all columns", clrType.Name);
            // fall through - we'll try to insert using whatever columns we have
            targetColumns = new HashSet<string>(sourceColumns, StringComparer.OrdinalIgnoreCase);
        }

        var batchRows = new List<object?[]>();
        var usedCols = sourceColumns.Where(c => targetColumns.Contains(c)).ToArray();
        var sensitiveToRecord = new List<string>();

        while (await reader.ReadAsync())
        {
            var row = new object?[usedCols.Length];
            for (int j = 0; j < usedCols.Length; j++)
            {
                var colName = usedCols[j];
                var idx = Array.IndexOf(sourceColumns, colName);
                if (await reader.IsDBNullAsync(idx)) row[j] = null; else row[j] = reader.GetValue(idx);
            }

            // Collect sensitive id if needed (by CategoryId column)
            if (includeSensitive && targetColumns.Contains("CategoryId"))
            {
                var idxCat = Array.IndexOf(usedCols, "CategoryId");
                if (idxCat >= 0 && row[idxCat] != null && sensitiveCatIds.Contains(Convert.ToInt32(row[idxCat])))
                {
                    var idxId = Array.IndexOf(usedCols, "Id");
                    var sid = idxId >= 0 && row[idxId] != null ? row[idxId].ToString() : "[unknown]";
                    sensitiveToRecord.Add(sid ?? "[unknown]");
                }
            }

            // Ensure SpokenCommand isn't null for WindowsSpeechVoiceCommand
            if (string.Equals(clrType.Name, "WindowsSpeechVoiceCommand", StringComparison.OrdinalIgnoreCase))
            {
                var idxSpoken = Array.IndexOf(usedCols, "SpokenCommand");
                if (idxSpoken >= 0 && row[idxSpoken] == null) row[idxSpoken] = string.Empty;
            }

            batchRows.Add(row);

            if (batchRows.Count >= batchSize)
            {
                await InsertBatchToSqlite(tableName, usedCols, batchRows);
                if (sensitiveToRecord.Count > 0)
                {
                    using var conn2 = new Microsoft.Data.Sqlite.SqliteConnection(targetConn);
                    await conn2.OpenAsync();
                    using var tx = conn2.BeginTransaction();
                    foreach (var sid in sensitiveToRecord)
                    {
                        using var ins = conn2.CreateCommand();
                        ins.CommandText = "INSERT INTO MigratedSensitiveRows (TableName, SourceId, MigratedAt) VALUES ($t,$s,$m)";
                        ins.Parameters.AddWithValue("$t", clrType.Name);
                        ins.Parameters.AddWithValue("$s", sid);
                        ins.Parameters.AddWithValue("$m", DateTime.UtcNow.ToString("o"));
                        await ins.ExecuteNonQueryAsync();
                    }
                    tx.Commit();
                }

                batchRows.Clear();
                sensitiveToRecord.Clear();
                logger.LogInformation("Inserted {n} rows into {t} via raw SQL (batch)", batchSize, clrType.Name);
            }
        }

        if (batchRows.Count > 0)
        {
            await InsertBatchToSqlite(tableName, usedCols, batchRows);
            if (sensitiveToRecord.Count > 0)
            {
                using var conn2 = new Microsoft.Data.Sqlite.SqliteConnection(targetConn);
                await conn2.OpenAsync();
                using var tx = conn2.BeginTransaction();
                foreach (var sid in sensitiveToRecord)
                {
                    using var ins = conn2.CreateCommand();
                    ins.CommandText = "INSERT INTO MigratedSensitiveRows (TableName, SourceId, MigratedAt) VALUES ($t,$s,$m)";
                    ins.Parameters.AddWithValue("$t", clrType.Name);
                    ins.Parameters.AddWithValue("$s", sid);
                    ins.Parameters.AddWithValue("$m", DateTime.UtcNow.ToString("o"));
                    await ins.ExecuteNonQueryAsync();
                }
                tx.Commit();
            }
            logger.LogInformation("Inserted {n} rows into {t} via raw SQL (final)", batchRows.Count, clrType.Name);
            batchRows.Clear();
        }

        return true;
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Raw-SQL streaming failed for {t}", clrType.Name);
        return false;
    }
}

// Helper: stream from source to target in batches to avoid loading entire tables into memory
async Task StreamAndInsertAsync<TEntity>(IQueryable<TEntity> sourceQuery, DbSet<TEntity> targetSet, string name, bool trackSensitive = false, List<int>? sensitiveCatIds = null) where TEntity : class
{
    int inserted = 0;
    var batch = new List<TEntity>(batchSize);
    var sw = System.Diagnostics.Stopwatch.StartNew();

    // Buffer of sensitive rows to record (source Ids as strings)
    var sensitiveToRecord = new List<string>();

    var idProp = typeof(TEntity).GetProperty("Id") ?? typeof(TEntity).GetProperty(typeof(TEntity).Name + "Id");
    var catProp = typeof(TEntity).GetProperty("CategoryId");

    await foreach (var item in sourceQuery.AsNoTracking().AsAsyncEnumerable())
    {
        // Detect sensitivity by CategoryId (if present) and record the source id
        if (trackSensitive && catProp != null && sensitiveCatIds != null)
        {
            var catVal = catProp.GetValue(item);
            if (catVal != null && sensitiveCatIds.Contains(Convert.ToInt32(catVal)))
            {
                var srcId = idProp?.GetValue(item)?.ToString() ?? "[unknown]";
                sensitiveToRecord.Add(srcId);
            }
        }

        batch.Add(item);
        if (batch.Count >= batchSize)
        {
            await targetSet.AddRangeAsync(batch);
            SanitizeWindowsSpeechVoiceCommands();
            await targetDb.SaveChangesAsync();
            targetDb.ChangeTracker.Clear();

            // Persist any sensitive row records for this batch
            if (sensitiveToRecord.Count > 0)
            {
                foreach (var sid in sensitiveToRecord)
                {
                    await targetDb.Database.ExecuteSqlRawAsync("INSERT INTO MigratedSensitiveRows (TableName, SourceId, MigratedAt) VALUES ({0}, {1}, {2})", name, sid, DateTime.UtcNow.ToString("o"));
                }
                sensitiveToRecord.Clear();
            }

            inserted += batch.Count;
            logger.LogInformation("Inserted {n} {t} rows (total:{tot})", batch.Count, name, inserted);
            batch.Clear();
        }
    }
    if (batch.Count > 0)
    {
        await targetSet.AddRangeAsync(batch);
        SanitizeWindowsSpeechVoiceCommands();
        await targetDb.SaveChangesAsync();
        targetDb.ChangeTracker.Clear();

        if (sensitiveToRecord.Count > 0)
        {
            foreach (var sid in sensitiveToRecord)
            {
                await targetDb.Database.ExecuteSqlRawAsync("INSERT INTO MigratedSensitiveRows (TableName, SourceId, MigratedAt) VALUES ({0}, {1}, {2})", name, sid, DateTime.UtcNow.ToString("o"));
            }
            sensitiveToRecord.Clear();
        }

        inserted += batch.Count;
        logger.LogInformation("Inserted {n} {t} rows (total:{tot})", batch.Count, name, inserted);
    }
    sw.Stop();
    logger.LogInformation("Completed inserting {tot} rows into {t} in {ms}ms", inserted, name, sw.ElapsedMilliseconds);
} 

logger.LogInformation("Inserting into target DB (this may overwrite existing rows with the same keys)...");

// Remove existing rows if present to avoid duplicate primary key conflicts
// Order matters for foreign keys: remove children first
await targetDb.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF;");

// Delete only selected tables (children first)
if (tablesSet == null || tablesSet.Contains("intellisense") || tablesSet.Contains("customintellisenses") || tablesSet.Contains("additionalcommands") || tablesSet.Contains("values") || tablesSet.Contains("prompts") || tablesSet.Contains("quickprompts") || tablesSet.Contains("vscommands"))
{
    targetDb.CustomIntelliSenses.RemoveRange(targetDb.CustomIntelliSenses);
    targetDb.AdditionalCommands.RemoveRange(targetDb.AdditionalCommands);
    targetDb.ValuesToInserts.RemoveRange(targetDb.ValuesToInserts);
    targetDb.Prompts.RemoveRange(targetDb.Prompts);
    targetDb.QuickPrompts.RemoveRange(targetDb.QuickPrompts);
    targetDb.VisualStudioCommands.RemoveRange(targetDb.VisualStudioCommands);
}

if (tablesSet == null || tablesSet.Contains("launchers") || tablesSet.Contains("lists") || tablesSet.Contains("commands"))
{
    targetDb.TalonVoiceCommands.RemoveRange(targetDb.TalonVoiceCommands);
    targetDb.TalonLists.RemoveRange(targetDb.TalonLists);
    targetDb.Launcher.RemoveRange(targetDb.Launcher);
}

if (tablesSet == null || tablesSet.Contains("transactions"))
{
    targetDb.Transactions.RemoveRange(targetDb.Transactions);
}

if (tablesSet == null || tablesSet.Contains("categories") || tablesSet.Contains("languages") || tablesSet.Contains("computers") || tablesSet.Contains("applications"))
{
    targetDb.Categories.RemoveRange(targetDb.Categories);
    targetDb.Languages.RemoveRange(targetDb.Languages);
    targetDb.Computers.RemoveRange(targetDb.Computers);
    targetDb.ApplicationDetails.RemoveRange(targetDb.ApplicationDetails);
}

// Delete remaining tables' rows in target if requested
if (tablesSet != null && tablesSet.Contains("remaining") && remainingTypes != null && remainingTypes.Count > 0)
{
    logger.LogInformation("Deleting existing rows from remaining tables in target DB...");
    foreach (var type in remainingTypes)
    {
        var et = targetDb.Model.FindEntityType(type);
        var tableName = et?.GetTableName();
        if (!string.IsNullOrWhiteSpace(tableName))
        {
            logger.LogInformation("Deleting target rows from {t}", tableName);
            await targetDb.Database.ExecuteSqlRawAsync($"DELETE FROM [{tableName}]");
        }
    }
}

await targetDb.SaveChangesAsync();

// Insert in FK-safe order — streaming inserts in batches when supported
// Precompute allowed custom intellisense ids for additional commands filter
var allowedCustomIntelliSenseIds = await sourceDb.CustomIntelliSenses.Where(i => allowedCategoryIds.Contains(i.CategoryId)).Select(i => i.Id).ToListAsync();

if ((tablesSet == null || tablesSet.Contains("computers")) && computersCount > 0) await StreamAndInsertAsync(sourceDb.Computers.AsNoTracking(), targetDb.Computers, "Computers", includeSensitive, sensitiveCategoryIds);
if ((tablesSet == null || tablesSet.Contains("languages")) && languagesCount > 0) await StreamAndInsertAsync(sourceDb.Languages.AsNoTracking(), targetDb.Languages, "Languages", includeSensitive, sensitiveCategoryIds);
if ((tablesSet == null || tablesSet.Contains("categories")) && categoriesCount > 0) await StreamAndInsertAsync(includeSensitive ? sourceDb.Categories.AsNoTracking() : sourceDb.Categories.AsNoTracking().Where(c => !c.Sensitive), targetDb.Categories, "Categories", includeSensitive, sensitiveCategoryIds);
if ((tablesSet == null || tablesSet.Contains("applications")) && applicationsCount > 0) await StreamAndInsertAsync(sourceDb.ApplicationDetails.AsNoTracking(), targetDb.ApplicationDetails, "ApplicationDetails", includeSensitive, sensitiveCategoryIds);

if ((tablesSet == null || tablesSet.Contains("intellisense")) && intellisenseCount > 0) await StreamAndInsertAsync(sourceDb.CustomIntelliSenses.AsNoTracking().Where(i => allowedCategoryIds.Contains(i.CategoryId)), targetDb.CustomIntelliSenses, "CustomIntelliSenses", includeSensitive, sensitiveCategoryIds);
if ((tablesSet == null || tablesSet.Contains("additionalcommands")) && additionalCommandsCount > 0) await StreamAndInsertAsync(sourceDb.AdditionalCommands.AsNoTracking().Where(ac => allowedCustomIntelliSenseIds.Contains(ac.CustomIntelliSenseId)), targetDb.AdditionalCommands, "AdditionalCommands", includeSensitive, sensitiveCategoryIds);

if ((tablesSet == null || tablesSet.Contains("launchers")) && launchersCount > 0) await StreamAndInsertAsync(sourceDb.Launcher.AsNoTracking().Where(l => allowedCategoryIds.Contains(l.CategoryId)), targetDb.Launcher, "Launchers", includeSensitive, sensitiveCategoryIds);
if ((tablesSet == null || tablesSet.Contains("lists")) && listsCount > 0) await StreamAndInsertAsync(sourceDb.TalonLists.AsNoTracking(), targetDb.TalonLists, "TalonLists", includeSensitive, sensitiveCategoryIds);
if ((tablesSet == null || tablesSet.Contains("commands")) && commandsCount > 0) await StreamAndInsertAsync(sourceDb.TalonVoiceCommands.AsNoTracking(), targetDb.TalonVoiceCommands, "TalonVoiceCommands", includeSensitive, sensitiveCategoryIds);

if ((tablesSet == null || tablesSet.Contains("values")) && valuesToInsertCount > 0) await StreamAndInsertAsync(sourceDb.ValuesToInserts.AsNoTracking(), targetDb.ValuesToInserts, "ValuesToInserts", includeSensitive, sensitiveCategoryIds);
if ((tablesSet == null || tablesSet.Contains("prompts")) && promptsCount > 0) await StreamAndInsertAsync(sourceDb.Prompts.AsNoTracking(), targetDb.Prompts, "Prompts", includeSensitive, sensitiveCategoryIds);
if ((tablesSet == null || tablesSet.Contains("quickprompts")) && quickPromptsCount > 0) await StreamAndInsertAsync(sourceDb.QuickPrompts.AsNoTracking(), targetDb.QuickPrompts, "QuickPrompts", includeSensitive, sensitiveCategoryIds);
if ((tablesSet == null || tablesSet.Contains("vscommands")) && vsCommandsCount > 0) await StreamAndInsertAsync(sourceDb.VisualStudioCommands.AsNoTracking(), targetDb.VisualStudioCommands, "VisualStudioCommands", includeSensitive, sensitiveCategoryIds);
if ((tablesSet == null || tablesSet.Contains("transactions")) && transactionsCount > 0) await StreamAndInsertAsync(sourceDb.Transactions.AsNoTracking(), targetDb.Transactions, "Transactions", includeSensitive, sensitiveCategoryIds);

// Note: remaining entity handling now streams types (no in-memory reads)
await targetDb.SaveChangesAsync();

// Stream remaining entities (if requested) in FK-safe order
if (tablesSet != null && tablesSet.Contains("remaining") && remainingTypes != null && remainingTypes.Count > 0)
{
    logger.LogInformation("Streaming remaining entity tables in FK-safe order...");

    var typesToProcess = remainingTypes.ToList();
    var inserted = new HashSet<Type>();
    int maxPasses = Math.Max(typesToProcess.Count * 2, 10);
    int pass = 0;
    while (typesToProcess.Count > 0 && pass++ < maxPasses)
    {
        bool progressed = false;
        foreach (var type in typesToProcess.ToList())
        {
            var et = targetDb.Model.FindEntityType(type);
            var fks = et.GetForeignKeys();
            bool depends = false;
            foreach (var fk in fks)
            {
                var principal = fk.PrincipalEntityType.ClrType;
                if (typesToProcess.Contains(principal) && !inserted.Contains(principal))
                {
                    depends = true;
                    break;
                }
            }

            if (!depends)
            {
                logger.LogInformation("Streaming and inserting remaining type {t}", type.Name);
                int insertedCount = 0;
                var batch = new List<object>(batchSize);
                var setMethod = typeof(Microsoft.EntityFrameworkCore.DbContext).GetMethod("Set", System.Type.EmptyTypes).MakeGenericMethod(type);
                var queryable = (System.Linq.IQueryable)setMethod.Invoke(sourceDb, null);

                try
                {
                    // Page through source rows using Skip/Take + ToListAsync (reflection) to avoid AsAsyncEnumerable on non-generic IQueryable
                    // Stream using AsAsyncEnumerable to avoid fragile reflection overload selection
                    var sensitiveToRecord = new List<string>();
                    var asAsyncEnumerableMethod = typeof(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions)
                        .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                        .Where(m => m.Name == "AsAsyncEnumerable" && m.GetParameters().Length == 1)
                        .First()
                        .MakeGenericMethod(type);

                    var asyncEnum = asAsyncEnumerableMethod.Invoke(null, new object[] { queryable });
                    var getAsyncEnumerator = asyncEnum.GetType().GetMethod("GetAsyncEnumerator", new Type[] { typeof(System.Threading.CancellationToken) });
                    var enumerator = getAsyncEnumerator.Invoke(asyncEnum, new object[] { System.Threading.CancellationToken.None });
                    var moveNext = enumerator.GetType().GetMethod("MoveNextAsync", Type.EmptyTypes);
                    var currentProp = enumerator.GetType().GetProperty("Current");

                    try
                    {
                        while (true)
                        {
                            // MoveNextAsync returns a ValueTask<bool>; call AsTask() to await reliably
                            var moveTaskObj = moveNext.Invoke(enumerator, null);
                            var asTask = (System.Threading.Tasks.Task<bool>)moveTaskObj.GetType().GetMethod("AsTask").Invoke(moveTaskObj, null);
                            var moved = await asTask.ConfigureAwait(false);
                            if (!moved) break;

                            var item = currentProp.GetValue(enumerator);

                            if (includeSensitive)
                            {
                                var catPropObj = item.GetType().GetProperty("CategoryId");
                                if (catPropObj != null)
                                {
                                    var catValObj = catPropObj.GetValue(item);
                                    if (catValObj != null && sensitiveCategoryIds.Contains(Convert.ToInt32(catValObj)))
                                    {
                                        var idPropObj = item.GetType().GetProperty("Id") ?? item.GetType().GetProperty(item.GetType().Name + "Id");
                                        var sid = idPropObj?.GetValue(item)?.ToString() ?? "[unknown]";
                                        sensitiveToRecord.Add(sid);
                                    }
                                }
                            }

                            if (item is WindowsSpeechVoiceCommand ws && string.IsNullOrEmpty(ws.SpokenCommand)) ws.SpokenCommand = string.Empty;
                            targetDb.Add(item);
                            batch.Add(item);

                            if (batch.Count >= batchSize)
                            {
                                SanitizeWindowsSpeechVoiceCommands();
                                await targetDb.SaveChangesAsync();

                                if (sensitiveToRecord.Count > 0)
                                {
                                    foreach (var sid in sensitiveToRecord)
                                    {
                                        await targetDb.Database.ExecuteSqlRawAsync("INSERT INTO MigratedSensitiveRows (TableName, SourceId, MigratedAt) VALUES ({0}, {1}, {2})", type.Name, sid, DateTime.UtcNow.ToString("o"));
                                    }
                                    sensitiveToRecord.Clear();
                                }

                                targetDb.ChangeTracker.Clear();
                                insertedCount += batch.Count;
                                logger.LogInformation("Inserted {n} rows into {t} (total:{tot})", batch.Count, type.Name, insertedCount);
                                batch.Clear();
                            }
                        }

                        // flush remaining
                        if (batch.Count > 0)
                        {
                            SanitizeWindowsSpeechVoiceCommands();
                            await targetDb.SaveChangesAsync();

                            if (sensitiveToRecord.Count > 0)
                            {
                                foreach (var sid in sensitiveToRecord)
                                {
                                    await targetDb.Database.ExecuteSqlRawAsync("INSERT INTO MigratedSensitiveRows (TableName, SourceId, MigratedAt) VALUES ({0}, {1}, {2})", type.Name, sid, DateTime.UtcNow.ToString("o"));
                                }
                                sensitiveToRecord.Clear();
                            }

                            targetDb.ChangeTracker.Clear();
                            insertedCount += batch.Count;
                            logger.LogInformation("Inserted {n} rows into {t} (total:{tot})", batch.Count, type.Name, insertedCount);
                            batch.Clear();
                        }
                    }
                    finally
                    {
                        // Ensure the async enumerator is disposed (closes underlying DB reader)
                        var disposeAsyncMethod = enumerator.GetType().GetMethod("DisposeAsync", Type.EmptyTypes);
                        if (disposeAsyncMethod != null)
                        {
                            var disposeTaskObj = disposeAsyncMethod.Invoke(enumerator, null);
                            var disposeTaskAsTask = (System.Threading.Tasks.Task)disposeTaskObj.GetType().GetMethod("AsTask").Invoke(disposeTaskObj, null);
                            await disposeTaskAsTask.ConfigureAwait(false);
                        }
                    }

                    inserted.Add(type);
                    typesToProcess.Remove(type);
                }
                catch (Exception ex)
                {
                    // Fallback: try raw SQL streaming first (avoids EF/MARS/open reader issues), then fall back to in-memory ToList
                    logger.LogWarning(ex, "Paging/streaming failed for {t}; attempting raw-SQL fallback", type.Name);

                    var etFallback = sourceDb.Model.FindEntityType(type);
                    var tableNameFallback = etFallback?.GetTableName();
                    if (!string.IsNullOrWhiteSpace(tableNameFallback))
                    {
                        var rawOk = await RawSqlStreamInsert(type, tableNameFallback, batchSize, sensitiveCategoryIds, includeSensitive);
                        if (rawOk)
                        {
                            inserted.Add(type);
                            typesToProcess.Remove(type);
                            progressed = true;
                            continue;
                        }
                        else
                        {
                            logger.LogWarning("Raw-SQL fallback failed for {t}; attempting in-memory fallback", type.Name);
                        }
                    }

                    logger.LogWarning(ex, "Paging failed for {t}; attempting fallback to in-memory read", type.Name);
                    try
                    {
                        var toListAsyncFallback = typeof(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions)
                            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                            .Where(m => m.Name == "ToListAsync" && m.GetParameters().Length == 1)
                            .First()
                            .MakeGenericMethod(type);

                        // Use synchronous ToList fallback to avoid async overload mismatches
                        var toListMethod = typeof(System.Linq.Enumerable).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                            .Where(m => m.Name == "ToList" && m.GetParameters().Length == 1)
                            .First()
                            .MakeGenericMethod(type);

                        var fullListObj = toListMethod.Invoke(null, new object[] { queryable });
                        var fullList = (System.Collections.IList)fullListObj;

                        int written = 0;
                        for (int i = 0; i < fullList.Count; i += batchSize)
                        {
                            var chunk = new List<object>(batchSize);
                            for (int j = i; j < Math.Min(i + batchSize, fullList.Count); j++) chunk.Add(fullList[j]);
                            var sensitiveToRecordChunk = new List<string>();
                            foreach (var item in chunk)
                            {
                                if (includeSensitive)
                                {
                                    var catPropObj = item.GetType().GetProperty("CategoryId");
                                    if (catPropObj != null)
                                    {
                                        var catValObj = catPropObj.GetValue(item);
                                        if (catValObj != null && sensitiveCategoryIds.Contains(Convert.ToInt32(catValObj)))
                                        {
                                            var idPropObj = item.GetType().GetProperty("Id") ?? item.GetType().GetProperty(item.GetType().Name + "Id");
                                            var sid = idPropObj?.GetValue(item)?.ToString() ?? "[unknown]";
                                            sensitiveToRecordChunk.Add(sid);
                                        }
                                    }
                                }
                                if (item is WindowsSpeechVoiceCommand ws && string.IsNullOrEmpty(ws.SpokenCommand)) ws.SpokenCommand = string.Empty;
                                targetDb.Add(item);
                            }
                            SanitizeWindowsSpeechVoiceCommands();
                            await targetDb.SaveChangesAsync();

                            if (sensitiveToRecordChunk.Count > 0)
                            {
                                foreach (var sid in sensitiveToRecordChunk)
                                {
                                    await targetDb.Database.ExecuteSqlRawAsync("INSERT INTO MigratedSensitiveRows (TableName, SourceId, MigratedAt) VALUES ({0}, {1}, {2})", type.Name, sid, DateTime.UtcNow.ToString("o"));
                                }
                            }

                            targetDb.ChangeTracker.Clear();
                            written += chunk.Count;
                            logger.LogInformation("Inserted {n} rows into {t} (total:{tot})", chunk.Count, type.Name, written);
                        }
                        inserted.Add(type);
                        typesToProcess.Remove(type);
                    }
                    catch (Exception ex2)
                    {
                        logger.LogError(ex2, "Fallback read failed for {t}; skipping this type", type.Name);
                        // Remove the type to avoid blocking progress and continue with others
                        typesToProcess.Remove(type);
                        continue;
                    }
                }
                progressed = true;
            }
        }

        if (!progressed)
        {
            logger.LogWarning("Could not resolve dependencies fully; inserting remaining types directly (may break FK constraints): {t}", string.Join(',', typesToProcess.Select(t => t.Name)));
            foreach (var type in typesToProcess.ToList())
            {
                int insertedCount = 0;
                var batch = new List<object>(batchSize);
                var setMethod = typeof(Microsoft.EntityFrameworkCore.DbContext).GetMethod("Set", System.Type.EmptyTypes).MakeGenericMethod(type);
                var queryable = (System.Linq.IQueryable)setMethod.Invoke(sourceDb, null);
                // Page through source rows using Skip/Take + ToListAsync (reflection) in fallback path
                int offset = 0;
                while (true)
                {
                        var skipMethod = typeof(System.Linq.Queryable).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                        .Where(m => m.Name == "Skip" && m.GetParameters().Length == 2)
                        .First()
                        .MakeGenericMethod(type);
                    var takeMethod = typeof(System.Linq.Queryable).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                        .Where(m => m.Name == "Take" && m.GetParameters().Length == 2)
                        .First()
                        .MakeGenericMethod(type);

                    // Use the original queryable directly in the fallback path as well
                    var skipped = skipMethod.Invoke(null, new object[] { queryable, offset });
                    var paged = takeMethod.Invoke(null, new object[] { skipped, batchSize });

                    var toListAsync = typeof(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions)
                        .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                        .Where(m => m.Name == "ToListAsync" && m.GetParameters().Length >= 1)
                        .First()
                        .MakeGenericMethod(type);

                    // Determine correct parameter count for ToListAsync invocation and pass CancellationToken.None when required
                    var toListParams = toListAsync.GetParameters().Length;
                    object[] toListArgs = toListParams == 1 ? new object[] { paged } : new object[] { paged, System.Threading.CancellationToken.None };
                    var task = (System.Threading.Tasks.Task)toListAsync.Invoke(null, toListArgs);
                    await task.ConfigureAwait(false);
                    var resultProperty = task.GetType().GetProperty("Result");
                    var list = (System.Collections.IList)resultProperty.GetValue(task);

                    if (list == null || list.Count == 0) break;

                    var sensitiveToRecordDirect = new List<string>();
                    foreach (var item in list)
                    {
                        if (includeSensitive)
                        {
                            var catPropObj = item.GetType().GetProperty("CategoryId");
                            if (catPropObj != null)
                            {
                                var catValObj = catPropObj.GetValue(item);
                                if (catValObj != null && sensitiveCategoryIds.Contains(Convert.ToInt32(catValObj)))
                                {
                                    var idPropObj = item.GetType().GetProperty("Id") ?? item.GetType().GetProperty(item.GetType().Name + "Id");
                                    var sid = idPropObj?.GetValue(item)?.ToString() ?? "[unknown]";
                                    sensitiveToRecordDirect.Add(sid);
                                }
                            }
                        }
                        if (item is WindowsSpeechVoiceCommand ws && string.IsNullOrEmpty(ws.SpokenCommand)) ws.SpokenCommand = string.Empty;
                        targetDb.Add(item);
                    }
                    SanitizeWindowsSpeechVoiceCommands();
                    await targetDb.SaveChangesAsync();

                    if (sensitiveToRecordDirect.Count > 0)
                    {
                        foreach (var sid in sensitiveToRecordDirect)
                        {
                            await targetDb.Database.ExecuteSqlRawAsync("INSERT INTO MigratedSensitiveRows (TableName, SourceId, MigratedAt) VALUES ({0}, {1}, {2})", type.Name, sid, DateTime.UtcNow.ToString("o"));
                        }
                    }

                    targetDb.ChangeTracker.Clear();
                    insertedCount += list.Count;

                    offset += list.Count;
                    if (list.Count < batchSize) break;
                }
            }
            break;
        }
    }

    logger.LogInformation("Finished streaming remaining entities");
}

var retries = 0;
const int maxRetries = 5;
while (true)
{
    try
    {
        await targetDb.SaveChangesAsync();
        break;
    }
    catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
    {
        retries++;
        logger.LogWarning(ex, "Final SaveChanges failed (attempt {r}); attempting to detach conflicting Added entries and retry.", retries);
        var innerMsg = ex.InnerException?.Message ?? ex.Message;

        var matches = System.Text.RegularExpressions.Regex.Matches(innerMsg, @"UNIQUE constraint failed: *([^\.]+)\.");
        var detachedAny = false;

        if (matches.Count == 0)
        {
            logger.LogWarning("Could not parse table name from exception on attempt {r}; detaching all Added entries as fallback.", retries);
            foreach (var ent in targetDb.ChangeTracker.Entries().Where(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added).ToList())
            {
                ent.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                detachedAny = true;
            }
        }
        else
        {
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                var tn = m.Groups[1].Value;
                var et = targetDb.Model.GetEntityTypes().FirstOrDefault(t => string.Equals(t.GetTableName(), tn, StringComparison.OrdinalIgnoreCase));
                if (et != null && et.ClrType != null)
                {
                    foreach (var ent in targetDb.ChangeTracker.Entries().Where(e => e.Entity?.GetType() == et.ClrType && e.State == Microsoft.EntityFrameworkCore.EntityState.Added).ToList())
                    {
                        ent.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                        detachedAny = true;
                    }
                }
            }
        }

        if (!detachedAny)
        {
            logger.LogWarning("No Added entries detached on attempt {r}; aborting retries.", retries);
            break;
        }

        if (retries >= maxRetries)
        {
            logger.LogError("Reached max retries ({m}), aborting final save.", maxRetries);
            break;
        }

        logger.LogInformation("Retrying SaveChanges after detaching conflicting entries (attempt {r}).", retries + 1);
        // loop to retry
    }
}

// Re-enable foreign keys
await targetDb.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");

logger.LogInformation("Import complete: source counts — commands:{c}, lists:{l}, computers:{cmp}, languages:{lang}, categories:{cat}, launchers:{lnc}, transactions:{tx}",
    commandsCount, listsCount, computersCount, languagesCount, categoriesCount, launchersCount, transactionsCount);

if (verify)
{
    logger.LogInformation("Running verification after migration...");

    if (tablesSet == null || tablesSet.Contains("transactions"))
    {
        var srcTxIds = await sourceDb.Transactions.Select(t => t.Id).ToListAsync();
        var tgtTxIds = await targetDb.Transactions.Select(t => t.Id).ToListAsync();
        var missing = srcTxIds.Except(tgtTxIds).Take(5).ToList();
        logger.LogInformation("Transactions verification: source:{s}, target:{t}, missingInTargetSample:{m}", srcTxIds.Count, tgtTxIds.Count, string.Join(',', missing));
    }

    if (tablesSet != null && tablesSet.Contains("remaining") && remainingTypes != null && remainingTypes.Count > 0)
    {
        logger.LogInformation("Verifying remaining tables...");
        foreach (var type in remainingTypes)
        {
            var et = sourceDb.Model.FindEntityType(type);
            var tableName = et?.GetTableName();
            if (string.IsNullOrWhiteSpace(tableName)) continue;

            try
            {
                // Get source count via a new direct connection to avoid interfering with any active EF readers
                long srcCount = 0;
                try
                {
                    using var directSrcConn = new Microsoft.Data.SqlClient.SqlConnection(sourceConnection);
                    await directSrcConn.OpenAsync();
                    using var srcCmd = directSrcConn.CreateCommand();
                    srcCmd.CommandText = $"SELECT COUNT(1) FROM [{tableName}]";
                    var srcScalar = await srcCmd.ExecuteScalarAsync();
                    if (srcScalar != null && long.TryParse(srcScalar.ToString(), out var parsedSrc)) srcCount = parsedSrc;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to query source count for {t}", type.Name);
                }

                // Get target count via a new direct connection (SQLite) to avoid shared readers
                long tgtCount = 0;
                try
                {
                    var targetConnStr = targetConn; // captured earlier
                    using var directTgtConn = new Microsoft.Data.Sqlite.SqliteConnection(targetConnStr);
                    await directTgtConn.OpenAsync();
                    using var tgtCmd = directTgtConn.CreateCommand();
                    tgtCmd.CommandText = $"SELECT COUNT(1) FROM [{tableName}]";
                    var tgtScalar = await tgtCmd.ExecuteScalarAsync();
                    if (tgtScalar != null && long.TryParse(tgtScalar.ToString(), out var parsedTgt)) tgtCount = parsedTgt;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to query target count for {t}", type.Name);
                }

                logger.LogInformation("{t} verification: source:{s}, target:{tgt}", type.Name, srcCount, tgtCount);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to verify {t} (table may not exist in source/target); skipping verification for this table", type.Name);
                continue;
            }
        }
        logger.LogInformation("Remaining tables verification complete");
    }

    logger.LogInformation("Post-migration verification complete");

    // If requested, generate a delete script for migrated sensitive rows so these can be removed later
    if (includeSensitive && generateDeleteScript)
    {
        logger.LogInformation("Generating delete script for migrated sensitive rows into delete-sensitive.sql");
        var conn = targetDb.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT TableName, SourceId FROM MigratedSensitiveRows ORDER BY TableName";
        using var reader = await cmd.ExecuteReaderAsync();
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("-- Delete script for migrated sensitive rows generated on " + DateTime.UtcNow.ToString("o"));
        while (await reader.ReadAsync())
        {
            var table = reader.GetString(0);
            var id = reader.GetString(1);
            if (long.TryParse(id, out _))
                sb.AppendLine($"DELETE FROM [{table}] WHERE Id = {id};");
            else
                sb.AppendLine($"DELETE FROM [{table}] WHERE Id = '{id.Replace("'", "''")}';");
        }
        System.IO.File.WriteAllText("delete-sensitive.sql", sb.ToString());
        logger.LogInformation("Wrote delete-sensitive.sql");
    }
}

return 0;
