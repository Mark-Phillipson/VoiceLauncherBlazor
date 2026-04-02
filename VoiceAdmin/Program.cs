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

// Write startup marker directly to stderr (always available)
Console.Error.WriteLine($"[{DateTime.UtcNow:O}] VoiceAdmin startup initiated");
Console.Error.Flush();
try
{
    Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Creating WebApplicationBuilder...");
    
    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.UseStaticWebAssets();
// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
var environmentName = builder.Environment.EnvironmentName;
Console.Error.WriteLine($"[{DateTime.UtcNow:O}] EnvironmentName: {environmentName}");

Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Logging configured");

builder.AddServiceDefaults();
Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Service defaults added");

// Blazor Server services
var smartComponentsApiKey =
    builder.Configuration["SmartComponents:ApiKey"] ??
    builder.Configuration["OpenAI:ApiKey"] ??
    Environment.GetEnvironmentVariable("OPENAI_API_KEY");

var smartComponentsBuilder = builder.Services.AddSmartComponents();
if (!string.IsNullOrWhiteSpace(smartComponentsApiKey))
{
    try
    {
        smartComponentsBuilder.WithInferenceBackend<OpenAIInferenceBackend>();
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


var isDevLike = builder.Environment.IsDevelopment() ||
    environmentName.Equals("Local", StringComparison.OrdinalIgnoreCase) ||
    environmentName.Equals("LocalProduction", StringComparison.OrdinalIgnoreCase);
var isProdLike = builder.Environment.IsProduction() && !isDevLike;

Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Configuring SQLite database...");
Console.Error.WriteLine($"[{DateTime.UtcNow:O}] EnvironmentName: {environmentName}, IsDevelopment: {builder.Environment.IsDevelopment()}, IsProduction: {builder.Environment.IsProduction()}, IsDevLike: {isDevLike}, IsProdLike: {isProdLike}");

var configuredConnectionString = config.GetConnectionString("DefaultConnection");
var localFallbackConnectionString = config.GetConnectionString("LocalConnection");

string connectionString;

if (isProdLike)
{
    // Azure App Service exposes HOME as the stable, writable root. Sqlite needs an actual path, not a literal %HOME% token.
    var homePath = Environment.GetEnvironmentVariable("HOME");
    var fallbackAzureDbPath = !string.IsNullOrWhiteSpace(homePath)
        ? Path.Combine(homePath, "site", "data", "voicelauncher-azure.db")
        : Path.Combine(builder.Environment.ContentRootPath, "voicelauncher-azure.db");

    var rawConnectionString = string.IsNullOrWhiteSpace(configuredConnectionString)
        ? null
        : Environment.ExpandEnvironmentVariables(configuredConnectionString);

    // Check if connection string looks like SQL Server (not SQLite)
    if (!string.IsNullOrWhiteSpace(rawConnectionString) &&
        (rawConnectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
         rawConnectionString.Contains("Initial Catalog=", StringComparison.OrdinalIgnoreCase) ||
         rawConnectionString.Contains("Integrated Security=", StringComparison.OrdinalIgnoreCase) ||
         rawConnectionString.Contains("MultipleActiveResultSets=", StringComparison.OrdinalIgnoreCase)))
    {
        Console.Error.WriteLine($"[{DateTime.UtcNow:O}] WARNING: Connection string appears to be SQL Server format. Using fallback SQLite path.");
        rawConnectionString = null; // Reject it, use fallback
    }

    if (!string.IsNullOrWhiteSpace(rawConnectionString))
    {
        try
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
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{DateTime.UtcNow:O}] ERROR parsing connection string: {ex.Message}");
            Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Raw connection string was: {rawConnectionString}");
            Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Using fallback SQLite path instead");
            connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = fallbackAzureDbPath,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();
        }
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
            Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Ensured database directory exists: {databaseDirectory}");
        }

        if (!File.Exists(productionDatabasePath))
        {
            Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Production database not found at: {productionDatabasePath}");
            Console.Error.WriteLine($"[{DateTime.UtcNow:O}] ContentRootPath: {builder.Environment.ContentRootPath}");
            Console.Error.WriteLine($"[{DateTime.UtcNow:O}] WebRootPath: {builder.Environment.WebRootPath}");

            // List all files in ContentRootPath to help diagnose
            try
            {
                var contentRootFiles = Directory.GetFiles(builder.Environment.ContentRootPath, "*.db", SearchOption.AllDirectories);
                Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Found {contentRootFiles.Length} .db files in ContentRootPath:");
                foreach (var dbFile in contentRootFiles)
                {
                    var fileInfo = new System.IO.FileInfo(dbFile);
                    Console.Error.WriteLine($"[{DateTime.UtcNow:O}]   {dbFile} (size: {fileInfo.Length} bytes)");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{DateTime.UtcNow:O}] ERROR listing files: {ex.Message}");
            }

            var candidateSeedPaths = new[]
            {
                Path.Combine(builder.Environment.ContentRootPath, "voicelauncher-azure.db"),
                Path.Combine(builder.Environment.ContentRootPath, "data", "voicelauncher-azure.db"),
                Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "voicelauncher-azure.db"),
                Path.Combine(builder.Environment.WebRootPath ?? "", "voicelauncher-azure.db")
            };

            var seedDatabasePath = candidateSeedPaths.FirstOrDefault(p => File.Exists(p));
            Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Seed database found at: {seedDatabasePath}");

            if (!string.IsNullOrWhiteSpace(seedDatabasePath))
            {
                try
                {
                    Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Copying database from {seedDatabasePath} to {productionDatabasePath}");
                    File.Copy(seedDatabasePath, productionDatabasePath, overwrite: false);
                    Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Database copied successfully");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[{DateTime.UtcNow:O}] ERROR copying database: {ex}");
                }
            }
        }
        else
        {
            Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Production database already exists at: {productionDatabasePath}");
        }
    }
}
else
{
    // Non-production uses local path by default. Support explicit local connection string via LocalConnection.
    if (!string.IsNullOrWhiteSpace(localFallbackConnectionString))
    {
        connectionString = DataAccessLibrary.Configuration.DatabaseConfiguration.GetConnectionString(localFallbackConnectionString);
    }
    else
    {
        connectionString = DataAccessLibrary.Configuration.DatabaseConfiguration.GetConnectionString(configuredConnectionString);
    }
}

