using System.Runtime.Versioning;
using Microsoft.Extensions.Configuration;

namespace WinFormsApp
{
	[SupportedOSPlatform("windows")]
	internal static class Program
	{
		private static Mutex mutex = new Mutex(true, "{B1A7D5F9-8C3D-4A6F-9B2D-3A5D6F8E9C3D}");
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// Initialize configuration first
			Configuration = new ConfigurationBuilder()
				.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.Build();

			if (mutex.WaitOne(TimeSpan.Zero, true))
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				ApplicationConfiguration.Initialize();
				Application.Run(new MainForm());
				mutex.ReleaseMutex();
			}
			else
			{
				// Bring the existing instance to the foreground
				// MessageBox.Show("An instance of the application is already running.");
				NativeMethods.BringExistingInstanceToFront();
			}
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


		}
		public static IConfiguration? Configuration { get; private set; }

	}
	internal static class NativeMethods
	{
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool IsIconic(IntPtr hWnd);

		private const int SW_RESTORE = 9;

		public static void BringExistingInstanceToFront()
		{
			var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
			foreach (var process in System.Diagnostics.Process.GetProcessesByName(currentProcess.ProcessName))
			{
				if (process.Id != currentProcess.Id)
				{
					if (IsIconic(process.MainWindowHandle))
					{
						ShowWindowAsync(process.MainWindowHandle, SW_RESTORE);
					}
					SetForegroundWindow(process.MainWindowHandle);
					break;
				}
			}
		}
	}
}