using Bogus;
using DataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text;

string? GetArg(string name)
{
    for (int i = 0; i < args.Length; i++)
    {
        if (args[i] == name && i + 1 < args.Length) return args[i + 1];
        if (args[i].StartsWith(name + "=", StringComparison.OrdinalIgnoreCase)) return args[i].Substring(name.Length + 1);
    }
    return null;
}

var sourcePath = GetArg("--source");
var destinationPath = GetArg("--output");
if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(destinationPath))
{
    Console.WriteLine("Usage: DatabaseSanitizer --source <source.db> --output <destination.db>");
    return;
}

if (!File.Exists(sourcePath))
{
    Console.WriteLine($"Source file not found: {sourcePath}");
    return;
}

var targetFile = destinationPath;
if (File.Exists(targetFile))
{
    Console.WriteLine($"Deleting existing output database {targetFile}");
    File.Delete(targetFile);
}

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
var sourceOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite($"Data Source={sourcePath}").Options;
var destinationOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite($"Data Source={targetFile}").Options;
var emptyConfig = new ConfigurationBuilder().AddInMemoryCollection().Build();

using var sourceDb = new ApplicationDbContext(sourceOptions, emptyConfig);
using var destinationDb = new ApplicationDbContext(destinationOptions, emptyConfig);

Console.WriteLine("Creating destination database schema...");
destinationDb.Database.EnsureCreated();

var report = new StringBuilder();

static bool IsHardDriveLauncher(string commandLine)
{
    if (string.IsNullOrWhiteSpace(commandLine)) return false;
    var trimmed = commandLine.Trim();

    // Remove leading/trailing quotes, so "C:\path\file" still gets recognized as local
    if ((trimmed.StartsWith("\"") && trimmed.EndsWith("\"")) ||
        (trimmed.StartsWith("'") && trimmed.EndsWith("'")))
    {
        trimmed = trimmed[1..^1].Trim();
    }

    // Local file paths to exclude
    if (trimmed.StartsWith("file://", StringComparison.OrdinalIgnoreCase)) return true;
    if (trimmed.StartsWith("\\\\")) return true; // UNC paths
    if (trimmed.StartsWith("/")) return true; // absolute Unix-like path
    if (trimmed.StartsWith(".\\") || trimmed.StartsWith("../") || trimmed.StartsWith("./")) return true;
    if (trimmed.Length >= 3 && char.IsLetter(trimmed[0]) && trimmed[1] == ':' && (trimmed[2] == '\\' || trimmed[2] == '/')) return true;

    return false;
}

// Determine safe category set
var allCategories = await sourceDb.Categories.AsNoTracking().ToListAsync();
var sensitiveCategoryIds = allCategories.Where(c => c.Sensitive).Select(c => c.Id).ToHashSet();
var safeCategoryIds = allCategories.Where(c => !c.Sensitive).Select(c => c.Id).ToHashSet();

report.AppendLine($"Total Categories in source: {allCategories.Count}");
report.AppendLine($"Sensitive Categories: {sensitiveCategoryIds.Count}");
report.AppendLine($"Non-sensitive Categories: {safeCategoryIds.Count}");
report.AppendLine($"Child data will be filtered to non-sensitive categories only.");

// 1. Copy categories (all categories, including sensitive)
destinationDb.Categories.AddRange(allCategories);
await destinationDb.SaveChangesAsync();
report.AppendLine($"Copied all categories (including sensitive): {allCategories.Count}");
report.AppendLine($"Sensitive categories excluded from child entity copying: {sensitiveCategoryIds.Count}");

// 2. Copy base tables that are not sensitive-coded
void CopyEntities<TEntity>(DbSet<TEntity> sourceSet, DbSet<TEntity> destSet, string entityName)
    where TEntity : class
{
    var rows = sourceSet.AsNoTracking().ToList();
    if (!rows.Any()) return;
    try
    {
        destSet.AddRange(rows);
        destinationDb.SaveChanges();
        report.AppendLine($"Copied {rows.Count} rows into {entityName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error copying {entityName}: {ex.GetType().Name} - {ex.Message}");
        throw;
    }
}

