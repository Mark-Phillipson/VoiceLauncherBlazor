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
