namespace RazorClassLibrary.Services;

/// <summary>
/// Publishes activation events from the native clipboard window to its Blazor components.
/// </summary>
public sealed class ClipboardWindowActivationService
{
    public event EventHandler? Activated;

    public void NotifyActivated()
    {
        Activated?.Invoke(this, EventArgs.Empty);
    }
}
