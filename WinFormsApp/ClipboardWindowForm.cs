using Blazored.Toast;
using DataAccessLibrary;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RazorClassLibrary.Services;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.Versioning;

namespace WinFormsApp;

/// <summary>
/// Hosts Clipboard History in an independent Windows Form and Blazor WebView.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class ClipboardWindowForm : Form
{
    private readonly ServiceProvider _serviceProvider;
    private readonly BlazorWebView _blazorWebView;

    public ClipboardWindowForm()
    {
        Text = "Clipboard History";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1100, 750);
        MinimumSize = new Size(760, 480);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;

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

        _serviceProvider = services.BuildServiceProvider();

        _blazorWebView = new BlazorWebView
        {
            Dock = DockStyle.Fill,
            HostPage = Path.Combine("wwwroot", "index.html")
        };
        _blazorWebView.Services = _serviceProvider;
        _blazorWebView.RootComponents.Add<ClipboardWindowRoot>("#app");

        Controls.Add(_blazorWebView);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _serviceProvider.Dispose();
    }
}
