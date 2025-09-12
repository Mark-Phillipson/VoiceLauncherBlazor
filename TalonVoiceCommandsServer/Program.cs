using TalonVoiceCommandsServer.Components;
using TalonVoiceCommandsServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Enable detailed errors for circuits in development
builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
{
    options.DetailedErrors = true;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Theme service to toggle light/dark using JS interop and localStorage
builder.Services.AddScoped<TalonVoiceCommandsServer.Services.ThemeService>();

// Register HttpClient for use by services that expect one (use NavigationManager.BaseUri when available)
builder.Services.AddScoped<System.Net.Http.HttpClient>(sp =>
{
    var navigation = sp.GetService<Microsoft.AspNetCore.Components.NavigationManager>();
    var baseUri = navigation?.BaseUri ?? (builder.Configuration["BaseAddress"] ?? "http://localhost:5269/");
    return new System.Net.Http.HttpClient { BaseAddress = new Uri(baseUri) };
});

// Register the Talon voice command data service
builder.Services.AddScoped<TalonVoiceCommandsServer.Services.ITalonVoiceCommandDataService, TalonVoiceCommandsServer.Services.TalonVoiceCommandDataService>();

// Register a Windows service implementation used by components to query the active window/process
builder.Services.AddSingleton<IWindowsService, WindowsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

// Serve static files from wwwroot (including _content folder for local static assets)
app.UseStaticFiles();

// Map component endpoints
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

internal record ImportRequest(string? DirectoryPath);
