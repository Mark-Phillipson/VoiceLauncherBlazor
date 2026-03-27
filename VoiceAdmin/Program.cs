using System.Reflection;
using Blazored.Modal;
using Blazored.Toast;
using DataAccessLibrary;
using DataAccessLibrary.Models;
using DataAccessLibrary.Repositories;
using DataAccessLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Radzen;
using SampleApplication.Repositories;
using SampleApplication.Services;
using SmartComponents.Inference.OpenAI;
using SmartComponents.LocalEmbeddings;
using VoiceLauncher.Repositories;
using VoiceLauncher.Services;
using RazorClassLibrary.Services;
using VoiceAdmin;

var builder = WebApplication.CreateBuilder(args);
// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.AddServiceDefaults();

// Blazor Server services
var smartComponentsApiKey =
    builder.Configuration["SmartComponents:ApiKey"] ??
    builder.Configuration["OpenAI:ApiKey"] ??
    Environment.GetEnvironmentVariable("OPENAI_API_KEY");

builder.Services.AddSmartComponents();
if (!string.IsNullOrWhiteSpace(smartComponentsApiKey))
{
    try
    {
        builder.Services.AddSmartComponents().WithInferenceBackend<OpenAIInferenceBackend>();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"OpenAI backend registration failed; continuing without OpenAI inference backend. {ex}");
    }
}
else
{
    Console.WriteLine("OpenAI API key not configured. Continuing without OpenAI inference backend.");
}
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
// Increase resilience for transient disconnects: extend SignalR and circuit retention timeouts
builder.Services.AddSignalR(options =>
{
    // Allow a longer client timeout so brief network glitches don't cause immediate disconnects
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
    // KeepAliveInterval controls how often the server pings the client (default 15s)
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    // Handshake timeout for initial negotiation
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddBlazoredModal();
builder.Services.AddBlazoredToast();
builder.Services.AddScoped<RazorClassLibrary.Services.ComponentCacheService>();
var config = builder.Configuration;

// Phase 2: environment-sensitive SQLite configuration (local vs. Azure App Service)
var configuredConnectionString = config.GetConnectionString("DefaultConnection");

string connectionString;
if (builder.Environment.IsProduction())
{
    // Azure App Service exposes HOME as the stable, writable root. Sqlite needs an actual path, not a literal %HOME% token.
    var homePath = Environment.GetEnvironmentVariable("HOME");
    var fallbackAzureDbPath = !string.IsNullOrWhiteSpace(homePath)
        ? Path.Combine(homePath, "site", "data", "voicelauncher-azure.db")
        : Path.Combine(builder.Environment.ContentRootPath, "voicelauncher-azure.db");

    var rawConnectionString = string.IsNullOrWhiteSpace(configuredConnectionString)
        ? null
        : Environment.ExpandEnvironmentVariables(configuredConnectionString);

    if (!string.IsNullOrWhiteSpace(rawConnectionString))
    {
        var sqliteBuilder = new SqliteConnectionStringBuilder(rawConnectionString);
        if (string.IsNullOrWhiteSpace(sqliteBuilder.DataSource) ||
            sqliteBuilder.DataSource.Contains('%') ||
            sqliteBuilder.DataSource.Contains("$HOME", StringComparison.OrdinalIgnoreCase))
        {
            sqliteBuilder.DataSource = fallbackAzureDbPath;
        }

        if (!Path.IsPathRooted(sqliteBuilder.DataSource))
        {
            sqliteBuilder.DataSource = Path.Combine(builder.Environment.ContentRootPath, sqliteBuilder.DataSource);
        }

        connectionString = sqliteBuilder.ToString();
    }
    else
    {
        connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = fallbackAzureDbPath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();
    }

    var productionSqliteBuilder = new SqliteConnectionStringBuilder(connectionString);
    if (Path.IsPathRooted(productionSqliteBuilder.DataSource))
    {
        var productionDatabasePath = productionSqliteBuilder.DataSource;
        var databaseDirectory = Path.GetDirectoryName(productionSqliteBuilder.DataSource);
        if (!string.IsNullOrWhiteSpace(databaseDirectory))
        {
            Directory.CreateDirectory(databaseDirectory);
        }

        if (!File.Exists(productionDatabasePath))
        {
            var candidateSeedPaths = new[]
            {
                Path.Combine(builder.Environment.ContentRootPath, "voicelauncher-azure.db"),
                Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "voicelauncher-azure.db"),
                Path.Combine(builder.Environment.WebRootPath ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot"), "voicelauncher-azure.db")
            };

            var seedDatabasePath = candidateSeedPaths.FirstOrDefault(File.Exists);
            if (!string.IsNullOrWhiteSpace(seedDatabasePath))
            {
                File.Copy(seedDatabasePath, productionDatabasePath, overwrite: false);
            }
        }
    }
}
else
{
    // Non-production uses local AppData path unless explicitly configured.
    connectionString = DataAccessLibrary.Configuration.DatabaseConfiguration.GetConnectionString(configuredConnectionString);
}

Console.WriteLine($"Using database connection: {connectionString}");

builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddSingleton<LocalEmbedder>();
builder.Services.AddRadzenComponents();

// Keep disconnected circuits around longer so users can resume after short server/network blips.
builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
{
    // Default retention period is short; increase to 10 minutes to tolerate transient restarts.
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(40);
});

// Business services
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<CreateCommands>();
builder.Services.AddScoped<AdditionalCommandService>();
builder.Services.AddScoped<LanguageService>();
builder.Services.AddScoped<LauncherService>();
builder.Services.AddScoped<ComputerService>();
builder.Services.AddScoped<CustomIntellisenseService>();
builder.Services.AddScoped<GeneralLookupService>();
builder.Services.AddScoped<ISqlDataAccess, SqlDataAccess>();
		builder.Services.AddScoped<ITodoData, TodoDataEf>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<VisualStudioCommandService>();
builder.Services.AddScoped<CommandSetService>();
builder.Services.AddScoped<LauncherMultipleLauncherBridgeDataService>();
builder.Services.AddSingleton<NotifierService>();
builder.Services.AddSingleton<IWindowsService, WindowsService>();
try
{
    builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());
}
catch (ReflectionTypeLoadException ex)
{
    foreach (var loaderException in ex.LoaderExceptions)
    {
        Console.WriteLine($"AutoMapper loader exception: {loaderException}");
    }
    throw;
}

