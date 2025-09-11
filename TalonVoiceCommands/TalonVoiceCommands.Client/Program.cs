using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TalonVoiceCommands.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add HttpClient
builder.Services.AddScoped<HttpClient>();

// Register the Talon Voice Command Data Service
builder.Services.AddScoped<ITalonVoiceCommandDataService, TalonVoiceCommandDataService>();

await builder.Build().RunAsync();
