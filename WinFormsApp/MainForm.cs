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
using RazorClassLibrary;
using RazorClassLibrary.Services;
using SampleApplication.Repositories;
using SampleApplication.Services;
using VoiceLauncher.Repositories;
using VoiceLauncher.Services;
using System.Runtime.Versioning;
using System.Diagnostics;
using System.IO;
using Microsoft.Web.WebView2.Core;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using System.IO.Pipes;
using System.Text;

namespace WinFormsApp
{
    [SupportedOSPlatform("windows")]
    public partial class MainForm : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private bool _reallyExit = false;
        private ContextMenuStrip contextMenu;
        private NotifyIcon notifyIcon;
        private bool launchSearchMode;
        private CancellationTokenSource _pipeServerCts = new();

        public static event EventHandler<LaunchArgumentsEventArgs>? LaunchArgumentsReceived;

        public MainForm(bool launchSearchMode = false)
        {
            this.launchSearchMode = launchSearchMode;
            InitializeComponent();

            // Initialize NotifyIcon - prefer the embedded app icon resource if available
            Icon? trayIcon = null;

            // Try embedded resource first (from project EmbeddedResource)
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                using var rs = asm.GetManifestResourceStream("WinFormsApp.TempestRising.ico");
                if (rs != null)
                {
                    trayIcon = new Icon(rs);
                    // also set Form icon so designer and Alt-Tab show it
                    try { this.Icon = (Icon)trayIcon.Clone(); } catch { }
                }
            }
            catch { }

            // Fall back to form designer icon then exe-associated icon
            if (trayIcon == null)
            {
                try { trayIcon = this.Icon; } catch { }
            }
            if (trayIcon == null)
            {
                try { trayIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
            }

            notifyIcon = new NotifyIcon
            {
                Icon = trayIcon ?? SystemIcons.Application,
                Visible = true,
                Text = "Blazor Hybrid Voice Admin"
            };
            notifyIcon.MouseClick += NotifyIcon_MouseClick!;

            // Initialize ContextMenu with icons (touch/eye-tracking friendly)
            contextMenu = new ContextMenuStrip();
            // Show images inline so captions line up predictably with icons
            contextMenu.ShowImageMargin = false;
            contextMenu.ImageScalingSize = new Size(24, 24);
            contextMenu.Font = new Font("Segoe UI", 14F, FontStyle.Regular);
            contextMenu.RenderMode = ToolStripRenderMode.System;

            // Local helper to scale an Icon into a transparent Bitmap of the requested size
            Bitmap ScaleIconToBitmap(Icon icon, Size size)
            {
                var bmp = new Bitmap(size.Width, size.Height);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.DrawIcon(icon, new Rectangle(0, 0, size.Width, size.Height));
                }
                return bmp;
            }

            var imgSize = contextMenu.ImageScalingSize;
            var openIcon = (Icon?)(trayIcon) ?? SystemIcons.Application;
            Bitmap openBmp = ScaleIconToBitmap(openIcon, imgSize);
            Bitmap exitBmp = ScaleIconToBitmap(SystemIcons.Error, imgSize);

            // Helper to compute a touch-friendly width so text is visible
            int ComputeMenuItemWidth(string text, Font font, int imgWidth, Padding pad)
            {
                var textSize = TextRenderer.MeasureText(text, font);
                // extra space for margins and comfortable touch target
                return Math.Max(240, textSize.Width + imgWidth + pad.Left + pad.Right + 48);
            }

            var defaultPadding = new Padding(12, 8, 12, 8);
            int openWidth = ComputeMenuItemWidth("Open", contextMenu.Font, imgSize.Width, defaultPadding);
            int exitWidth = ComputeMenuItemWidth("Exit", contextMenu.Font, imgSize.Width, defaultPadding);
            int menuWidth = Math.Max(openWidth, exitWidth);

            var openItem = new ToolStripMenuItem("Open", openBmp, (s, e) => ShowMainForm())
            {
                Padding = defaultPadding,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                AutoSize = false,
                Size = new Size(menuWidth, 48),
                TextAlign = ContentAlignment.MiddleLeft
            };
            openItem.ImageScaling = ToolStripItemImageScaling.None;
            openItem.ImageAlign = ContentAlignment.MiddleLeft;

