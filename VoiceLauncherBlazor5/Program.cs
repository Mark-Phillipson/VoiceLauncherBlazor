using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace VoiceLauncherBlazor
{
    public class Program
    {
        //readonly IServiceCollection _builder;
        //public Program(IServiceCollection builder)
        //{
        //    this._builder = builder;
        //}
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
