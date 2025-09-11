using TalonVoiceCommandsServer.Components;

var builder = WebApplication.CreateBuilder(args);

// Enable detailed errors for circuits in development
builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
{
    options.DetailedErrors = true;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register HttpClient for use by services that expect one (use NavigationManager.BaseUri when available)
builder.Services.AddScoped<System.Net.Http.HttpClient>(sp =>
{
    var navigation = sp.GetService<Microsoft.AspNetCore.Components.NavigationManager>();
    var baseUri = navigation?.BaseUri ?? (builder.Configuration["BaseAddress"] ?? "http://localhost:5269/");
    return new System.Net.Http.HttpClient { BaseAddress = new Uri(baseUri) };
});

// Register the Talon voice command data service
builder.Services.AddScoped<TalonVoiceCommandsServer.Services.ITalonVoiceCommandDataService, TalonVoiceCommandsServer.Services.TalonVoiceCommandDataService>();

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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
