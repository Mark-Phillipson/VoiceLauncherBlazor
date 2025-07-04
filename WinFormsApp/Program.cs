using System.Runtime.Versioning;
using Microsoft.Extensions.Configuration;
using RazorClassLibrary.Services;

namespace WinFormsApp
{
	[SupportedOSPlatform("windows")]
	internal static class Program
	{
		// Removed mutex to allow multiple instances
		// private static Mutex mutex = new Mutex(true, "{B1A7D5F9-8C3D-4A6F-9B2D-3A5D6F8E9C3D}");		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			// Initialize configuration first
			var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
			
			Configuration = new ConfigurationBuilder()
				.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

			// Allow multiple instances by removing mutex logic
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			ApplicationConfiguration.Initialize();
			
			// Check for command line arguments to determine launch mode
			var launchSearchMode = args.Length > 0 && 
				(args[0].Equals("search", StringComparison.OrdinalIgnoreCase) || 
				 args[0].Equals("Talon", StringComparison.OrdinalIgnoreCase));
			
			Application.Run(new MainForm(launchSearchMode));			AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
			{
#if DEBUG
				MessageBox.Show(text: error.ExceptionObject.ToString(), caption: "Error");
#else
				MessageBox.Show(text: "An error has occurred.", caption: "Error");
#endif
				// Log the error information (error.ExceptionObject)
			};

			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
		}
		public static IConfiguration? Configuration { get; private set; }
	}
}