// Repository & Data services
builder.Services.AddScoped<ISavedMousePositionRepository, SavedMousePositionRepository>();
builder.Services.AddScoped<ApplicationMappingService>();
builder.Services.AddScoped<ISavedMousePositionDataService, SavedMousePositionDataService>();
builder.Services.AddScoped<ICustomWindowsSpeechCommandDataService, CustomWindowsSpeechCommandDataService>();
builder.Services.AddScoped<ICustomWindowsSpeechCommandRepository, CustomWindowsSpeechCommandRepository>();
builder.Services.AddScoped<IWindowsSpeechVoiceCommandRepository, WindowsSpeechVoiceCommandRepository>();
builder.Services.AddScoped<IWindowsSpeechVoiceCommandDataService, WindowsSpeechVoiceCommandDataService>();
builder.Services.AddScoped<IGrammarNameRepository, GrammarNameRepository>();
builder.Services.AddScoped<IGrammarNameDataService, GrammarNameDataService>();
builder.Services.AddScoped<IGrammarItemRepository, GrammarItemRepository>();
builder.Services.AddScoped<IGrammarItemDataService, GrammarItemDataService>();
builder.Services.AddScoped<IHtmlTagRepository, HtmlTagRepository>();
builder.Services.AddScoped<IHtmlTagDataService, HtmlTagDataService>();
builder.Services.AddScoped<IApplicationDetailRepository, ApplicationDetailRepository>();
builder.Services.AddScoped<IApplicationDetailDataService, ApplicationDetailDataService>();
builder.Services.AddScoped<IIdiosyncrasyRepository, IdiosyncrasyRepository>();
builder.Services.AddScoped<IIdiosyncrasyDataService, IdiosyncrasyDataService>();
builder.Services.AddScoped<IPhraseListGrammarRepository, PhraseListGrammarRepository>();
builder.Services.AddScoped<IPhraseListGrammarDataService, PhraseListGrammarDataService>();
builder.Services.AddScoped<ILauncherRepository, LauncherRepository>();
builder.Services.AddScoped<ILauncherDataService, LauncherDataService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryDataService, CategoryDataService>();
builder.Services.AddScoped<IValueToInsertRepository, ValueToInsertRepository>();
builder.Services.AddScoped<IValueToInsertDataService, ValueToInsertDataService>();
builder.Services.AddScoped<ISpokenFormRepository, SpokenFormRepository>();
builder.Services.AddScoped<ISpokenFormDataService, SpokenFormDataService>();
builder.Services.AddScoped<IMicrophoneRepository, MicrophoneRepository>();
builder.Services.AddScoped<IMicrophoneDataService, MicrophoneDataService>();
builder.Services.AddScoped<ICustomIntelliSenseRepository, CustomIntelliSenseRepository>();
builder.Services.AddScoped<ICustomIntelliSenseDataService, CustomIntelliSenseDataService>();
builder.Services.AddScoped<ITalonAlphabetRepository, TalonAlphabetRepository>();
builder.Services.AddScoped<ITalonAlphabetDataService, TalonAlphabetDataService>();
builder.Services.AddScoped<IPromptRepository, PromptRepository>();
builder.Services.AddScoped<IPromptDataService, PromptDataService>();
builder.Services.AddScoped<ILanguageRepository, LanguageRepository>();
builder.Services.AddScoped<ILanguageDataService, LanguageDataService>();
builder.Services.AddScoped<ICursorlessCheatsheetItemRepository, CursorlessCheatsheetItemRepository>();
builder.Services.AddScoped<ICursorlessCheatsheetItemDataService, CursorlessCheatsheetItemDataService>();
builder.Services.AddScoped<ICssPropertyRepository, CssPropertyRepository>();
builder.Services.AddScoped<ICssPropertyDataService, CssPropertyDataService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionDataService, TransactionDataService>();
builder.Services.AddScoped<ITransactionTypeMappingRepository, TransactionTypeMappingRepository>();
builder.Services.AddScoped<ITransactionTypeMappingDataService, TransactionTypeMappingDataService>();
builder.Services.AddScoped<IExampleRepository, ExampleRepository>();
builder.Services.AddScoped<IExampleDataService, ExampleDataService>();
builder.Services.AddScoped<IQuickPromptRepository, QuickPromptRepository>();
builder.Services.AddScoped<IQuickPromptDataService, QuickPromptDataService>();
builder.Services.AddScoped<DataAccessLibrary.Services.ITalonVoiceCommandDataService, TalonVoiceCommandDataService>();
builder.Services.AddScoped<ITalonAnalysisService, TalonAnalysisService>();
builder.Services.Configure<JsonRepositoryOptions>(config.GetSection("JsonRepository"));
builder.Services.AddScoped<DataAccessLibrary.Repositories.ITalonListRepository, DataAccessLibrary.Repositories.TalonListRepository>();
builder.Services.AddScoped<RazorClassLibrary.Services.ITalonListDataService, RazorClassLibrary.Services.TalonListDataService>();
builder.Services.AddScoped<ICursorlessCheatsheetItemJsonRepository, VoiceAdmin.CursorlessCheatsheetItemJsonRepository>();
builder.Services.AddScoped<IFaceImageRepository, FaceImageRepository>();
builder.Services.AddScoped<IFaceTagRepository, FaceTagRepository>();

