using Blazored.Modal;
using Blazored.Toast;
using DataAccessLibrary;
using DataAccessLibrary.Models;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;

namespace VoiceLauncherBlazor
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddScoped<HttpClient>();
			services.AddBlazoredToast();
			services.AddBlazoredModal();
			services.AddRazorPages();
			services.AddServerSideBlazor();
			services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
			services.AddScoped<CategoryService>();
			services.AddScoped<AdditionalCommandService>();
			services.AddScoped<LanguageService>();
			services.AddScoped<LauncherService>();
			services.AddScoped<ComputerService>();
			services.AddScoped<CustomIntellisenseService>();
			services.AddScoped<GeneralLookupService>();
			services.AddScoped<ISqlDataAccess, SqlDataAccess>();
			services.AddScoped<ITodoData, TodoData>();
			services.AddScoped<AppointmentService>();
			services.AddScoped<VisualStudioCommandService>();
			services.AddScoped<CommandSetService>();
			services.AddSingleton<NotifierService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			//Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjE3ODA5QDMxMzcyZTM0MmUzME44MU0yUkh5aWNJakV0Y2VRTTNrQzFBNTI5cUt1L2dBUFBYVDRRa1RPejA9");
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
			});
		}
	}
}
