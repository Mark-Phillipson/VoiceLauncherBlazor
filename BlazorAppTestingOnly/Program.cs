using BlazorAppTestingOnly.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
	.AddHubOptions(options =>
	{
		options.ClientTimeoutInterval = TimeSpan.FromMinutes(30); // 30 min
		options.HandshakeTimeout = TimeSpan.FromSeconds(30);
	});
builder.Services.AddIdleCircuitHandler(options =>
	options.IdleTimeout = TimeSpan.FromMinutes(30)); // 30 min
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<TalonVoiceCommandDataService>();

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
