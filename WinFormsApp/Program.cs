using System.Runtime.Versioning;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;
using System;
using System.Collections.Generic;
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
			// Allow tests or special runs to bypass the single-instance guard by setting this env var to '1'
			var allowMultiple = Environment.GetEnvironmentVariable("VOICE_LAUNCHER_ALLOW_MULTIPLE_INSTANCES");
			bool enforceSingleInstance = !string.Equals(allowMultiple, "1", StringComparison.OrdinalIgnoreCase);
			try
			{
				if (enforceSingleInstance)
				{
					_singleInstanceMutex = new Mutex(true, "Global\\VoiceLauncherBlazor_SingleInstance", out createdNew);
				}
				else
				{
					// Tests may request multiple instances; treat as if mutex created
					createdNew = true;
				}
			}
			catch
			{
				createdNew = true; // fall back to allowing start if mutex cannot be created
			}

			// Write a small startup debug record so tests can see whether the env var was detected
			try
			{
				var startupLogDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory, "logs");
				Directory.CreateDirectory(startupLogDir);
				var startupLogPath = Path.Combine(startupLogDir, "startup.log");
				File.AppendAllText(startupLogPath, $"{DateTime.Now:o} allowMultiple={allowMultiple ?? "<null>"} enforceSingleInstance={enforceSingleInstance} createdNew={createdNew}\n");
			}
			catch { }

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
					// Normalize arguments: trim whitespace, strip surrounding quotes and leading slashes
					var normalized = args.Select(a => (a ?? string.Empty)
						.Trim()
						.Trim('"')
						.Trim('\'')
						.TrimStart('/')
						.Trim())
						.ToArray();
					string argsMessage = string.Join("|", normalized);

					// Retry connecting because the first instance may not have started
					// its pipe server yet (StartNamedPipeServer is called in OnLoad).
					const int maxAttempts = 5;
					const int retryDelayMs = 1000;
					bool sent = false;
					for (int attempt = 1; attempt <= maxAttempts && !sent; attempt++)
					{
						try
						{
							using var client = new NamedPipeClientStream(
								".",
								"VoiceLauncherBlazor_LaunchArgs",
								PipeDirection.Out);

							client.Connect(1000); // 1 second timeout per attempt

							using var writer = new StreamWriter(client, Encoding.UTF8);
							Debug.WriteLine($"Sending normalized args via pipe (attempt {attempt}): '{argsMessage}'");
							writer.WriteLine(argsMessage);
							writer.Flush();
							sent = true;

							// Wait for Index to process the IPC and write an ACK into ipc.log
							try
							{
								int ackTimeoutMs = 8000;
								var ackDeadline = DateTime.Now.AddMilliseconds(ackTimeoutMs);
								var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory, "logs", "ipc.log");
								bool ackFound = false;
								Debug.WriteLine($"Waiting up to {ackTimeoutMs}ms for Index.HandledIPC ack (searching for '{argsMessage}')");
								while (DateTime.Now < ackDeadline)
								{
									try
									{
										if (File.Exists(logPath))
										{
											var content = File.ReadAllText(logPath);
											if (content.Contains("Index.HandledIPC") && content.Contains(argsMessage))
											{
												Debug.WriteLine($"Received Index.HandledIPC ack for args: '{argsMessage}'");
												ackFound = true;
												break;
											}
										}
									}
									catch { }
									Thread.Sleep(200);
								}
								if (!ackFound)
								{
									Debug.WriteLine("Did not receive Index.HandledIPC ack within timeout.");
								}
							}
							catch (Exception ex)
							{
								Debug.WriteLine($"Error while waiting for IPC ACK: {ex.Message}");
							}
						}
						catch (Exception ex)
						{
							Debug.WriteLine($"IPC connect attempt {attempt}/{maxAttempts} failed: {ex.Message}");
							if (attempt < maxAttempts)
								Thread.Sleep(retryDelayMs);
						}
					}
					if (!sent)
						Debug.WriteLine("Failed to send args to running instance after all retry attempts.");
				}
			}
			catch { }

		return; // another instance is running
	}

	// Initialize configuration first
	var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
	
	Configuration = new ConfigurationBuilder()
		.SetBasePath(AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory)
		.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
		.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
		.AddEnvironmentVariables()
		.Build();

			// Add a developer-friendly fallback for the ClipboardHistory DB if not configured.
			try
			{
				var cbConn = Configuration.GetConnectionString("ClipboardHistory");
				if (string.IsNullOrWhiteSpace(cbConn))
				{
					// Known local repo path (developer-provided). Use when present.
					var altPath = @"C:\Users\MPhil\source\repos\personal-assistant\clipboard-history.db";
					if (File.Exists(altPath))
					{
						var mem = new Dictionary<string, string?>
						{
							["ConnectionStrings:ClipboardHistory"] = $"Data Source={altPath}"
						};
						Configuration = new ConfigurationBuilder().AddConfiguration(Configuration).AddInMemoryCollection(mem).Build();
						Console.WriteLine($"ClipboardHistory connection string set from fallback: {altPath}");
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error applying ClipboardHistory fallback: {ex.Message}");
			}

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