// Non-category-related reference data.
CopyEntities(sourceDb.Computers, destinationDb.Computers, nameof(sourceDb.Computers));
CopyEntities(sourceDb.Languages, destinationDb.Languages, nameof(sourceDb.Languages));
CopyEntities(sourceDb.ApplicationDetails, destinationDb.ApplicationDetails, nameof(sourceDb.ApplicationDetails));
CopyEntities(sourceDb.Prompts, destinationDb.Prompts, nameof(sourceDb.Prompts));
CopyEntities(sourceDb.QuickPrompts, destinationDb.QuickPrompts, nameof(sourceDb.QuickPrompts));
CopyEntities(sourceDb.VisualStudioCommands, destinationDb.VisualStudioCommands, nameof(sourceDb.VisualStudioCommands));
CopyEntities(sourceDb.GrammarNames, destinationDb.GrammarNames, nameof(sourceDb.GrammarNames));
CopyEntities(sourceDb.GrammarItems, destinationDb.GrammarItems, nameof(sourceDb.GrammarItems));
CopyEntities(sourceDb.PhraseListGrammars, destinationDb.PhraseListGrammars, nameof(sourceDb.PhraseListGrammars));
// PhraseListGrammarStorages maps to the same underlying table as PhraseListGrammars, avoid duplicate key insert.
// CopyEntities(sourceDb.PhraseListGrammarStorages, destinationDb.PhraseListGrammarStorages, nameof(sourceDb.PhraseListGrammarStorages));
CopyEntities(sourceDb.CssProperties, destinationDb.CssProperties, nameof(sourceDb.CssProperties));
CopyEntities(sourceDb.CursorlessCheatsheetItems, destinationDb.CursorlessCheatsheetItems, nameof(sourceDb.CursorlessCheatsheetItems));
CopyEntities(sourceDb.Microphones, destinationDb.Microphones, nameof(sourceDb.Microphones));

// 3. Copy category-related tables filtering by safeCategoryIds and existing computers/languages
var safeComputerIds = sourceDb.Computers.AsNoTracking().Select(c => c.Id).ToHashSet();
var safeLanguageIds = sourceDb.Languages.AsNoTracking().Select(l => l.Id).ToHashSet();
var allCandidateLaunchers = sourceDb.Launcher
    .Where(l => safeCategoryIds.Contains(l.CategoryId)
                && (l.ComputerId == null || safeComputerIds.Contains(l.ComputerId.Value)))
    .AsNoTracking()
    .ToList();

var hardDriveLaunchers = allCandidateLaunchers.Where(l => IsHardDriveLauncher(l.CommandLine)).ToList();
var safeLaunchers = allCandidateLaunchers.Where(l => !IsHardDriveLauncher(l.CommandLine)).ToList();

destinationDb.Launcher.AddRange(safeLaunchers);
await destinationDb.SaveChangesAsync();
report.AppendLine($"Copied non-sensitive launchers (excluding hard-drive paths): {safeLaunchers.Count}");
report.AppendLine($"Excluded launchers pointing to local file paths: {hardDriveLaunchers.Count}");

var safeIntellisense = sourceDb.CustomIntelliSenses
    .Where(i => safeCategoryIds.Contains(i.CategoryId)
                && safeLanguageIds.Contains(i.LanguageId)
                && (i.ComputerId == null || safeComputerIds.Contains(i.ComputerId.Value)))
    .AsNoTracking()
    .ToList();
destinationDb.CustomIntelliSenses.AddRange(safeIntellisense);
await destinationDb.SaveChangesAsync();
report.AppendLine($"Copied non-sensitive custom intellisense: {safeIntellisense.Count}");

// LauncherCategoryBridges refer categories/launchers, keep only safe refs
var safeLauncherIds = safeLaunchers.Select(x => x.Id).ToHashSet();
var safeBridges = sourceDb.LauncherCategoryBridges
    .Where(b => safeCategoryIds.Contains(b.CategoryId) && safeLauncherIds.Contains(b.LauncherId))
    .AsNoTracking()
    .ToList();
destinationDb.LauncherCategoryBridges.AddRange(safeBridges);
await destinationDb.SaveChangesAsync();
report.AppendLine($"Copied non-sensitive launcher-category bridges: {safeBridges.Count}");

// 4. Copy additional entities (non-sensitive by default)
var safeIntelliSenseIds = safeIntellisense.Select(i => i.Id).ToHashSet();
CopyEntities(sourceDb.TalonVoiceCommands, destinationDb.TalonVoiceCommands, nameof(sourceDb.TalonVoiceCommands));
CopyEntities(sourceDb.TalonLists, destinationDb.TalonLists, nameof(sourceDb.TalonLists));
var safeAdditionalCommands = sourceDb.AdditionalCommands
    .Where(a => safeIntelliSenseIds.Contains(a.CustomIntelliSenseId))
    .AsNoTracking()
    .ToList();
if (safeAdditionalCommands.Any())
{
    destinationDb.AdditionalCommands.AddRange(safeAdditionalCommands);
    destinationDb.SaveChanges();
    report.AppendLine($"Copied {safeAdditionalCommands.Count} rows into AdditionalCommands");
}
CopyEntities(sourceDb.MigrationHistory, destinationDb.MigrationHistory, nameof(sourceDb.MigrationHistory));
CopyEntities(sourceDb.MousePositions, destinationDb.MousePositions, nameof(sourceDb.MousePositions));
CopyEntities(sourceDb.MultipleLauncher, destinationDb.MultipleLauncher, nameof(sourceDb.MultipleLauncher));
var safeMultipleLauncherIds = sourceDb.MultipleLauncher.AsNoTracking().Select(m => m.Id).ToHashSet();
var safeLauncherMultipleLauncherBridges = sourceDb.LauncherMultipleLauncherBridge
    .Where(b => safeLauncherIds.Contains(b.LauncherId) && safeMultipleLauncherIds.Contains(b.MultipleLauncherId))
    .AsNoTracking()
    .ToList();
