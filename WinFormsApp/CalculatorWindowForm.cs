using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.Versioning;
using System.Diagnostics;

namespace WinFormsApp;

/// <summary>
/// Hosts the Calculator component in an independent Windows Form and Blazor WebView.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class CalculatorWindowForm : Form
{
    private readonly ServiceProvider _serviceProvider;
    private readonly BlazorWebView _blazorWebView;

    public CalculatorWindowForm()
    {
        Text = "Calculator";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(760, 600);
MinimumSize = new Size(560, 440);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;

        try
        {
            Icon = WindowIconFactory.CreateCalculator();
        }
        catch
        {
            // Use the default application icon if extraction fails.
        }

        var services = new ServiceCollection();
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
        _blazorWebView.RootComponents.Add<CalculatorWindowRoot>("#app");

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
                    "MarkPhillipson.VoiceLauncherBlazor.Calculator",
                    Icon,
                    "Calculator.ico");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not assign Calculator taskbar identity: {ex}");
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
