namespace StreamDeckPedals.Services;

public interface IStreamDeckManager
{
    bool IsConnected { get; }
    Task<bool> InitializeAsync();
    Task DisconnectAsync();
    event EventHandler<int> PedalPressed;
    event EventHandler<bool> ConnectionChanged;
}

// Mock implementation for development and testing
// This will be replaced with actual StreamDeck implementation once the API is working
public class StreamDeckManager : IStreamDeckManager, IDisposable
{
    private bool _isConnected;
    private Timer? _simulationTimer;

    public bool IsConnected => _isConnected;

    public event EventHandler<int>? PedalPressed;
    public event EventHandler<bool>? ConnectionChanged;

    public async Task<bool> InitializeAsync()
    {
        try
        {
            // Simulate connection to Stream Deck
            await Task.Delay(1000); // Simulate connection time
            
            _isConnected = true;
            Console.WriteLine("Mock Stream Deck Pedal connected (simulation mode)");
            
            // Start simulation timer for testing
            StartSimulation();
            
            ConnectionChanged?.Invoke(this, _isConnected);
            return _isConnected;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing mock Stream Deck: {ex.Message}");
            _isConnected = false;
            ConnectionChanged?.Invoke(this, _isConnected);
            return false;
        }
    }

    private void StartSimulation()
    {
        // Simulate random pedal presses for testing (every 10-15 seconds)
        var random = new Random();
        _simulationTimer = new Timer((_) =>
        {
            if (_isConnected)
            {
                var randomPedal = random.Next(0, 3); // 0, 1, or 2
                PedalPressed?.Invoke(this, randomPedal);
                Console.WriteLine($"Simulated pedal {randomPedal + 1} press");
            }
        }, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(random.Next(10, 16)));
    }

    public async Task DisconnectAsync()
    {
        await Task.Run(() =>
        {
            _simulationTimer?.Dispose();
            _simulationTimer = null;
            _isConnected = false;
        });
        
        Console.WriteLine("Mock Stream Deck Pedal disconnected");
        ConnectionChanged?.Invoke(this, _isConnected);
    }

    // Method to manually simulate pedal press for testing
    public void SimulatePedalPress(int pedalIndex)
    {
        if (_isConnected && pedalIndex >= 0 && pedalIndex < 3)
        {
            PedalPressed?.Invoke(this, pedalIndex);
        }
    }

    public void Dispose()
    {
        DisconnectAsync().Wait();
    }
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
