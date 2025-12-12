using System.Reflection;
using Blazored.Modal;
using Blazored.Toast;
using DataAccessLibrary;
using DataAccessLibrary.Models;
using DataAccessLibrary.Repositories;
using DataAccessLibrary.Services;
using Microsoft.EntityFrameworkCore;
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
try
{
    builder.Services.AddSmartComponents().WithInferenceBackend<OpenAIInferenceBackend>();
}
catch (Exception ex)
{
    Console.WriteLine($"Error registering SmartComponents or OpenAI backend: {ex}");
    throw;
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
string? connectionString = builder.Configuration.GetConnectionString("VoiceLauncher");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddSmartComponents().WithInferenceBackend<OpenAIInferenceBackend>();
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
builder.Services.AddScoped<ITodoData, TodoData>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<VisualStudioCommandService>();
builder.Services.AddScoped<CommandSetService>();
builder.Services.AddScoped<LauncherMultipleLauncherBridgeDataService>();
builder.Services.AddSingleton<NotifierService>();
builder.Services.AddSingleton<IWindowsService, WindowsService>();
try
{
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
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
builder.Services.AddScoped<DataAccessLibrary.Services.IClubMemberDataService, DataAccessLibrary.Services.ClubMemberDataService>();
builder.Services.Configure<JsonRepositoryOptions>(config.GetSection("JsonRepository"));
builder.Services.AddScoped<DataAccessLibrary.Repositories.ITalonListRepository, DataAccessLibrary.Repositories.TalonListRepository>();
builder.Services.AddScoped<RazorClassLibrary.Services.ITalonListDataService, RazorClassLibrary.Services.TalonListDataService>();
builder.Services.AddScoped<ICursorlessCheatsheetItemJsonRepository, VoiceAdmin.CursorlessCheatsheetItemJsonRepository>();

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