Console.WriteLine($"Using database connection: {connectionString}");
Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Adding DbContextFactory...");

builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlite(connectionString));
Console.Error.WriteLine($"[{DateTime.UtcNow:O}] DbContextFactory added");

builder.Services.AddRadzenComponents();
Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Radzen components added");

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
// ANCM out-of-process sets ASPNETCORE_URLS automatically; do not override with UseUrls
// (overriding with PORT ?? "80" would try to bind port 80 already owned by IIS → SocketException → 502.5)
Console.Error.WriteLine($"[{DateTime.UtcNow:O}] ASPNETCORE_URLS={Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "(not set)"}");
Console.Error.WriteLine($"[{DateTime.UtcNow:O}] ASPNETCORE_PORT={Environment.GetEnvironmentVariable("ASPNETCORE_PORT") ?? "(not set)"}");
Console.Error.Flush();

Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Calling builder.Build()...");
var app = builder.Build();
Console.Error.WriteLine($"[{DateTime.UtcNow:O}] builder.Build() succeeded");
Console.Error.Flush();

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

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName.Equals("Local", StringComparison.OrdinalIgnoreCase))
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapStaticAssets();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host"); // Ensure _Host exists from Server template

Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Mapping complete. Starting app.Run()...");
Console.Error.Flush();

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[{DateTime.UtcNow:O}] FATAL: app.Run() threw exception: {ex}");
    Console.Error.Flush();
    throw;
}
finally
{
    Console.Error.WriteLine($"[{DateTime.UtcNow:O}] app.Run() exited");
    Console.Error.Flush();
}
}
catch (Exception outerEx)
{
    Console.Error.WriteLine($"[{DateTime.UtcNow:O}] FATAL OUTER EXCEPTION during startup: {outerEx}");
    Console.Error.WriteLine($"Message: {outerEx.Message}");
    Console.Error.WriteLine($"StackTrace: {outerEx.StackTrace}");
    Console.Error.Flush();
    throw;
}
