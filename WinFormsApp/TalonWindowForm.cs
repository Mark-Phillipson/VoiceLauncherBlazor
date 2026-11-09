using DataAccessLibrary.Models;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RazorClassLibrary.Services;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace WinFormsApp;

/// <summary>
/// Hosts Talon Voice Command Search in an independent Windows Form and Blazor WebView.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class TalonWindowForm : Form
{
    private readonly ServiceProvider _serviceProvider;
    private readonly BlazorWebView _blazorWebView;

    public TalonWindowForm(string? searchTerm = null)
    {
        Text = "Talon Voice Command Search";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1450, 950);
        Padding = new Padding(3);
        MinimumSize = new Size(950, 650);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        ShowInTaskbar = true;
        ShowIcon = true;

        try
        {
            Icon = WindowIconFactory.CreateTalon();
        }
        catch
        {
            // Use the default application icon if extraction fails.
        }

        var configuration = Program.Configuration ?? new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddMemoryCache();
        services.AddSingleton<ApplicationMappingService>();
        services.AddSingleton<IWindowsService, WindowsService>();

        var sqliteConnection = DataAccessLibrary.Configuration.DatabaseConfiguration.GetConnectionString();
        services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlite(sqliteConnection));
        services.AddScoped<ITalonVoiceCommandDataService, TalonVoiceCommandDataService>();
        services.AddWindowsFormsBlazorWebView();
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif

        _serviceProvider = services.BuildServiceProvider();

        _blazorWebView = new BlazorWebView
        {
            Dock = DockStyle.Fill,
            HostPage = Path.Combine("wwwroot", "index.html")
        };
        _blazorWebView.Services = _serviceProvider;
        _blazorWebView.RootComponents.Add<TalonWindowRoot>(
            "#app",
            new Dictionary<string, object?>
            {
                { "SearchTerm", searchTerm ?? string.Empty }
            });

        Controls.Add(_blazorWebView);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        try
        {
            if (Icon != null)
            {
                TaskbarIdentity.SetWindowAppUserModelId(
                    Handle,
                    "MarkPhillipson.VoiceLauncherBlazor.Talon",
                    Icon,
                    "Talon.ico");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not assign Talon taskbar identity: {ex}");
        }
    }

    protected override void Dispose(bool disposing)
    {
        // Let the Form dispose the BlazorWebView before disposing its service provider.
        base.Dispose(disposing);

        if (disposing)
            _serviceProvider.Dispose();
    }
}