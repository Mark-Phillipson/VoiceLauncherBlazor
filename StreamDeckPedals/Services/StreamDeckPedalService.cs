namespace StreamDeckPedals.Services;

public interface IStreamDeckPedalService : IDisposable // Add IDisposable
{
    bool IsConnected { get; }
    bool IsInMenuMode { get; }
    string CurrentMenuName { get; }
    IReadOnlyDictionary<int, IMenuAction> CurrentPedalActionObjects { get; } // Replaced property

    Task InitializeAsync();
    Task DisconnectAsync();

    event EventHandler<MenuChangedEventArgs> MenuChanged;
    event EventHandler<bool> ConnectionChanged;
    event EventHandler<string> StatusUpdated;
    event Action StateChanged; // Generic state change for UI refresh
}

public class StreamDeckPedalService : IStreamDeckPedalService // No need to add IDisposable here again, it's inherited
{
    private readonly IStreamDeckManager _streamDeckManager;
    private readonly IStreamDeckPedalController _pedalController;

    public bool IsConnected => _streamDeckManager.IsConnected;
    public bool IsInMenuMode => _pedalController.IsInMenuMode;
    public string CurrentMenuName => _pedalController.CurrentMenuName;
    public IReadOnlyDictionary<int, IMenuAction> CurrentPedalActionObjects => _pedalController.GetCurrentActions(); // Replaced property

    public event EventHandler<MenuChangedEventArgs>? MenuChanged;
    public event EventHandler<bool>? ConnectionChanged;
    public event EventHandler<string>? StatusUpdated;
    public event Action? StateChanged; // Generic state change for UI refresh

    public StreamDeckPedalService(IStreamDeckManager streamDeckManager, IStreamDeckPedalController pedalController)
    {
        _streamDeckManager = streamDeckManager;
        _pedalController = pedalController;

        // Wire up events
        _streamDeckManager.PedalPressed += OnPedalPressed;
        _streamDeckManager.ConnectionChanged += OnConnectionChanged;
        _pedalController.ControllerStateChanged += OnControllerStateChanged; // Subscribe to new event
        // _pedalController.MenuChanged += OnMenuChanged; // This is now handled by ControllerStateChanged
    }

    private void OnControllerStateChanged()
    {
        StateChanged?.Invoke();
        // Update derived properties if needed, or let UI pull them
        var menuArgs = new MenuChangedEventArgs
        {
            MenuName = _pedalController.CurrentMenuName,
            IsInMenuMode = _pedalController.IsInMenuMode,
            PedalActions = _pedalController.GetCurrentActions().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Name)
        };
        MenuChanged?.Invoke(this, menuArgs);
        StatusUpdated?.Invoke(this, $"Menu: {menuArgs.MenuName}");
    }

    public async Task InitializeAsync()
    {
        await _streamDeckManager.InitializeAsync();
        if (_pedalController is StreamDeckPedalController controller) // Initialize if it's the concrete type
        {
            controller.Initialize();
        }
        StatusUpdated?.Invoke(this, IsConnected ? "Stream Deck Pedal connected" : "Stream Deck Pedal not found/mock");
        StateChanged?.Invoke(); // Notify UI of initial state
    }

    public async Task DisconnectAsync()
    {
        await _streamDeckManager.DisconnectAsync();
        StatusUpdated?.Invoke(this, "Stream Deck Pedal disconnected");
    }

    private async void OnPedalPressed(object? sender, int pedalIndex)
    {
        await _pedalController.OnPedalPressedAsync(pedalIndex);
        StatusUpdated?.Invoke(this, $"Pedal {pedalIndex + 1} pressed. Menu: {CurrentMenuName}");
        // StateChanged will be invoked by the controller if the state changes
    }

    private void OnConnectionChanged(object? sender, bool isConnected)
    {
        ConnectionChanged?.Invoke(this, isConnected);
        StatusUpdated?.Invoke(this, isConnected ? "Stream Deck Pedal connected" : "Stream Deck Pedal disconnected/mock");
        StateChanged?.Invoke();
    }

    // This method is no longer directly needed if ControllerStateChanged covers it
    // private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    // {
    //     // CurrentPedalActions = e.PedalActions;
    //     MenuChanged?.Invoke(this, e);
    //     StateChanged?.Invoke();
    // }

    // Method for UI to simulate pedal press
    public async Task SimulatePedalPressAsync(int pedalIndex)
    {
        await _pedalController.OnPedalPressedAsync(pedalIndex);
        // The controller will trigger StateChanged if the menu/actions change
    }

    public void Dispose()
    {
        _streamDeckManager.ConnectionChanged -= OnConnectionChanged;
        _streamDeckManager.PedalPressed -= OnPedalPressed;
        _pedalController.ControllerStateChanged -= OnControllerStateChanged;
        // _pedalController.MenuChanged -= OnMenuChanged; 

        if (_streamDeckManager is IDisposable disposableManager)
        {
            disposableManager.Dispose();
        }
    }
}
