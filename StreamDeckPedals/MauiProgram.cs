using Microsoft.Extensions.Logging;
using StreamDeckPedals.Services;
using System; // Added for Environment.GetCommandLineArgs()

namespace StreamDeckPedals;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		// Initialize StreamDeckLaunchConfig with command-line arguments
		var args = Environment.GetCommandLineArgs();
		StreamDeckLaunchConfig.Initialize(args);

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		// Register Stream Deck services
		builder.Services.AddSingleton<IStreamDeckManager, StreamDeckManager>();
		builder.Services.AddSingleton<IStreamDeckPedalController, StreamDeckPedalController>();
		builder.Services.AddSingleton<IStreamDeckPedalService, StreamDeckPedalService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
