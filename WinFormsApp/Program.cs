using System.Runtime.Versioning;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;
using System;
using Microsoft.Extensions.Configuration;
using RazorClassLibrary.Services;

namespace WinFormsApp
{
	[SupportedOSPlatform("windows")]
	internal static class Program
	{
		private static Mutex? _singleInstanceMutex;
		private const int SW_RESTORE = 9;
		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);
		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		// Ensure only a single instance runs

		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			// Try to acquire a named mutex; if another instance exists, bring it to foreground and exit
			bool createdNew = false;
			try
			{
				_singleInstanceMutex = new Mutex(true, "Global\\VoiceLauncherBlazor_SingleInstance", out createdNew);
			}
			catch
			{
				createdNew = true; // fall back to allowing start if mutex cannot be created
			}

			if (!createdNew)
			{
				try
				{
					var current = Process.GetCurrentProcess();
					var others = Process.GetProcessesByName(current.ProcessName).Where(p => p.Id != current.Id);
					foreach (var p in others)
					{
						var h = p.MainWindowHandle;
						if (h != IntPtr.Zero)
						{
							ShowWindow(h, SW_RESTORE);
							SetForegroundWindow(h);
							break;
						}
					}
				}
				catch { }

				return; // another instance is running
			}

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