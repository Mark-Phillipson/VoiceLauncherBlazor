namespace StreamDeckPedals.Services;

public interface IStreamDeckManager
{
    bool IsConnected { get; }
    Task<bool> InitializeAsync();
    Task DisconnectAsync();
    event EventHandler<int> PedalPressed;
    event EventHandler<bool> ConnectionChanged;
}

/* TODO: Real StreamDeck implementation
 * Once StreamDeckSharp API is working properly, replace the mock implementation above
 * with actual StreamDeck integration:
 * 
 * public class RealStreamDeckManager : IStreamDeckManager, IDisposable
 * {
 *     // Real implementation using StreamDeckSharp
 * }
 */