var app = builder.Build();
// Log all incoming requests and their paths
app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("RequestLogger");
    logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
    await next();
});
// Global error logging for unhandled exceptions
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalException");
        logger.LogError(ex, "Unhandled exception");
        throw;
    }
});

app.MapDefaultEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapStaticAssets();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host"); // Ensure _Host exists from Server template

// Precompute embeddings
var embedder = app.Services.GetRequiredService<LocalEmbedder>();
var embeddings = embedder.EmbedRange(new[] { "indent", "inspect", "move", "paste ", "phones", "post", "pour", "pre", "puff", "quick fix", "reference", "rename", "reverse", "scout", "scout all", "shuffle", "snippet", "snippet make", "sort", "swap", "take", "type deaf", "unfold", "after", "before", "to", "form", "chuck", "crown", "centre", "bottom", "drink", "repack", "wrap", "break", "breakpoint", "bring", "carve", "change", "clone", "clone up", "comment", "concrete", "copy", "decrement", "increment", "dedent", "define", "drop", "extract", "float", "fold" });
app.MapSmartComboBox("api/cursorless-spokenforms", request => embedder.FindClosest(request.Query, embeddings));
var expenseCategories = embedder.EmbedRange(["Groceries", "Utilities", "Rent", "Mortgage", "Car Payment", "Car Insurance", "Health Insurance", "Life Insurance", "Home Insurance", "Gas", "Public Transportation", "Dining Out", "Entertainment", "Travel", "Clothing", "Electronics", "Home Improvement", "Gifts", "Charity", "Education", "Childcare", "Pet Care", "Other"]);
var issueLabels = embedder.EmbedRange(["Bug", "Docs", "Enhancement", "Question", "UI (Android)", "UI (iOS)", "UI (Windows)", "UI (Mac)", "Performance", "Security", "Authentication", "Accessibility"]);
app.MapSmartComboBox("/api/suggestions/expense-category", request => embedder.FindClosest(request.Query, expenseCategories));
app.MapSmartComboBox("/api/suggestions/issue-label", request => embedder.FindClosest(request.Query, issueLabels));

app.Run();
