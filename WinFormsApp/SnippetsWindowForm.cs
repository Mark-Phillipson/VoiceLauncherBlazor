using Blazored.Modal;
using Blazored.Toast;
using DataAccessLibrary.Models;
using DataAccessLibrary.Repositories;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RazorClassLibrary.Services;
using SampleApplication.Repositories;
using SampleApplication.Services;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.Versioning;
using System.Diagnostics;
using VoiceLauncher.Repositories;
using VoiceLauncher.Services;

namespace WinFormsApp;

/// <summary>
/// Hosts the Snippets table in an independent Windows Form and Blazor WebView.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class SnippetsWindowForm : Form
{
    private readonly ServiceProvider _serviceProvider;
    private readonly BlazorWebView _blazorWebView;

    public SnippetsWindowForm(string? launchArguments = null)
    {
        Text = "Snippets";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1250, 850);
        Padding = new Padding(3);
        MinimumSize = new Size(850, 600);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;

        try
        {
            Icon = WindowIconFactory.CreateSnippets();
        }
        catch
        {
            // Use the default application icon if extraction fails.
        }

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

        services.AddScoped<ILauncherRepository, LauncherRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ILanguageRepository, LanguageRepository>();
        services.AddScoped<ICustomIntelliSenseRepository, CustomIntelliSenseRepository>();
        services.AddScoped<IPromptRepository, PromptRepository>();
        services.AddScoped<IQuickPromptRepository, QuickPromptRepository>();

        services.AddScoped<ComputerService>();
        services.AddScoped<LanguageService>();
        services.AddScoped<ILanguageDataService, LanguageDataService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<ICategoryDataService, CategoryDataService>();
        services.AddScoped<ILauncherDataService, LauncherDataService>();
        services.AddScoped<ICustomIntelliSenseDataService, CustomIntelliSenseDataService>();
        services.AddScoped<IPromptDataService, PromptDataService>();
        services.AddScoped<IQuickPromptDataService, QuickPromptDataService>();
        services.AddScoped<GeneralLookupService>();
        services.AddScoped<AdditionalCommandService>();
        services.AddScoped<CustomIntellisenseService>();
        services.AddScoped<LauncherMultipleLauncherBridgeDataService>();
        services.AddScoped<LauncherService>();
        services.AddScoped<DataAccessLibrary.Services.IValueToInsertDataService, ValueToInsertDataService>();
        services.AddScoped<IValueToInsertRepository, ValueToInsertRepository>();
        services.AddScoped<DataAccessLibrary.Services.ITalonVoiceCommandDataService, TalonVoiceCommandDataService>();

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
        _blazorWebView.RootComponents.Add<SnippetsWindowRoot>(
            "#app",
            new Dictionary<string, object?>
            {
                { "LaunchArguments", launchArguments ?? string.Empty },
                { "CloseWindowCallback", new EventCallback(null, Hide) },
                { "MaximizeWindowCallback", new EventCallback(null, () => WindowState = FormWindowState.Maximized) }
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
                    "MarkPhillipson.VoiceLauncherBlazor.Snippets",
                    Icon,
                    "Snippets.ico");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not assign Snippets taskbar identity: {ex}");
        }
    }

    protected override void Dispose(bool disposing)
    {
        // Form.Dispose disposes child controls, including the BlazorWebView.
        // Dispose the provider afterwards so the WebView can still resolve services
        // while it is shutting down. Do not dispose the WebView separately here,
        // otherwise the base Form disposal can dispose it a second time.
        base.Dispose(disposing);

        if (disposing)
            _serviceProvider.Dispose();
    }
}
