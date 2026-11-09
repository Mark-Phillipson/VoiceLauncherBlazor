using Blazored.Modal;
using Blazored.Toast;
using DataAccessLibrary.Models;
using DataAccessLibrary.Repositories;
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
using VoiceLauncher.Repositories;
using VoiceLauncher.Services;

namespace WinFormsApp;

/// <summary>
/// Hosts Global Search in an independent Windows Form and Blazor WebView.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class GlobalWindowForm : Form
{
    private readonly ServiceProvider _serviceProvider;
    private readonly BlazorWebView _blazorWebView;

    public GlobalWindowForm()
    {
        Text = "Global Search";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1450, 950);
MinimumSize = new Size(950, 650);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        ShowInTaskbar = true;
        ShowIcon = true;

        try { Icon = WindowIconFactory.CreateGlobal(); }
        catch { }

        var configuration = Program.Configuration ?? new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddMemoryCache();
        services.AddSingleton<ComponentCacheService>();
        services.AddSingleton<ApplicationMappingService>();
        services.AddSingleton<IWindowsService, WindowsService>();

        var sqliteConnection = DataAccessLibrary.Configuration.DatabaseConfiguration.GetConnectionString();
        services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlite(sqliteConnection));
        services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());

        services.AddScoped<ILanguageRepository, LanguageRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICustomIntelliSenseRepository, CustomIntelliSenseRepository>();
        services.AddScoped<ILauncherRepository, LauncherRepository>();
        services.AddScoped<IValueToInsertRepository, ValueToInsertRepository>();

        services.AddScoped<ILanguageDataService, LanguageDataService>();
        services.AddScoped<ICategoryDataService, CategoryDataService>();
        services.AddScoped<ICustomIntelliSenseDataService, CustomIntelliSenseDataService>();
        services.AddScoped<ILauncherDataService, LauncherDataService>();
        services.AddScoped<IValueToInsertDataService, ValueToInsertDataService>();
        services.AddScoped<LauncherMultipleLauncherBridgeDataService>();

        services.AddBlazoredModal();
        services.AddBlazoredToast();
        services.AddHttpClient();
        services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient());
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
        _blazorWebView.RootComponents.Add<GlobalWindowRoot>("#app");

        // Host the WebView in a padded panel so the inset is applied consistently.
        var webViewHost = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(3)
        };
        webViewHost.Controls.Add(_blazorWebView);
        Controls.Add(webViewHost);
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
                    "MarkPhillipson.VoiceLauncherBlazor.Global",
                    Icon,
                    "Global.ico");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not assign Global Search taskbar identity: {ex}");
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
