using Blazored.Modal;
using Blazored.Toast;
using DataAccessLibrary.Models;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
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
         services.AddScoped<LanguageService>();
         services.AddScoped<ILauncherRepository, LauncherRepository>();
         services.AddScoped<ILauncherDataService, LauncherDataService>();
         services.AddScoped<ICategoryRepository, CategoryRepository>();
         services.AddScoped<ICategoryDataService, CategoryDataService>();
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

         blazorWebView1.HostPage = "wwwroot\\index.html";
         blazorWebView1.Services = services.BuildServiceProvider();
         blazorWebView1.RootComponents.Add<Index>("#app");
      }
	}
}
