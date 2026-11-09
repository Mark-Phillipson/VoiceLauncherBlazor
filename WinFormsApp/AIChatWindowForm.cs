using Blazored.Modal;
using Blazored.Toast;
using DataAccessLibrary.Models;
using DataAccessLibrary.Repositories;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RazorClassLibrary.Pages;
using SampleApplication.Repositories;
using SampleApplication.Services;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.Versioning;
using System.Diagnostics;

namespace WinFormsApp;

/// <summary>
/// Hosts AI Chat in an independent Windows Form and Blazor WebView.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class AIChatWindowForm : Form
{
    private readonly ServiceProvider _serviceProvider;
    private readonly BlazorWebView _blazorWebView;

    public AIChatWindowForm()
    {
        Text = "AI Chat";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1150, 850);
MinimumSize = new Size(760, 560);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;

        try
        {
            Icon = WindowIconFactory.CreateChat();
        }
        catch
        {
            // Use the default application icon if extraction fails.
        }

        var configuration = Program.Configuration ?? new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddMemoryCache();

        var sqliteConnection = DataAccessLibrary.Configuration.DatabaseConfiguration.GetConnectionString();
        services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlite(sqliteConnection));
        services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());

        // AI Chat loads its predefined prompts and quick prompts from the application database.
        services.AddScoped<IPromptRepository, PromptRepository>();
        services.AddScoped<IQuickPromptRepository, QuickPromptRepository>();
        services.AddScoped<IPromptDataService, PromptDataService>();
        services.AddScoped<IQuickPromptDataService, QuickPromptDataService>();

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
        _blazorWebView.RootComponents.Add<AIChatWindowRoot>("#app");

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
                    "MarkPhillipson.VoiceLauncherBlazor.AIChat",
                    Icon,
                    "AIChat.ico");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not assign AIChat taskbar identity: {ex}");
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
