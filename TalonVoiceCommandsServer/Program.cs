using TalonVoiceCommandsServer.Components;
using TalonVoiceCommandsServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Enable detailed errors for circuits in development
builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
{
    options.DetailedErrors = true;
});

// Configure SignalR timeouts for large data operations
builder.Services.AddSignalR(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(5); // Client must send keep-alive within 5 minutes
    options.HandshakeTimeout = TimeSpan.FromMinutes(2); // 2 minutes for handshake
    options.KeepAliveInterval = TimeSpan.FromSeconds(15); // Send keep-alive every 15 seconds
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

// Register FilterRefreshService for automatic filter refresh after import
builder.Services.AddSingleton<FilterRefreshService>();

// Register ApplicationMappingService for normalizing application names
builder.Services.AddScoped<RazorClassLibrary.Services.ApplicationMappingService>();

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

// Server-only API endpoint: accept uploaded talon file content from the browser and
// route it through the server import + persist path so filter lists are saved to
// IndexedDB/localStorage and the search filter cache is invalidated.
app.MapPost("/api/upload-talon", async (HttpRequest request) =>
{
    try
    {
        // Read JSON body
        var payload = await request.ReadFromJsonAsync<Dictionary<string, string>>();
        if (payload == null || !payload.ContainsKey("fileName") || !payload.ContainsKey("content"))
            return Results.BadRequest("Expected JSON with 'fileName' and 'content' properties");

        var fileName = payload["fileName"] ?? "uploaded.talon";
        var content = payload["content"] ?? string.Empty;

        // Resolve the data service and js runtime from the request services
        var svc = request.HttpContext.RequestServices.GetService<TalonVoiceCommandsServer.Services.ITalonVoiceCommandDataService>();
        var js = request.HttpContext.RequestServices.GetService<Microsoft.JSInterop.IJSRuntime>();

        if (svc == null)
            return Results.StatusCode(500);

        // Import the content using the server-side import (this will add to in-memory cache)
        var importedCount = await svc.ImportTalonFileContentAsync(content, fileName);

        // Attempt to persist via IndexedDB first, falling back to localStorage like other server import paths
        if (js != null)
        {
            try
            {
                await svc.SaveToIndexedDBAsync(js);
            }
            catch (Exception)
            {
                try
                {
                    await svc.SaveToLocalStorageAsync(js);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"/api/upload-talon: persistence failed: {ex.Message}");
                }
            }
        }

        // Invalidate the static filter cache so UI picks up new values
        TalonVoiceCommandsServer.Components.Pages.TalonVoiceCommandSearch.InvalidateFilterCache();

        return Results.Json(new { imported = importedCount });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"/api/upload-talon error: {ex.Message}");
        return Results.StatusCode(500);
    }
});

app.Run();

internal record ImportRequest(string? DirectoryPath);
