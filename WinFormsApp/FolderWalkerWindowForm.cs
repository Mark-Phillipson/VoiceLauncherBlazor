using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace WinFormsApp;

/// <summary>
/// Hosts the reusable folder search view in its own Windows Forms window.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class FolderWalkerWindowForm : Form
{
    private readonly ServiceProvider _serviceProvider;
    private readonly BlazorWebView _blazorWebView;

    public FolderWalkerWindowForm()
    {
        Text = "Folder Walker";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1100, 760);
        MinimumSize = new Size(760, 540);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        ShowInTaskbar = true;

        var services = new ServiceCollection();
        services.AddSingleton(FolderWalkerService.Shared);
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
        _blazorWebView.RootComponents.Add<FolderWalkerWindowRoot>(
            "#app",
            new Dictionary<string, object?>
            {
                ["FolderSelected"] = EventCallback.Factory.Create<string>(this, RaiseFolderSelected)
            });

        var webViewHost = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(3)
        };
        webViewHost.Controls.Add(_blazorWebView);
        Controls.Add(webViewHost);
    }

    public event EventHandler<FolderSelectionEventArgs>? FolderSelected;

    private void RaiseFolderSelected(string folderPath) =>
        FolderSelected?.Invoke(this, new FolderSelectionEventArgs(folderPath));

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _serviceProvider.Dispose();
    }
}

internal sealed class FolderSelectionEventArgs(string folderPath) : EventArgs
{
    public string FolderPath { get; } = folderPath;
}
