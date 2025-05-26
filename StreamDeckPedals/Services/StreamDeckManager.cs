using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace StreamDeckPedals.Services;

// Helper classes for JSON deserialization (based on Elgato SDK docs)
internal class StreamDeckRegistrationMessage
{
    [JsonPropertyName("event")]
    public string Event { get; set; }
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; }
}

internal class StreamDeckEventBase
{
    [JsonPropertyName("event")]
    public string Event { get; set; }
    [JsonPropertyName("device")]
    public string Device { get; set; }
    [JsonPropertyName("context")]
    public string Context { get; set; }
    [JsonPropertyName("action")]
    public string Action { get; set; }
}

internal class StreamDeckKeyDownEvent : StreamDeckEventBase
{
    // Inherits event, device, context, action
    // Add payload if specific details are needed, e.g., for coordinates or settings
}

internal class StreamDeckDeviceConnectEvent : StreamDeckEventBase
{
    [JsonPropertyName("deviceInfo")]
    public ElgatoDeviceInfo DeviceInfo { get; set; }
}

internal class ElgatoDeviceInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("type")]
    public int Type { get; set; } // kESDSDKDeviceType_StreamDeckPedal = 4
    [JsonPropertyName("size")]
    public DeviceSize Size { get; set; }
}

internal class DeviceSize
{
    [JsonPropertyName("columns")]
    public int Columns { get; set; }
    [JsonPropertyName("rows")]
    public int Rows { get; set; }
}

public class StreamDeckManager : IStreamDeckManager, IDisposable
{
    private ClientWebSocket? _clientWebSocket;
    private CancellationTokenSource? _cts;
    private Task? _receiveLoopTask;
    private bool _isConnectedToWebSocket = false;
    private bool _isPedalDeviceConnected = false;
    private string? _pedalDeviceId;

    private readonly Dictionary<string, int> _actionToPedalIndexMap;
    private string? _port;
    private string? _pluginUuid;
    private string? _registerEventName;

    private const int StreamDeckPedalDeviceType = 4; // From Elgato SDK docs

    public bool IsConnected => _isConnectedToWebSocket && _isPedalDeviceConnected;

    public event EventHandler<int>? PedalPressed;
    public event EventHandler<bool>? ConnectionChanged;

    public StreamDeckManager()
    {
        // Parameters will be fetched from StreamDeckLaunchConfig in InitializeAsync
        // Ensure StreamDeckLaunchConfig is populated before InitializeAsync is called.
        _actionToPedalIndexMap = StreamDeckLaunchConfig.ActionToPedalIndexMap ?? new Dictionary<string, int>();
    }

