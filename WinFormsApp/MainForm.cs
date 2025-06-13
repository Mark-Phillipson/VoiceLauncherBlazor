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


namespace WinFormsApp
{	[SupportedOSPlatform("windows")]
	public partial class MainForm : Form
	{
		private ContextMenuStrip contextMenu;
		private NotifyIcon notifyIcon;
		public MainForm()
		{
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
		}		private void InitializeServices()
		{
			var services = new ServiceCollection();

			// Add core services first
			if (Program.Configuration != null)
			{
				services.AddSingleton<IConfiguration>(Program.Configuration);
			}
			services.AddMemoryCache();
			services.AddSingleton<ComponentCacheService>();
			
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
			services.AddScoped<TalonVoiceCommandDataService>();// Add UI services
			services.AddBlazoredModal();
			services.AddBlazoredToast();

			// Add Blazor services last
			services.AddWindowsFormsBlazorWebView();
#if DEBUG
			services.AddBlazorWebViewDeveloperTools();
#endif
			blazorWebView1.HostPage = "wwwroot\\index.html";
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
