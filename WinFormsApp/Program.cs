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
			AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
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

			ApplicationConfiguration.Initialize();
			Application.Run(new MainForm());

		}
		public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
	 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
	 .Build();

	}

}