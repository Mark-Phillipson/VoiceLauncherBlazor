using Blazored.Modal;
using Blazored.Toast;
using DataAccessLibrary.Models;
using DataAccessLibrary.Repositories;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RazorClassLibrary;
using VoiceLauncher.Repositories;
using VoiceLauncher.Services;

namespace WinFormsApp
{
	public partial class MainForm : Form
	{
		private ContextMenuStrip contextMenu;
		private NotifyIcon notifyIcon;
		public MainForm()
		{
			InitializeComponent();
			this.Text = "Blazor Hybrid Voice Admin"; // Set the desired title here
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
			var services = new ServiceCollection();
			// Blazor WebView initialization
			blazorWebView1.HostPage = "wwwroot\\index.html";
			blazorWebView1.Services = services.BuildServiceProvider();
			// blazorWebView1.RootComponents.Add<Index>("#app",
			// 	new Dictionary<string, object?>
			// 	{
			// 		{"CloseWindowCallback", new EventCallback(null, ()=>{ Application.Exit(); }) },
			// 		{"MaximizeWindowCallback", new EventCallback(null, ()=>{ WindowState = FormWindowState.Maximized; }) },
			// 		{"MinimizeWindowCallback", new EventCallback(null, ()=>{ WindowState = FormWindowState.Minimized; }) },
			// 		{"RestoreWindowCallback", new EventCallback(null, ()=>{ WindowState = FormWindowState.Normal; }) },
			// 		{"SetTitleCallback", new EventCallback<string>(null, ( string title)=>{ Text = title; })}
			// 	});


			// Register IConfigurationRoot
			var configurationBuilder = new ConfigurationBuilder()
				.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
			IConfigurationRoot configurationRoot = configurationBuilder.Build();
			services.AddSingleton<IConfigurationRoot>(configurationRoot);
			services.AddSingleton<IConfiguration>(configurationRoot);

			services.AddWindowsFormsBlazorWebView();
#if DEBUG
			services.AddBlazorWebViewDeveloperTools();
#endif
			services.AddScoped<LanguageService>();
			services.AddScoped<ILauncherRepository, LauncherRepository>();
			services.AddScoped<ILauncherDataService, LauncherDataService>();
			services.AddScoped<ICategoryRepository, CategoryRepository>();
			services.AddScoped<ICategoryDataService, CategoryDataService>();
			services.AddScoped<ILanguageDataService, LanguageDataService>();
			services.AddScoped<ILanguageRepository, LanguageRepository>();
			services.AddBlazoredModal();
			services.AddBlazoredToast();
			string connectionString = "Data Source=Localhost;Initial Catalog=VoiceLauncher;Integrated Security=True;Connect Timeout=120;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
			services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
			services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
			services.AddScoped<LanguageService>();
			services.AddScoped<ICustomIntelliSenseRepository, CustomIntelliSenseRepository>();
			services.AddScoped<ICustomIntelliSenseDataService, CustomIntelliSenseDataService>();
			services.AddScoped<GeneralLookupService>();
			services.AddScoped<AdditionalCommandService>();
			services.AddScoped<CustomIntellisenseService>();
			services.AddScoped<CategoryService>();
			services.AddScoped<LauncherMultipleLauncherBridgeDataService>();
			services.AddScoped<LauncherDataService>();

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
			e.Cancel = true;
			this.Hide();
			notifyIcon.ShowBalloonTip(1000, "Blazor Hybrid App", "The application is still running in the system tray.", ToolTipIcon.Info);
		}

		private void MainForm_Resize(object sender, EventArgs e)
		{
			if (this.WindowState == FormWindowState.Minimized)
			{
				this.Hide();
				notifyIcon.ShowBalloonTip(1000, "Blazor Hybrid App", "The application is still running in the system tray.", ToolTipIcon.Info);
			}
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
        }
		private void ExitApplication()
		{
			notifyIcon.Visible = false;
			Application.Exit();
		}
	}
}