    public async Task<bool> InitializeAsync()
    {
        _port = StreamDeckLaunchConfig.Port;
        _pluginUuid = StreamDeckLaunchConfig.PluginUuid;
        _registerEventName = StreamDeckLaunchConfig.RegisterEvent;

        if (string.IsNullOrEmpty(_port) || string.IsNullOrEmpty(_pluginUuid) || string.IsNullOrEmpty(_registerEventName))
        {
            Console.WriteLine("Stream Deck Manager: Registration parameters not provided.");
            return false;
        }

        if (_clientWebSocket != null && _clientWebSocket.State == WebSocketState.Open)
        {
            return true; // Already initialized
        }

        _clientWebSocket = new ClientWebSocket();
        _cts = new CancellationTokenSource();
        var uri = new Uri($"ws://127.0.0.1:{_port}");

        try
        {
            await _clientWebSocket.ConnectAsync(uri, _cts.Token);
            _isConnectedToWebSocket = true;
            Console.WriteLine("Stream Deck Manager: Connected to WebSocket.");

            var registrationMessage = new StreamDeckRegistrationMessage
            {
                Event = _registerEventName,
                Uuid = _pluginUuid
            };
            string jsonMessage = JsonSerializer.Serialize(registrationMessage);
            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, _cts.Token);
            Console.WriteLine("Stream Deck Manager: Registration message sent.");

            _receiveLoopTask = ReceiveLoopAsync(_clientWebSocket, _cts.Token);
            UpdateOverallConnectionState(); 
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Stream Deck Manager: Error initializing or connecting: {ex.Message}");
            _isConnectedToWebSocket = false;
            UpdateOverallConnectionState();
            return false;
        }
    }

    private async Task ReceiveLoopAsync(ClientWebSocket webSocket, CancellationToken token)
    {
        var buffer = new ArraySegment<byte>(new byte[8192]);
        try
        {
            while (webSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
            {
                WebSocketReceiveResult result;
                using (var ms = new System.IO.MemoryStream())
                {
                    do
                    {
                        result = await webSocket.ReceiveAsync(buffer, token);
                        if (result.MessageType == WebSocketMessageType.Close) break;
                        ms.Write(buffer.Array!, buffer.Offset, result.Count);
                    }
                    while (!result.EndOfMessage);
                    
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("Stream Deck Manager: WebSocket closed by server.");
                        break;
                    }

                    ms.Seek(0, System.IO.SeekOrigin.Begin);
                    string message = Encoding.UTF8.GetString(ms.ToArray());
                    HandleWebSocketMessage(message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Stream Deck Manager: Receive loop cancelled.");
        }
        catch (WebSocketException ex)
        {
            Console.WriteLine($"Stream Deck Manager: WebSocket error in receive loop: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Stream Deck Manager: Error in receive loop: {ex.Message}");
        }
        finally
        {
            _isConnectedToWebSocket = false;
            _isPedalDeviceConnected = false; // Assume pedal disconnects if WebSocket does
            _pedalDeviceId = null;
            UpdateOverallConnectionState();
        }
    }

    private void HandleWebSocketMessage(string message)
    {
        try
        {
            //Console.WriteLine($"Stream Deck Plugin Received: {message}");
            var baseEvent = JsonSerializer.Deserialize<StreamDeckEventBase>(message);

            if (baseEvent == null) return;

            switch (baseEvent.Event)
            {
                case "keyDown":
                    var keyDownEvent = JsonSerializer.Deserialize<StreamDeckKeyDownEvent>(message);
                    if (keyDownEvent != null && _actionToPedalIndexMap.TryGetValue(keyDownEvent.Action, out int pedalIndex))
                    {
                        if (_pedalDeviceId == null || keyDownEvent.Device == _pedalDeviceId) // Ensure event is from our pedal
                        {
                             PedalPressed?.Invoke(this, pedalIndex);
                             Console.WriteLine($"Stream Deck Manager: Pedal {pedalIndex} pressed (Action: {keyDownEvent.Action}).");
                        }
                    }
                    break;
                // case "keyUp": // Handle if needed
                //     break;
                case "deviceDidConnect":
                    var connectEvent = JsonSerializer.Deserialize<StreamDeckDeviceConnectEvent>(message);
                    if (connectEvent?.DeviceInfo?.Type == StreamDeckPedalDeviceType)
                    {
                        _isPedalDeviceConnected = true;
                        _pedalDeviceId = connectEvent.Device;
                        Console.WriteLine($"Stream Deck Manager: Pedal device connected (ID: {_pedalDeviceId}, Name: {connectEvent.DeviceInfo.Name}).");
                        UpdateOverallConnectionState();
                    }
                    break;
                case "deviceDidDisconnect":
                    if (baseEvent.Device == _pedalDeviceId)
                    {
                        _isPedalDeviceConnected = false;
                        _pedalDeviceId = null;
                        Console.WriteLine("Stream Deck Manager: Pedal device disconnected.");
                        UpdateOverallConnectionState();
                    }
                    break;
                // Handle other events like willAppear, applicationDidLaunch etc. if needed
            }
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine($"Stream Deck Manager: Error deserializing JSON: {jsonEx.Message}. Message: {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Stream Deck Manager: Error handling message: {ex.Message}");
        }
    }

    private void UpdateOverallConnectionState()
    {
        bool previousState = IsConnected; // This will call the getter which uses the old field values
        bool currentState = _isConnectedToWebSocket && _isPedalDeviceConnected;
        if (previousState != currentState)
        {
            ConnectionChanged?.Invoke(this, currentState);
        }
    }

    public async Task DisconnectAsync()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        if (_clientWebSocket != null)
        {
            if (_clientWebSocket.State == WebSocketState.Open || _clientWebSocket.State == WebSocketState.CloseReceived)
            {
                try
                {
                    await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Stream Deck Manager: Error closing WebSocket: {ex.Message}");
                }
            }
            _clientWebSocket.Dispose();
            _clientWebSocket = null;
        }

        if (_receiveLoopTask != null && !_receiveLoopTask.IsCompleted)
        {
            try { await _receiveLoopTask; } catch { /* Ignored as we are shutting down */ }
        }
        _receiveLoopTask = null;

        _isConnectedToWebSocket = false;
        _isPedalDeviceConnected = false;
        _pedalDeviceId = null;
        UpdateOverallConnectionState();
        Console.WriteLine("Stream Deck Manager: Disconnected.");
    }

    public void SimulatePedalPress(int pedalIndex)
    {
        // This method was for the mock implementation and is no longer applicable
        // with the actual Stream Deck SDK. It can be left as a no-op or throw NotSupportedException.
        Console.WriteLine("Stream Deck Manager: SimulatePedalPress is not supported with the Elgato SDK.");
        // PedalPressed?.Invoke(this, pedalIndex); // Or simulate if really needed for some internal test
    }

    public void Dispose()
    {
        DisconnectAsync().Wait(); // Ensure cleanup
        GC.SuppressFinalize(this);
    }
}
