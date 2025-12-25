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

HashSet<string>? tablesSet = null;
if (!string.IsNullOrWhiteSpace(tablesArg))
{
    tablesSet = new HashSet<string>(tablesArg.Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.Trim().ToLowerInvariant()));
}

var loggerFactory = LoggerFactory.Create(lb => lb.AddConsole());
var logger = loggerFactory.CreateLogger("Migrator");

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

// Precompute non-sensitive category IDs — rows with Sensitive == true will be excluded from migration
var allowedCategoryIds = await sourceDb.Categories.Where(c => !c.Sensitive).Select(c => c.Id).ToListAsync();
logger.LogInformation("Non-sensitive categories found: {c}", allowedCategoryIds.Count);

int commandsCount = 0, listsCount = 0;
int computersCount = 0, languagesCount = 0, categoriesCount = allowedCategoryIds.Count, launchersCount = 0;
int applicationsCount = 0, intellisenseCount = 0, additionalCommandsCount = 0;
int valuesToInsertCount = 0, promptsCount = 0, quickPromptsCount = 0, vsCommandsCount = 0;

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

logger.LogInformation("Source counts: commands={c}, lists={l}, computers={cmp}, languages={lang}, categories={cat}, launchers={lnc}, apps={app}, intellisense={int}, addl={addl}, values={vals}, prompts={p}, quickPrompts={qp}, vsCommands={vs}",
    commandsCount, listsCount, computersCount, languagesCount, categoriesCount, launchersCount, applicationsCount, intellisenseCount, additionalCommandsCount, valuesToInsertCount, promptsCount, quickPromptsCount, vsCommandsCount);

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

if (tablesSet == null || tablesSet.Contains("commands") || tablesSet.Contains("talonvoicecommands"))
{
    logger.LogInformation("Reading commands from source...");
    commands = await sourceDb.TalonVoiceCommands.AsNoTracking().ToListAsync();
    logger.LogInformation("Read {c} commands", commands.Count);
}

if (tablesSet == null || tablesSet.Contains("lists") || tablesSet.Contains("talonlists"))
{
    logger.LogInformation("Reading lists from source...");
    lists = await sourceDb.TalonLists.AsNoTracking().ToListAsync();
    logger.LogInformation("Read {c} lists", lists.Count);
}

if (tablesSet == null || tablesSet.Contains("computers"))
{
    logger.LogInformation("Reading computers from source...");
    computers = await sourceDb.Computers.AsNoTracking().ToListAsync();
    logger.LogInformation("Read {c} computers", computers.Count);
}

if (tablesSet == null || tablesSet.Contains("languages"))
{
    logger.LogInformation("Reading languages from source...");
    languages = await sourceDb.Languages.AsNoTracking().ToListAsync();
    logger.LogInformation("Read {c} languages", languages.Count);
}

if (tablesSet == null || tablesSet.Contains("categories"))
{
    logger.LogInformation("Reading categories from source (excluding sensitive)...");
    categories = await sourceDb.Categories.AsNoTracking().Where(c => !c.Sensitive).ToListAsync();
    logger.LogInformation("Read {c} non-sensitive categories", categories.Count);
}

if (tablesSet == null || tablesSet.Contains("launchers"))
{
    logger.LogInformation("Reading launchers from source (excluding those in sensitive categories)...");
    launchers = await sourceDb.Launcher.AsNoTracking().Where(l => allowedCategoryIds.Contains(l.CategoryId)).ToListAsync();
    var rawCount = await sourceDb.Launcher.CountAsync();
    logger.LogInformation("Read {c} launchers (raw:{raw}, excluded:{excluded})", launchers.Count, rawCount, rawCount - launchers.Count);
}

if (tablesSet == null || tablesSet.Contains("applications") || tablesSet.Contains("applicationdetails"))
{
    logger.LogInformation("Reading application details from source...");
    applications = await sourceDb.ApplicationDetails.AsNoTracking().ToListAsync();
    logger.LogInformation("Read {c} application details", applications.Count);
}

if (tablesSet == null || tablesSet.Contains("intellisense") || tablesSet.Contains("customintellisenses"))
{
    logger.LogInformation("Reading custom intellisense from source (excluding those in sensitive categories)...");
    intellisenses = await sourceDb.CustomIntelliSenses.AsNoTracking().Where(i => allowedCategoryIds.Contains(i.CategoryId)).ToListAsync();
    var rawCount = await sourceDb.CustomIntelliSenses.CountAsync();
    logger.LogInformation("Read {c} custom intellisenses (raw:{raw}, excluded:{excluded})", intellisenses.Count, rawCount, rawCount - intellisenses.Count);
}

