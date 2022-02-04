using DataAccessLibrary.Models;
using DataAccessLibrary.Services;
using DataAccessLibrary;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using VoiceLauncher.Data;
using Blazored.Toast;
using Blazored.Modal;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazoredToast();
builder.Services.AddBlazoredModal();
string connectionString = "Data Source=DESKTOP-UROO8T1;Initial Catalog=VoiceLauncher;Integrated Security=True;Connect Timeout=120;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<CategoryService>();
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
builder.Services.AddSingleton<NotifierService>();


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