            var exitItem = new ToolStripMenuItem("Exit", exitBmp, (s, e) => ExitApplication())
            {
                Padding = defaultPadding,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                AutoSize = false,
                Size = new Size(menuWidth, 48),
                TextAlign = ContentAlignment.MiddleLeft
            };
            exitItem.ImageScaling = ToolStripItemImageScaling.None;
            exitItem.ImageAlign = ContentAlignment.MiddleLeft;

            contextMenu.Items.Add(openItem);
            contextMenu.Items.Add(exitItem);
            notifyIcon.ContextMenuStrip = contextMenu;

            // Handle Form events
            this.FormClosing += MainForm_FormClosing!;
            this.Resize += MainForm_Resize!;
            InitializeServices();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var screen = Screen.PrimaryScreen;
            if (screen != null)
            {
                var workingArea = screen.WorkingArea;
                this.Width = workingArea.Width / 2;
                this.Height = workingArea.Height;
                this.Location = new Point(workingArea.Right - this.Width, workingArea.Top);
            }

            // Start listening for launch arguments from other instances
            StartNamedPipeServer();
        }

        private void InitializeServices()
        {
            var services = new ServiceCollection();

            // Add core services first
            if (Program.Configuration != null)
            {
                services.AddSingleton<IConfiguration>(Program.Configuration);
            }
            services.AddMemoryCache();
            services.AddSingleton<ComponentCacheService>();
            services.AddSingleton<ApplicationMappingService>();
            // Windows integration service for hybrid Blazor
            services.AddSingleton<RazorClassLibrary.Services.IWindowsService, RazorClassLibrary.Services.WindowsService>();

            // Register this form for JSInterop
            services.AddSingleton<MainForm>(this);

            // Add database context
            var sqliteConnection = DataAccessLibrary.Configuration.DatabaseConfiguration.GetConnectionString();
            services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseSqlite(sqliteConnection));

            // Add AutoMapper
            services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());
            // Add repositories
            services.AddScoped<ILauncherRepository, LauncherRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ILanguageRepository, LanguageRepository>();
            services.AddScoped<ICustomIntelliSenseRepository, CustomIntelliSenseRepository>();
            services.AddScoped<IPromptRepository, PromptRepository>();
            services.AddScoped<IQuickPromptRepository, QuickPromptRepository>();

            // Add core services
            services.AddScoped<ComputerService>();
            services.AddScoped<LanguageService>();
            services.AddScoped<ILanguageDataService, LanguageDataService>();
            services.AddScoped<CategoryService>();
            services.AddScoped<ICategoryDataService, CategoryDataService>();
            services.AddScoped<ILauncherDataService, LauncherDataService>();
            services.AddScoped<ICustomIntelliSenseDataService, CustomIntelliSenseDataService>();
            services.AddScoped<IPromptDataService, PromptDataService>();
            services.AddScoped<IQuickPromptDataService, QuickPromptDataService>();
            // Add additional services
            services.AddScoped<GeneralLookupService>();
            services.AddScoped<AdditionalCommandService>();
            services.AddScoped<CustomIntellisenseService>();
            services.AddScoped<LauncherMultipleLauncherBridgeDataService>();
            services.AddScoped<LauncherService>();
            services.AddScoped<DataAccessLibrary.Services.ITalonVoiceCommandDataService, TalonVoiceCommandDataService>();
            services.AddBlazoredModal();
            services.AddBlazoredToast();

            // Add Blazor services last
            services.AddWindowsFormsBlazorWebView();
#if DEBUG
            services.AddBlazorWebViewDeveloperTools();
