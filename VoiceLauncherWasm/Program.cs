using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VoiceLauncherWasm;
using VoiceLauncherWasm.Services.IndexedDB;
using VoiceLauncherWasm.Services;
using VoiceLauncherWasm.Repositories;
using Blazored.LocalStorage;
using Blazored.Modal;
using Blazored.Toast;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add Blazored services
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredModal();
builder.Services.AddBlazoredToast();

// Add IndexedDB services
builder.Services.AddScoped<IIndexedDBService, IndexedDBService>();

// Add repositories
builder.Services.AddScoped<ITalonVoiceCommandRepository, TalonVoiceCommandRepository>();
builder.Services.AddScoped<ITalonListRepository, TalonListRepository>();

// Add application services
builder.Services.AddScoped<TalonVoiceCommandService>();
builder.Services.AddScoped<TalonListService>();

var host = builder.Build();

// Initialize IndexedDB
var indexedDBService = host.Services.GetRequiredService<IIndexedDBService>();
await indexedDBService.InitializeAsync();

await host.RunAsync();
