using Microsoft.Extensions.Logging;
using StreamDeckPedals.Services;

namespace StreamDeckPedals;

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
