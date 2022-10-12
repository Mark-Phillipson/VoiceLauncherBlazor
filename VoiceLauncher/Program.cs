using DataAccessLibrary.Models;
using DataAccessLibrary.Services;
using DataAccessLibrary;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Blazored.Toast;
using Blazored.Modal;
using Microsoft.EntityFrameworkCore;
using VoiceLauncher.Services;
using DataAccessLibrary.Repositories;
using VoiceLauncher.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazoredToast();
builder.Services.AddBlazoredModal();
var config = builder.Configuration;
string connectionString = builder.Configuration.GetConnectionString("VoiceLauncher");
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
