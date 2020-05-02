using DataAccessLibrary;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DataAccessLibrary.Services;

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
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddDbContext<DataAccessLibrary.Models.ApplicationDbContext>(options =>
             options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")),
             ServiceLifetime.Transient);
            services.AddTransient<CategoryService>();
            services.AddTransient<LanguageService>();
            services.AddTransient<LauncherService>();
            services.AddTransient<ComputerService>();
            services.AddTransient<CustomIntellisenseService>();
            services.AddTransient<GeneralLookupService>();
            services.AddTransient<ISqlDataAccess, SqlDataAccess>();
            services.AddTransient<ITodoData, TodoData>();
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
