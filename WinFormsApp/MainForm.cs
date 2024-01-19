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
		public MainForm()
		{
			InitializeComponent();

			var services = new ServiceCollection();
			//services.AddScoped(IConfiguration, Configuration > ();
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
	}
}
