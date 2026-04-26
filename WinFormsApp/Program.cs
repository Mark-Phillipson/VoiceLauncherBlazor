using System.Runtime.Versioning;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;
using System;
using Microsoft.Extensions.Configuration;
using RazorClassLibrary.Services;
using System.IO.Pipes;
using System.Text;

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
				// First, bring existing instance to focus
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

				// Then, send launch arguments via named pipe (if any args provided)
				if (args.Length > 0)
				{
					try
					{
						using var client = new NamedPipeClientStream(
							".",
							"VoiceLauncherBlazor_LaunchArgs",
							PipeDirection.Out);
						
						client.Connect(1000); // 1 second timeout
						
						using var writer = new StreamWriter(client, Encoding.UTF8);
						// Send all arguments joined as a single message
						string argsMessage = string.Join("|", args);
						writer.WriteLine(argsMessage);
						writer.Flush();
					}
					catch (Exception ex)
					{
						Debug.WriteLine($"Failed to send args to running instance: {ex.Message}");
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

	Application.EnableVisualStyles();
	Application.SetCompatibleTextRenderingDefault(false);
	ApplicationConfiguration.Initialize();
	
	// Check for command line arguments to determine launch mode
	var launchSearchMode = args.Length > 0 && 
		(args[0].Equals("search", StringComparison.OrdinalIgnoreCase) || 
		 args[0].Equals("Talon", StringComparison.OrdinalIgnoreCase));
	
	AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
	{
#if DEBUG
		MessageBox.Show(text: error.ExceptionObject.ToString(), caption: "Error");
#else
		MessageBox.Show(text: "An error has occurred.", caption: "Error");
#endif
		// Log the error information (error.ExceptionObject)
	};

	Application.Run(new MainForm(launchSearchMode));
}
		public static IConfiguration? Configuration { get; private set; }
	}
}