# How to Launch the StreamDeckPedals MAUI Application

This document provides instructions on how to launch the StreamDeckPedals MAUI application.

## Prerequisites

1.  Ensure you have the .NET 9 SDK installed.
2.  Ensure you have the .NET MAUI workload installed:
    ```powershell
    dotnet workload install maui
    ```
3.  Navigate to the project directory in your terminal:
    ```powershell
    cd c:\Users\MPhil\source\repos\VoiceLauncherBlazor\StreamDeckPedals
    ```

## Option 1: Recommended CLI Method (MAUI Specific)

This method explicitly tells MSBuild to run the MAUI application for the specified Windows framework. This should launch the application directly as a desktop window.

```powershell
cd c:\Users\MPhil\source\repos\VoiceLauncherBlazor\StreamDeckPedals
dotnet build -t:Run -f net9.0-windows10.0.19041.0
```

## Option 2: Running from an IDE (Visual Studio / JetBrains Rider)

1.  Open the `VoiceLauncherBlazor.sln` solution in your IDE.
2.  Set `StreamDeckPedals` as the startup project.
3.  Ensure the launch profile is set to "Windows Machine" (or the equivalent for your IDE).
4.  Click the "Run" or "Debug" button in the IDE.

This is often the most straightforward way to develop and run MAUI applications.

## Option 3: Using `dotnet run` (Current Behavior Note)

You can also use `dotnet run`. However, as observed, this might currently launch a web server and output a localhost URL due to how it interprets `launchSettings.json` or other project configurations. The desktop app window might still appear, or it might not, depending on the exact .NET SDK behavior.

1.  Navigate to the project directory (if not already there):
    ```powershell
    cd c:\Users\MPhil\source\repos\VoiceLauncherBlazor\StreamDeckPedals
    ```
2.  Run the command:
    ```powershell
    dotnet run
    ```
    If the desktop window doesn't appear and you only see web server output, prefer Option 1 or Option 2.

## Cleaning the Project (Optional Troubleshooting)

If you encounter issues, cleaning the project can sometimes help:

```powershell
dotnet clean
```
Then try one of the run commands above.
