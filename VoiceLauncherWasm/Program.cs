using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VoiceLauncherWasm;
using VoiceLauncherWasm.Services;
using VoiceLauncherWasm.Services.TestStubs;
using SharedContracts.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add application services
builder.Services.AddScoped<IIndexedDBService, IndexedDBService>();
builder.Services.AddScoped<TalonParserService>();
// Register the concrete IndexedDB-backed talon data service (used by the shared component via DI in RazorClassLibrary)
// Register a mock repository for the local Talon search page
builder.Services.AddScoped<SharedContracts.Services.ITalonVoiceCommandRepository, VoiceLauncherWasm.Services.MockTalonRepository>();
builder.Services.AddScoped<IWindowsService, WindowsServiceStub>();

await builder.Build().RunAsync();
