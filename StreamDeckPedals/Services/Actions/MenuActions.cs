using StreamDeckPedals.Services;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StreamDeckPedals.Services.Actions;

public class MenuToggleAction : IMenuAction
{
    public string Name => "Back/Exit";
    public string Description => "Return to previous menu or exit menu mode";

    public async Task ExecuteAsync(IStreamDeckPedalController controller)
    {
        await controller.ExitCurrentMenuAsync();
    }
}

public class MouseMenuAction : IMenuAction
{
    public string Name => "Mouse Controls";
    public string Description => "Enter mouse control menu";

    public async Task ExecuteAsync(IStreamDeckPedalController controller)
    {
        var mouseMenu = new Dictionary<int, IMenuAction>
        {
            [0] = new MenuToggleAction(),
            [1] = new MouseUpAction(),
            [2] = new MouseDownAction()
        };
        await controller.EnterSubMenuAsync(mouseMenu, "Mouse Controls");
    }
}

public class ScrollMenuAction : IMenuAction
{
    public string Name => "Scroll Controls";
    public string Description => "Enter scroll control menu";

    public async Task ExecuteAsync(IStreamDeckPedalController controller)
    {
        var scrollMenu = new Dictionary<int, IMenuAction>
        {
            [0] = new MenuToggleAction(),
            [1] = new ScrollUpAction(),
            [2] = new ScrollDownAction()
        };
        await controller.EnterSubMenuAsync(scrollMenu, "Scroll Controls");
    }
}

public class MouseUpAction : IMenuAction
{
    public string Name => "Mouse Up";
    public string Description => "Move mouse cursor up";    public async Task ExecuteAsync(IStreamDeckPedalController controller)
    {
        await controller.UpdateStatusAsync("Moving mouse up...");
        
        var currentPos = Cursor.Position;
        Cursor.Position = new System.Drawing.Point(currentPos.X, currentPos.Y - 10);
        
        await Task.Delay(100); // Brief delay for user feedback
    }
}

public class MouseDownAction : IMenuAction
{
    public string Name => "Mouse Down";
    public string Description => "Move mouse cursor down";    public async Task ExecuteAsync(IStreamDeckPedalController controller)
    {
        await controller.UpdateStatusAsync("Moving mouse down...");
        
        var currentPos = Cursor.Position;
        Cursor.Position = new System.Drawing.Point(currentPos.X, currentPos.Y + 10);
        
        await Task.Delay(100);
    }
}

public class ScrollUpAction : IMenuAction
{
    public string Name => "Scroll Up";
    public string Description => "Scroll page up";

    public async Task ExecuteAsync(IStreamDeckPedalController controller)
    {
        await controller.UpdateStatusAsync("Scrolling up...");
        
        // Use Windows API for more reliable scrolling
        WindowsInputHelper.SendScrollUp();
        
        await Task.Delay(100);
    }
}

public class ScrollDownAction : IMenuAction
{
    public string Name => "Scroll Down";
    public string Description => "Scroll page down";

    public async Task ExecuteAsync(IStreamDeckPedalController controller)
    {
        await controller.UpdateStatusAsync("Scrolling down...");
        
        // Use Windows API for more reliable scrolling
        WindowsInputHelper.SendScrollDown();
        
        await Task.Delay(100);
    }
}

// Helper class for Windows-specific input operations
public static class WindowsInputHelper
{
    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

    private const uint MOUSEEVENTF_WHEEL = 0x0800;
    private const uint WHEEL_DELTA = 120;

    public static void SendScrollUp()
    {
        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, WHEEL_DELTA, UIntPtr.Zero);
    }

    public static void SendScrollDown()
    {
        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, unchecked((uint)(-((int)WHEEL_DELTA))), UIntPtr.Zero);
    }
}