if (safeLauncherMultipleLauncherBridges.Any())
{
    destinationDb.LauncherMultipleLauncherBridge.AddRange(safeLauncherMultipleLauncherBridges);
    destinationDb.SaveChanges();
    report.AppendLine($"Copied {safeLauncherMultipleLauncherBridges.Count} rows into LauncherMultipleLauncherBridge");
}
var safeWindowsSpeechVoiceCommandIds = sourceDb.WindowsSpeechVoiceCommands.AsNoTracking().Select(x => x.Id).ToHashSet();
var safeCustomWindowsSpeechCommands = sourceDb.CustomWindowsSpeechCommands
    .Where(x => x.WindowsSpeechVoiceCommandId == null || safeWindowsSpeechVoiceCommandIds.Contains(x.WindowsSpeechVoiceCommandId.Value))
    .AsNoTracking()
    .ToList();
CopyEntities(sourceDb.WindowsSpeechVoiceCommands, destinationDb.WindowsSpeechVoiceCommands, nameof(sourceDb.WindowsSpeechVoiceCommands));
if (safeCustomWindowsSpeechCommands.Any())
{
    destinationDb.CustomWindowsSpeechCommands.AddRange(safeCustomWindowsSpeechCommands);
    destinationDb.SaveChanges();
    report.AppendLine($"Copied {safeCustomWindowsSpeechCommands.Count} rows into CustomWindowsSpeechCommands");
}
CopyEntities(sourceDb.SpokenForms, destinationDb.SpokenForms, nameof(sourceDb.SpokenForms));

// 5. Generate dummy Transactions and ValuesToInsert instead of copying source to avoid leaking sensitive financial data
var faker = new Faker();
int txCount = faker.Random.Int(120, 250);
var transactions = new List<Transaction>(txCount);
var balance = faker.Random.Decimal(1000m, 10000m);
var types = new[] { "Credit", "Debit", "Transfer", "Fee" };
for (int i = 0; i < txCount; i++)
{
    bool isCredit = faker.Random.Bool();
    var moneyIn = isCredit ? faker.Random.Decimal(1m, 500m) : 0m;
    var moneyOut = isCredit ? 0m : faker.Random.Decimal(1m, 400m);
    balance += moneyIn;
    balance -= moneyOut;

    transactions.Add(new Transaction
    {
        Date = DateTime.UtcNow.Date.AddDays(-faker.Random.Int(0, 365)),
        Description = faker.Lorem.Sentence(5, 8),
        Type = isCredit ? "Credit" : "Debit",
        MoneyIn = Math.Round(moneyIn, 2),
        MoneyOut = Math.Round(moneyOut, 2),
        Balance = Math.Round(balance, 2),
        MyTransactionType = faker.PickRandom(types),
        ImportFilename = "sanitized-demo.csv",
        ImportDate = DateTime.UtcNow
    });
}
destinationDb.Transactions.AddRange(transactions);
await destinationDb.SaveChangesAsync();
report.AppendLine($"Added {transactions.Count} dummy transactions");

int valueCount = faker.Random.Int(50, 120);
var values = new List<ValuesToInsert>(valueCount);
for (int i = 0; i < valueCount; i++)
{
    values.Add(new ValuesToInsert
    {
        ValueToInsertValue = faker.Commerce.ProductName() + " " + faker.Random.AlphaNumeric(4),
        Lookup = faker.Hacker.Noun(),
        Description = faker.Lorem.Sentence(6)
    });
}
destinationDb.ValuesToInserts.AddRange(values);
await destinationDb.SaveChangesAsync();
report.AppendLine($"Added {values.Count} dummy ValuesToInsert rows");

// 6. Sanitized login setup
if (destinationDb.Logins.Any())
{
    destinationDb.Logins.RemoveRange(destinationDb.Logins);
    await destinationDb.SaveChangesAsync();
}

destinationDb.Logins.Add(new Logins
{
    Name = "demo",
    Username = "demo.user",
    Password = "password123",
    Description = "Demo login account; no sensitive data"
});
await destinationDb.SaveChangesAsync();
report.AppendLine("Inserted 1 demo Login row (no real credentials)");

var reportPath = Path.Combine(Path.GetDirectoryName(targetFile) ?? ".", "DatabaseSanitizerReport.txt");
File.WriteAllText(reportPath, report.ToString());
Console.WriteLine("Sanitization complete.");
Console.WriteLine($"Output DB: {targetFile}");
Console.WriteLine($"Report: {reportPath}");

