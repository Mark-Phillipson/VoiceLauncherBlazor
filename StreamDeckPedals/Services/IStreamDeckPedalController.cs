namespace StreamDeckPedals.Services;

public interface IStreamDeckPedalController
{
    bool IsInMenuMode { get; }
    string CurrentMenuName { get; }
    Task EnterSubMenuAsync(Dictionary<int, IMenuAction> subMenu, string menuName);
    Task ExitCurrentMenuAsync();
    Task OnPedalPressedAsync(int pedalIndex);
    Task UpdateStatusAsync(string status); // Added this line as it was used by actions

    // Events
    event EventHandler<MenuChangedEventArgs> MenuChanged;
    event Action? ControllerStateChanged; // Added this line

    // Methods
    IReadOnlyDictionary<int, IMenuAction> GetCurrentActions(); // Added this line
    void Initialize(); // Added this line for explicit initialization
}

public class MenuChangedEventArgs : EventArgs
{
    public string MenuName { get; set; } = string.Empty;
    public bool IsInMenuMode { get; set; }
    public Dictionary<int, string> PedalActions { get; set; } = new();
}
