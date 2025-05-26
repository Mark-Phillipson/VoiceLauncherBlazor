namespace StreamDeckPedals.Services;

public interface IMenuAction
{
    string Name { get; }
    string Description { get; }
    Task ExecuteAsync(IStreamDeckPedalController controller);
}
