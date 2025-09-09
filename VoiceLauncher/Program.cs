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

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazoredModal();
builder.Services.AddBlazoredToast();
builder.Services.AddScoped<RazorClassLibrary.Services.ComponentCacheService>();
var config = builder.Configuration;
string? connectionString = builder.Configuration.GetConnectionString("VoiceLauncher");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
// Register EF-backed implementation of the shared ITalonRepository so RazorClassLibrary components can persist
builder.Services.AddScoped<RCLTalonShared.Services.ITalonRepository, DataAccessLibrary.Services.EfCoreTalonRepository>();
builder.Services.AddSmartComponents()
	.WithInferenceBackend<OpenAIInferenceBackend>();
builder.Services.AddSingleton<LocalEmbedder>();
builder.Services.AddRadzenComponents();
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
        Console.WriteLine(loaderException?.Message);
    }
    throw; // Re-throw the exception after logging
}
builder.Services.AddScoped<ISavedMousePositionRepository, SavedMousePositionRepository>();
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
builder.Services.AddScoped<TalonVoiceCommandDataService>();
// Ensure the interface is registered so services depending on ITalonVoiceCommandDataService can be resolved
builder.Services.AddScoped<DataAccessLibrary.Services.ITalonVoiceCommandDataService, DataAccessLibrary.Services.TalonVoiceCommandDataService>();
// Register server-side Talon analysis implementation (kept as concrete type to avoid compile-time coupling during migration)
builder.Services.AddScoped(typeof(RazorClassLibrary.Services.ITalonAnalysisService), typeof(VoiceLauncher.Services.ServerTalonAnalysisService));
// Register configuration
builder.Services.Configure<JsonRepositoryOptions>(config.GetSection("JsonRepository"));

builder.Services.AddScoped<ICursorlessCheatsheetItemJsonRepository, VoiceLauncher.CursorlessCheatsheetItemJsonRepository>();
var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
// Compute the embeddings once up front
var embedder = app.Services.GetRequiredService<LocalEmbedder>();
var embeddings = embedder.EmbedRange(new[] { "indent", "inspect", "move", "paste ", "phones", "post", "pour", "pre", "puff", "quick fix", "reference", "rename", "reverse", "scout", "scout all", "shuffle", "snippet", "snippet make", "sort", "swap", "take", "type deaf", "unfold", "after", "before", "to", "form", "chuck", "crown", "centre", "bottom", "drink", "repack", "wrap", "break", "breakpoint", "bring", "carve", "change", "clone", "clone up", "comment", "concrete", "copy", "decrement", "increment", "dedent", "define", "drop", "extract", "float", "fold" });

app.MapSmartComboBox("api/cursorless-spokenforms",
	request => embedder.FindClosest(request.Query, embeddings));

var expenseCategories = embedder.EmbedRange(
	["Groceries", "Utilities", "Rent", "Mortgage", "Car Payment", "Car Insurance", "Health Insurance", "Life Insurance", "Home Insurance", "Gas", "Public Transportation", "Dining Out", "Entertainment", "Travel", "Clothing", "Electronics", "Home Improvement", "Gifts", "Charity", "Education", "Childcare", "Pet Care", "Other"]);
var issueLabels = embedder.EmbedRange(
	["Bug", "Docs", "Enhancement", "Question", "UI (Android)", "UI (iOS)", "UI (Windows)", "UI (Mac)", "Performance", "Security", "Authentication", "Accessibility"]);

app.MapSmartComboBox("/api/suggestions/expense-category",
	request => embedder.FindClosest(request.Query, expenseCategories));

app.MapSmartComboBox("/api/suggestions/issue-label",
	request => embedder.FindClosest(request.Query, issueLabels));

app.Run();


