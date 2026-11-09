using Blazored.Toast;
using DataAccessLibrary;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RazorClassLibrary.Services;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.Versioning;
using System.Diagnostics;

namespace WinFormsApp;

/// <summary>
/// Hosts Clipboard History in an independent Windows Form and Blazor WebView.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class ClipboardWindowForm : Form
{
    private readonly ServiceProvider _serviceProvider;
    private readonly BlazorWebView _blazorWebView;
    private readonly ClipboardWindowActivationService _activationService;

    public ClipboardWindowForm()
    {
        Text = "Clipboard History";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1100, 750);
MinimumSize = new Size(760, 480);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        ShowInTaskbar = true;
        ShowIcon = true;

        try
        {
            Icon = WindowIconFactory.CreateClipboard();
        }
        catch
        {
            // The form can use the default application icon if extraction fails.
        }

        var configuration = Program.Configuration
            ?? new ConfigurationBuilder().Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddScoped<ISqlDataAccess, DataAccessLibrary.SqlDataAccess>();
        services.AddScoped<IClipboardHistoryService>(serviceProvider =>
        {
            var dataAccess = ActivatorUtilities.CreateInstance<DataAccessLibrary.SqlDataAccess>(serviceProvider);
            dataAccess.ConnectionStringName = "ClipboardHistory";
            return new ClipboardHistoryService(dataAccess, configuration);
        });
        services.AddBlazoredToast();
        services.AddWindowsFormsBlazorWebView();
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif

        services.AddSingleton<ClipboardWindowActivationService>();
        _serviceProvider = services.BuildServiceProvider();
        _activationService = _serviceProvider.GetRequiredService<ClipboardWindowActivationService>();
        Activated += (_, _) => _activationService.NotifyActivated();

        _blazorWebView = new BlazorWebView
        {
            Dock = DockStyle.Fill,
            HostPage = Path.Combine("wwwroot", "index.html")
        };
        _blazorWebView.Services = _serviceProvider;
        _blazorWebView.RootComponents.Add<ClipboardWindowRoot>("#app");

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
                    "MarkPhillipson.VoiceLauncherBlazor.Clipboard",
                    Icon,
                    "Clipboard.ico");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not assign Clipboard taskbar identity: {ex}");
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _serviceProvider.Dispose();
    }
}