#endif

            // Register HttpClient and IHttpClientFactory
            services.AddHttpClient();
            services.AddScoped(sp => sp.GetRequiredService<System.Net.Http.IHttpClientFactory>().CreateClient());

            var webview = blazorWebView1?.WebView;
            if (webview != null)
            {
                var currentWebView = webview;
                currentWebView.CoreWebView2InitializationCompleted += (s, e) =>
                {
                    if (e.IsSuccess && currentWebView.CoreWebView2 != null)
                    {
                        var settings = currentWebView.CoreWebView2.Settings;
                        settings.IsGeneralAutofillEnabled = true;
                        settings.AreDefaultContextMenusEnabled = true;
                        settings.IsStatusBarEnabled = false;

                        var profile = webview.CoreWebView2.Profile;
                        profile.PreferredTrackingPreventionLevel = CoreWebView2TrackingPreventionLevel.None;
                    }
                };
            }

            blazorWebView1!.HostPage = "wwwroot\\index.html";
            var serviceProvider = services.BuildServiceProvider();

            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var factory = scope.ServiceProvider.GetRequiredService<Microsoft.EntityFrameworkCore.IDbContextFactory<DataAccessLibrary.Models.ApplicationDbContext>>();
                    using (var ctx = factory.CreateDbContext())
                    {
                        var connStr = ctx.Database.GetDbConnection().ConnectionString;
                        Debug.WriteLine($"SQLite connection string: {connStr}");
                        var dbPath = DataAccessLibrary.Configuration.DatabaseConfiguration.GetDatabasePath();
                        Debug.WriteLine($"SQLite DB path: {dbPath}, exists: {File.Exists(dbPath)}");

                        ctx.Database.Migrate();

                        try
                        {
                            var launcherCount = ctx.Launcher.Count();
                            Debug.WriteLine($"Launcher rows: {launcherCount}");
                        }
                        catch (Exception exCount)
                        {
                            Debug.WriteLine($"Error counting rows: {exCount.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Database diagnostic/migration error: {ex.Message}");
            }

            blazorWebView1.Services = serviceProvider;

            blazorWebView1.RootComponents.Add<Index>("#app",
             new Dictionary<string, object?>
             {
                  {"CloseWindowCallback", new EventCallback(null, ()=>{ this.Hide(); }) },
                  {"MaximizeWindowCallback", new EventCallback(null, ()=>{ WindowState = FormWindowState.Maximized; }) },
                  {"MinimizeWindowCallback", new EventCallback(null, ()=>{ WindowState = FormWindowState.Minimized; }) },
                  {"RestoreWindowCallback", new EventCallback(null, ()=>{ WindowState = FormWindowState.Normal; }) },
                  {"SetTitleCallback", new EventCallback<string>(null, ( string title)=>{ Text = title; })}
             });

            // Setup WebView2 logging, DevTools and console forwarding
#if DEBUG
            _ = SetupWebViewLoggingAsync();
#endif
        }

        private async Task ConfigureWebView2EnvironmentAsync()
        {
            try
            {
                var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VoiceAdminWebView2");
                Directory.CreateDirectory(userDataFolder);

                var env = await CoreWebView2Environment.CreateAsync(
                    userDataFolder: userDataFolder);

                var webview = blazorWebView1?.WebView;
                if (webview != null)
                {
                    await webview.EnsureCoreWebView2Async(env);

                    if (webview.CoreWebView2 != null)
                    {
                        var settings = webview.CoreWebView2.Settings;
                        settings.IsGeneralAutofillEnabled = true;
                        settings.AreDefaultContextMenusEnabled = true;
                        settings.IsStatusBarEnabled = false;

                        var profile = webview.CoreWebView2.Profile;
                        profile.PreferredTrackingPreventionLevel = CoreWebView2TrackingPreventionLevel.None;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ConfigureWebView2EnvironmentAsync error: {ex.Message}");
            }
        }

        private Task SetupWebViewLoggingAsync()
        {
            try
            {
                var webview = blazorWebView1?.WebView;
                if (webview == null)
                {
                    Debug.WriteLine("WebView control not available for logging setup");
                    return Task.CompletedTask;
                }

                if (webview.CoreWebView2 != null)
                {
                    _ = InitializeCoreWebView2Async(webview.CoreWebView2);
                }
                else
                {
                    void Handler(object? s, CoreWebView2InitializationCompletedEventArgs e)
                    {
                        try
                        {
                            webview.CoreWebView2InitializationCompleted -= Handler;
                            if (e.IsSuccess && webview.CoreWebView2 != null)
                            {
                                _ = InitializeCoreWebView2Async(webview.CoreWebView2);
                            }
                            else
                            {
                                Debug.WriteLine("CoreWebView2 initialization failed or was not successful.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("CoreWebView2 Initialization handler error: " + ex.Message);
                        }
                    }

                    webview.CoreWebView2InitializationCompleted += Handler;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SetupWebViewLoggingAsync error: " + ex.Message);
            }
            return Task.CompletedTask;
        }

        private async Task InitializeCoreWebView2Async(CoreWebView2 core)
        {
            try
            {
                try { core.OpenDevToolsWindow(); } catch { }

                core.WebMessageReceived += (s, e) =>
                {
                    try
                    {
                        var msg = e.TryGetWebMessageAsString();
                        Debug.WriteLine("From JS via postMessage: " + msg);
                    }
                    catch { }
                };

                var script = @"(function(){
      const origLog = console.log;
      const origError = console.error;
      const origWarn = console.warn;
      function post(level, args){
        try { window.chrome.webview.postMessage(JSON.stringify({ level: level, args: args })); } catch(e){}
      }
      console.log = function(...args){ post('log', args); origLog.apply(console, args); };
      console.error = function(...args){ post('error', args); origError.apply(console, args); };
      console.warn = function(...args){ post('warn', args); origWarn.apply(console, args); };
    })();";

                try
                {
                    await core.AddScriptToExecuteOnDocumentCreatedAsync(script);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to inject console-forwarding script: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitializeCoreWebView2Async error: " + ex.Message);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_reallyExit)
            {
                e.Cancel = true;
                this.Hide();
            }
            // otherwise allow the form to close and the process to exit
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            // if (this.WindowState == FormWindowState.Minimized)
            // {
            //     this.Hide();
            //     notifyIcon.ShowBalloonTip(1000, "Blazor Hybrid App", "The application is still running in the system tray.", ToolTipIcon.Info);
            // }
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            // Only activate on left-click to avoid interfering with right-click menu
            if (e.Button == MouseButtons.Left)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
                this.Focus();
                this.Activate();
            }
        }

        private void ShowMainForm()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Activate();
            SetForegroundWindow(this.Handle);
        }

        private void ExitApplication()
        {
            notifyIcon.Visible = false;
            _reallyExit = true;
            Application.Exit();
        }

        /// <summary>
        /// Starts listening for launch arguments on a named pipe.
        /// Called after the form is fully loaded.
        /// </summary>
        public void StartNamedPipeServer()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    while (!_pipeServerCts.Token.IsCancellationRequested)
                    {
                        using var server = new NamedPipeServerStream(
                            "VoiceLauncherBlazor_LaunchArgs",
                            PipeDirection.In,
                            NamedPipeServerStream.MaxAllowedServerInstances,
                            PipeTransmissionMode.Message);

                        await server.WaitForConnectionAsync(_pipeServerCts.Token);

                        using var reader = new StreamReader(server, Encoding.UTF8);
                        string? message = reader.ReadLine();

                        if (!string.IsNullOrEmpty(message))
                        {
                            Debug.WriteLine($"Received launch args from pipe: {message}");
                            Invoke(() =>
                            {
                                this.Show();
                                this.WindowState = FormWindowState.Normal;
                                this.BringToFront();
                                this.Activate();
                                SetForegroundWindow(this.Handle);

                                // Raise event so Index component can handle the category
                                LaunchArgumentsReceived?.Invoke(this, new LaunchArgumentsEventArgs { Arguments = message });
                            });
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when shutting down
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Named pipe server error: {ex.Message}");
                }
            }, _pipeServerCts.Token);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _pipeServerCts.Cancel();
            base.OnFormClosing(e);
        }
    }

    /// <summary>
    /// Event args for when launch arguments are received from another instance.
    /// </summary>
    public class LaunchArgumentsEventArgs : EventArgs
    {
        public string? Arguments { get; set; }
    }
}

