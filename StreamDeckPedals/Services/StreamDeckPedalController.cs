using StreamDeckPedals.Services.Actions;

namespace StreamDeckPedals.Services;

public class StreamDeckPedalController : IStreamDeckPedalController
{
    private Dictionary<int, IMenuAction> _currentMenu;
    private readonly Stack<(Dictionary<int, IMenuAction> menu, string name)> _menuStack;
    private string _currentMenuName = "Main Menu";

    public bool IsInMenuMode => _menuStack.Count > 0;
    public string CurrentMenuName => _currentMenuName;

    public event EventHandler<MenuChangedEventArgs>? MenuChanged;
    public event Action? ControllerStateChanged; // Renamed for clarity

    public StreamDeckPedalController()
    {
        _menuStack = new Stack<(Dictionary<int, IMenuAction>, string)>();
        _currentMenu = new Dictionary<int, IMenuAction>(); // Initialize _currentMenu here
        InitializeMainMenu();
    }

    private void InitializeMainMenu()
    {
        _currentMenu = new Dictionary<int, IMenuAction>
        {
            [0] = new MenuToggleAction(),
            [1] = new MouseMenuAction(),
            [2] = new ScrollMenuAction()
        };
        _currentMenuName = "Main Menu";
        NotifyMenuChanged();
        ControllerStateChanged?.Invoke(); // Notify initial state
    }

    public void Initialize() // Explicit Initialize method
    {
        InitializeMainMenu();
    }

    public async Task OnPedalPressedAsync(int pedalIndex)
    {
        if (_currentMenu.ContainsKey(pedalIndex))
        {
            await _currentMenu[pedalIndex].ExecuteAsync(this);
        }
        else
        {
            await UpdateStatusAsync($"No action assigned to pedal {pedalIndex + 1}");
        }

        NotifyMenuChanged(); // Ensure menu change notification happens after execution
        ControllerStateChanged?.Invoke();
    }

    public async Task EnterSubMenuAsync(Dictionary<int, IMenuAction> subMenu, string menuName)
    {
        _menuStack.Push((_currentMenu, _currentMenuName));
        _currentMenu = subMenu;
        _currentMenuName = menuName;
        
        await UpdateStatusAsync($"Entered {menuName}");
        NotifyMenuChanged();
        ControllerStateChanged?.Invoke();
    }

    public async Task ExitCurrentMenuAsync()
    {
        if (_menuStack.Count > 0)
        {
            var (menu, name) = _menuStack.Pop();
            _currentMenu = menu;
            _currentMenuName = name;
            
            await UpdateStatusAsync($"Returned to {_currentMenuName}");
        }
        else
        {
            await UpdateStatusAsync("Already at main menu");
        }
        
        NotifyMenuChanged();
        ControllerStateChanged?.Invoke();
    }

    public async Task UpdateStatusAsync(string status)
    {
        // This can be used to update UI status
        await Task.CompletedTask;
        Console.WriteLine($"Status: {status}");
    }

    private void NotifyMenuChanged()
    {
        var pedalActions = _currentMenu.ToDictionary(
            kvp => kvp.Key, 
            kvp => kvp.Value.Name
        );

        MenuChanged?.Invoke(this, new MenuChangedEventArgs
        {
            MenuName = _currentMenuName,
            IsInMenuMode = IsInMenuMode,
            PedalActions = pedalActions
        });
        // ControllerStateChanged?.Invoke(); // Already called by public methods
    }

    // Method to get current actions for the UI
    public IReadOnlyDictionary<int, IMenuAction> GetCurrentActions()
    {
        return _currentMenu;
    }
}
