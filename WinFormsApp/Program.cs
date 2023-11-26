using Microsoft.Extensions.Configuration;


namespace WinFormsApp
{
   internal static class Program
   {
      /// <summary>
      ///  The main entry point for the application.
      /// </summary>
      [STAThread]
      static void Main()
      {

         // To customize application configuration such as set high DPI settings or default font,
         // see https://aka.ms/applicationconfiguration.


         ApplicationConfiguration.Initialize();
         Application.Run(new MainForm());
      }
      public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

   }
}