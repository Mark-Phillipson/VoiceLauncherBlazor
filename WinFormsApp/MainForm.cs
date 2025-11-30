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
using Microsoft.Web.WebView2.Core;


namespace WinFormsApp
{	[SupportedOSPlatform("windows")]	public partial class MainForm : Form
	{
		private ContextMenuStrip contextMenu;
		private NotifyIcon notifyIcon;
		private bool launchSearchMode;
		
		public MainForm(bool launchSearchMode = false)
		{
			this.launchSearchMode = launchSearchMode;
			InitializeComponent();
			// Initialize NotifyIcon
			notifyIcon = new NotifyIcon
			{
				Icon = SystemIcons.Application,
				Visible = true,
				Text = "Blazor Hybrid Voice Admin"
			};
			notifyIcon.DoubleClick += NotifyIcon_DoubleClick!;
			// Initialize ContextMenu
			contextMenu = new ContextMenuStrip();
			contextMenu.Items.Add("Open", null, (s, e) => ShowMainForm());
			contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());
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
			// Windows integration service for hybrid Blazor
			services.AddSingleton<RazorClassLibrary.Services.IWindowsService, RazorClassLibrary.Services.WindowsService>();

			// Register this form for JSInterop
			services.AddSingleton<MainForm>(this);

			// Add database context
			if (Program.Configuration != null)
			{
				var connectionString = Program.Configuration.GetConnectionString("DefaultConnection");
				services.AddDbContextFactory<ApplicationDbContext>(options =>
					options.UseSqlServer(connectionString));
			}

			// Add AutoMapper
			services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());			// Add repositories
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
			services.AddScoped<IQuickPromptDataService, QuickPromptDataService>();			// Add additional services
			services.AddScoped<GeneralLookupService>();
			services.AddScoped<AdditionalCommandService>();
			services.AddScoped<CustomIntellisenseService>();
			services.AddScoped<LauncherMultipleLauncherBridgeDataService>();
			services.AddScoped<LauncherService>();
			// Register the talon data service under its interface so Razor components can resolve it
			services.AddScoped<DataAccessLibrary.Services.ITalonVoiceCommandDataService, TalonVoiceCommandDataService>();// Add UI services
			services.AddBlazoredModal();
			services.AddBlazoredToast();

			// Add Blazor services last
			services.AddWindowsFormsBlazorWebView();
#if DEBUG
			services.AddBlazorWebViewDeveloperTools();
#endif

			// Configure WebView2 environment with user data folder
			// Removed manual WebView2 initialization to avoid double initialization and environment conflict
			// Subscribe to CoreWebView2InitializationCompleted to set tracking prevention and storage settings
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
			blazorWebView1.Services = services.BuildServiceProvider();

			blazorWebView1.RootComponents.Add<Index>("#app",
			 new Dictionary<string, object?>
			 {
				  {"CloseWindowCallback", new EventCallback(null, ()=>{ Application.Exit(); }) },
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
				// Set up a dedicated user data folder for WebView2
				var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VoiceAdminWebView2");
				Directory.CreateDirectory(userDataFolder);

				var env = await CoreWebView2Environment.CreateAsync(
					userDataFolder: userDataFolder);

				var webview = blazorWebView1?.WebView;
				if (webview != null)
				{
					await webview.EnsureCoreWebView2Async(env);

					// Configure settings to disable tracking prevention and allow storage
					if (webview.CoreWebView2 != null)
					{
						var settings = webview.CoreWebView2.Settings;
						settings.IsGeneralAutofillEnabled = true;
						settings.AreDefaultContextMenusEnabled = true;
						settings.IsStatusBarEnabled = false;

						// Configure profile to allow storage and cookies
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

					// If CoreWebView2 is already created by the host, use it; otherwise wait for initialization
					if (webview.CoreWebView2 != null)
					{
						_ = InitializeCoreWebView2Async(webview.CoreWebView2);
					}
					else
					{
						// Subscribe once to initialization completed
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
					// Open DevTools for interactive inspection if available
					try { core.OpenDevToolsWindow(); } catch { }

					// Receive postMessage calls from the page
					core.WebMessageReceived += (s, e) =>
					{
						try
						{
							var msg = e.TryGetWebMessageAsString();
							Debug.WriteLine("From JS via postMessage: " + msg);
						}
						catch { }
					};

					// Inject script to forward console.* calls to host via chrome.webview.postMessage
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
			// Minimize to system tray instead of closing
			// e.Cancel = true;
			// this.Hide();
			// notifyIcon.ShowBalloonTip(1000, "Blazor Hybrid App", "The application is still running in the system tray.", ToolTipIcon.Info);
		}

		private void MainForm_Resize(object sender, EventArgs e)
		{
			// if (this.WindowState == FormWindowState.Minimized)
			// {
			// 	this.Hide();
			// 	notifyIcon.ShowBalloonTip(1000, "Blazor Hybrid App", "The application is still running in the system tray.", ToolTipIcon.Info);
			// }
		}

		private void NotifyIcon_DoubleClick(object sender, EventArgs e)
		{
			this.Show();
			this.WindowState = FormWindowState.Normal;
		}
		private void ShowMainForm()
		{
			this.Show();
			this.WindowState = FormWindowState.Normal;
		}		private void ExitApplication()
		{
			notifyIcon.Visible = false;
			Application.Exit();
		}
	}
}
