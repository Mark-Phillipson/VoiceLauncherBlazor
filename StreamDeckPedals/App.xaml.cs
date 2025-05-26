namespace StreamDeckPedals;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		try
		{
			var window = new Window(new MainPage()) { Title = "StreamDeckPedals" };
            // Adjust window size and position if desired for desktop
            #if WINDOWS
            window.Width = 800;
            window.Height = 600;
            #endif
            return window;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"!!!!!!!!!! ERROR CREATING WINDOW: {ex} !!!!!!!!!!");
			// Optionally, rethrow or handle gracefully, but for debugging, printing is key.
			// Consider a simple fallback window or an error display page if this were production.
			throw; // Rethrow to see if the app still crashes or if the console output is visible
		}
	}
}
