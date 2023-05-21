using Blazored.Modal;
using Blazored.Toast;

using DataAccessLibrary.Models;
using DataAccessLibrary.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using VoiceAdminMAUI.Data;

using VoiceLauncher.Repositories;
using VoiceLauncher.Services;

namespace VoiceAdminMAUI
{
  public static class MauiProgram
  {
    public static MauiApp CreateMauiApp()
    {
      var builder = MauiApp.CreateBuilder();
      builder
          .UseMauiApp<App>()
          .ConfigureFonts(fonts =>
          {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
          });

      builder.Services.AddMauiBlazorWebView();


#if DEBUG
      builder.Services.AddBlazorWebViewDeveloperTools();
      builder.Logging.AddDebug();
#endif

      builder.Services.AddSingleton<WeatherForecastService>();
      builder.Services.AddScoped<LanguageService>();
      builder.Services.AddScoped<ILauncherRepository, LauncherRepository>();
      builder.Services.AddScoped<ILauncherDataService, LauncherDataService>();
      builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
      builder.Services.AddScoped<ICategoryDataService, CategoryDataService>();
      builder.Services.AddBlazoredModal();
      builder.Services.AddBlazoredToast();
      string connectionString = "Data Source=Localhost;Initial Catalog=VoiceLauncher;Integrated Security=True;Connect Timeout=120;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
      builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
      builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
      builder.Services.AddScoped<LanguageService>();
      builder.Services.AddScoped<ICustomIntelliSenseRepository, CustomIntelliSenseRepository>();
      builder.Services.AddScoped<ICustomIntelliSenseDataService, CustomIntelliSenseDataService>();
      builder.Services.AddScoped<GeneralLookupService>();
      return builder.Build();
    }
  }
}