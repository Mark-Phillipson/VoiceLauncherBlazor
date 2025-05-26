using System;
using System.Collections.Generic;

namespace StreamDeckPedals.Services;

public static class StreamDeckLaunchConfig
{
    public static int Port { get; private set; }
    public static string? PluginUuid { get; private set; } // Made nullable
    public static string? RegisterEvent { get; private set; } // Made nullable

    // Define your action UUIDs and map them to pedal indices (0, 1, 2)
    // IMPORTANT: Replace these with the actual UUIDs from your manifest.json
    public static readonly Dictionary<string, int> ActionToPedalIndexMap = new Dictionary<string, int>
    {
        { "com.voicelauncher.pedal1", 0 }, // Example UUID for pedal 1
        { "com.voicelauncher.pedal2", 1 }, // Example UUID for pedal 2
        { "com.voicelauncher.pedal3", 2 }  // Example UUID for pedal 3
    };

    public static void Initialize(string[] args)
    {
        // Initialize with defaults or indicate they are unset
        Port = 0;
        PluginUuid = null; 
        RegisterEvent = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-port":
                    if (i + 1 < args.Length && int.TryParse(args[i + 1], out int portValue))
                    {
                        Port = portValue;
                        i++; // Skip next argument as it's consumed
                    }
                    break;
                case "-pluginUUID":
                    if (i + 1 < args.Length)
                    {
                        PluginUuid = args[i + 1];
                        i++; // Skip next argument as it's consumed
                    }
                    break;
                case "-registerEvent":
                    if (i + 1 < args.Length)
                    {
                        RegisterEvent = args[i + 1];
                        i++; // Skip next argument as it's consumed
                    }
                    break;
            }
        }
        // Basic validation or logging can be added here
        Console.WriteLine($"StreamDeckLaunchConfig Initialized: Port: {Port}, PluginUUID: {PluginUuid ?? "Not Set"}, RegisterEvent: {RegisterEvent ?? "Not Set"}");
    }
}
