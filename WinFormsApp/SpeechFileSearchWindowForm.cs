using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace WinFormsApp;

/// <summary>
/// Hosts the migrated speech file search in an independent hybrid window.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class SpeechFileSearchWindowForm : Form
{
    private readonly ServiceProvider _serviceProvider;
    private readonly BlazorWebView _blazorWebView;
    private FolderWalkerWindowForm? _folderWalkerWindow;
    private TaskCompletionSource<string?>? _folderSelection;

    public SpeechFileSearchWindowForm()
    {
        Text = "Speech File Search";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1200, 820);
        MinimumSize = new Size(800, 600);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        ShowInTaskbar = true;

        var services = new ServiceCollection();
        services.AddSingleton<SpeechFileSearchService>();
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
        _blazorWebView.RootComponents.Add<SpeechFileSearchWindowRoot>(
            "#app",
            new Dictionary<string, object?>
            {
                ["RequestFolderAsync"] = (Func<Task<string?>>)RequestFolderAsync
            });

        var webViewHost = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(3)
        };
        webViewHost.Controls.Add(_blazorWebView);
        Controls.Add(webViewHost);
    }

    private async Task<string?> RequestFolderAsync()
    {
        if (_folderSelection is not null)
            return await _folderSelection.Task;

        var selection = new TaskCompletionSource<string?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        _folderSelection = selection;

        var window = new FolderWalkerWindowForm();
        _folderWalkerWindow = window;
        window.FolderSelected += (_, args) =>
        {
            selection.TrySetResult(args.FolderPath);
            window.Close();
        };
        window.FormClosed += (_, _) =>
        {
            if (ReferenceEquals(_folderWalkerWindow, window))
                _folderWalkerWindow = null;
            selection.TrySetResult(null);
            if (ReferenceEquals(_folderSelection, selection))
                _folderSelection = null;
        };

        window.Show(this);
        return await selection.Task;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _folderSelection?.TrySetResult(null);
        _folderWalkerWindow?.Close();
        base.OnFormClosed(e);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _serviceProvider.Dispose();
    }
}
