using Blazored.Modal;
using Blazored.Toast;

using DataAccessLibrary;
using DataAccessLibrary.Models;
using DataAccessLibrary.Repositories;
using DataAccessLibrary.Services;

using Microsoft.EntityFrameworkCore;

using SampleApplication.Repositories;
using SampleApplication.Services;

using VoiceLauncher.Repositories;
using VoiceLauncher.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazoredModal();
builder.Services.AddBlazoredToast();
var config = builder.Configuration;
string? connectionString = builder.Configuration.GetConnectionString("VoiceLauncher");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
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
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
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
var app = builder.Build();

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

app.Run();