if (tablesSet == null || tablesSet.Contains("additionalcommands"))
{
    logger.LogInformation("Reading additional commands from source (only for non-sensitive custom intellisense)...");

    // Allowed CustomIntelliSense IDs: derived from already-filtered intellisenses (or queried if not yet loaded)
    HashSet<int> allowedCustomIntelliSenseIds = intellisenses.Count > 0
        ? intellisenses.Select(i => i.Id).ToHashSet()
        : new HashSet<int>(await sourceDb.CustomIntelliSenses.Where(i => allowedCategoryIds.Contains(i.CategoryId)).Select(i => i.Id).ToListAsync());

    additionalCommands = await sourceDb.AdditionalCommands.AsNoTracking()
        .Where(ac => allowedCustomIntelliSenseIds.Contains(ac.CustomIntelliSenseId))
        .ToListAsync();

    var rawCount = await sourceDb.AdditionalCommands.CountAsync();
    logger.LogInformation("Read {c} additional commands (raw:{raw}, excluded:{excluded})", additionalCommands.Count, rawCount, rawCount - additionalCommands.Count);
}

if (tablesSet == null || tablesSet.Contains("values") || tablesSet.Contains("valuestoinsert"))
{
    logger.LogInformation("Reading values to insert from source...");
    valuesToInsert = await sourceDb.ValuesToInserts.AsNoTracking().ToListAsync();
    logger.LogInformation("Read {c} values to insert", valuesToInsert.Count);
}

if (tablesSet == null || tablesSet.Contains("prompts"))
{
    logger.LogInformation("Reading prompts from source...");
    prompts = await sourceDb.Prompts.AsNoTracking().ToListAsync();
    logger.LogInformation("Read {c} prompts", prompts.Count);
}

if (tablesSet == null || tablesSet.Contains("quickprompts"))
{
    logger.LogInformation("Reading quick prompts from source...");
    quickPrompts = await sourceDb.QuickPrompts.AsNoTracking().ToListAsync();
    logger.LogInformation("Read {c} quick prompts", quickPrompts.Count);
}

if (tablesSet == null || tablesSet.Contains("vscommands") || tablesSet.Contains("visualstudiocommands"))
{
    logger.LogInformation("Reading visual studio commands from source...");
    vsCommands = await sourceDb.VisualStudioCommands.AsNoTracking().ToListAsync();
    logger.LogInformation("Read {c} visual studio commands", vsCommands.Count);
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

if (tablesSet == null || tablesSet.Contains("categories") || tablesSet.Contains("languages") || tablesSet.Contains("computers") || tablesSet.Contains("applications"))
{
    targetDb.Categories.RemoveRange(targetDb.Categories);
    targetDb.Languages.RemoveRange(targetDb.Languages);
    targetDb.Computers.RemoveRange(targetDb.Computers);
    targetDb.ApplicationDetails.RemoveRange(targetDb.ApplicationDetails);
}

await targetDb.SaveChangesAsync();

// Insert in FK-safe order — only insert tables that were read
if ((tablesSet == null || tablesSet.Contains("computers")) && computers.Count > 0) await targetDb.Computers.AddRangeAsync(computers);
if ((tablesSet == null || tablesSet.Contains("languages")) && languages.Count > 0) await targetDb.Languages.AddRangeAsync(languages);
if ((tablesSet == null || tablesSet.Contains("categories")) && categories.Count > 0) await targetDb.Categories.AddRangeAsync(categories);
if ((tablesSet == null || tablesSet.Contains("applications")) && applications.Count > 0) await targetDb.ApplicationDetails.AddRangeAsync(applications);

if ((tablesSet == null || tablesSet.Contains("intellisense")) && intellisenses.Count > 0) await targetDb.CustomIntelliSenses.AddRangeAsync(intellisenses);
if ((tablesSet == null || tablesSet.Contains("additionalcommands")) && additionalCommands.Count > 0) await targetDb.AdditionalCommands.AddRangeAsync(additionalCommands);

if ((tablesSet == null || tablesSet.Contains("launchers")) && launchers.Count > 0) await targetDb.Launcher.AddRangeAsync(launchers);
if ((tablesSet == null || tablesSet.Contains("lists")) && lists.Count > 0) await targetDb.TalonLists.AddRangeAsync(lists);
if ((tablesSet == null || tablesSet.Contains("commands")) && commands.Count > 0) await targetDb.TalonVoiceCommands.AddRangeAsync(commands);

if ((tablesSet == null || tablesSet.Contains("values")) && valuesToInsert.Count > 0) await targetDb.ValuesToInserts.AddRangeAsync(valuesToInsert);
if ((tablesSet == null || tablesSet.Contains("prompts")) && prompts.Count > 0) await targetDb.Prompts.AddRangeAsync(prompts);
if ((tablesSet == null || tablesSet.Contains("quickprompts")) && quickPrompts.Count > 0) await targetDb.QuickPrompts.AddRangeAsync(quickPrompts);
if ((tablesSet == null || tablesSet.Contains("vscommands")) && vsCommands.Count > 0) await targetDb.VisualStudioCommands.AddRangeAsync(vsCommands);

await targetDb.SaveChangesAsync();

// Re-enable foreign keys
await targetDb.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");

logger.LogInformation("Import complete: inserted {c} commands, {l} lists, {cmp} computers, {lang} languages, {cat} categories, {lnc} launchers",
    commands.Count, lists.Count, computers.Count, languages.Count, categories.Count, launchers.Count);

return 0;
