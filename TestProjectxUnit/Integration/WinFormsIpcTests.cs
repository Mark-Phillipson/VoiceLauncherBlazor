using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using Xunit;

namespace TestProjectxUnit.Integration
{
    public class WinFormsIpcTests
    {
        [Fact]
        public async Task RepeatedIpcMessages_DoNotCrashApp()
        {
            if (!OperatingSystem.IsWindows())
                return;

            // Locate repository root from test assembly location
            var baseDir = AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();
            var di = new DirectoryInfo(baseDir);
            var repoRoot = di.Parent?.Parent?.Parent?.Parent; // ../../../../ from bin/<cfg>/net*/
            if (repoRoot == null)
                repoRoot = new DirectoryInfo(Directory.GetCurrentDirectory());

            var exePath = Path.Combine(repoRoot.FullName, "WinFormsApp", "bin", "Release", "net10.0-windows", "WinFormsApp.exe");
            var dllPath = Path.Combine(repoRoot.FullName, "WinFormsApp", "bin", "Release", "net10.0-windows", "WinFormsApp.dll");

            Process? process = null;
            try
            {
                ProcessStartInfo psi;
                if (File.Exists(exePath))
                {
                    psi = new ProcessStartInfo(exePath)
                    {
                        UseShellExecute = false
                    };
                }
                else if (File.Exists(dllPath))
                {
                    psi = new ProcessStartInfo("dotnet", '"' + dllPath + '"')
                    {
                        UseShellExecute = false
                    };
                }
                else
                {
                    throw new FileNotFoundException($"Could not find WinFormsApp executable or dll at '{exePath}' or '{dllPath}'");
                }

                process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start WinFormsApp process");

                var pipeName = "VoiceLauncherBlazor_LaunchArgs";

                // Wait for the named pipe server to be available (retry loop)
                var connected = false;
                for (int attempt = 0; attempt < 100; attempt++)
                {
                    try
                    {
                        using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
                        client.Connect(200);
                        using var sw = new StreamWriter(client) { AutoFlush = true };
                        sw.WriteLine("Talon|search|integration-probe");
                        connected = true;
                        break;
                    }
                    catch
                    {
                        await Task.Delay(100);
                    }
                }

                Assert.True(connected, "Failed to connect to WinFormsApp named pipe server within timeout.");

                // Rapidly send multiple IPC messages
                for (int i = 0; i < 30; i++)
                {
                    try
                    {
                        using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
                        client.Connect(1000);
                        using var sw = new StreamWriter(client) { AutoFlush = true };
                        sw.WriteLine($"Talon|search|rapid-message-{i}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Named-pipe send failed: {ex.Message}");
                    }
                    await Task.Delay(20);
                }

                // Allow some time for the app to process renders
                await Task.Delay(1500);

                // Verify the process is still running (no crash)
                Assert.False(process.HasExited, "WinFormsApp exited after rapid IPC messages (possible crash)");
            }
            finally
            {
                try
                {
                    if (process != null && !process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                        process.WaitForExit(2000);
                    }
                }
                catch { }
            }
        }
    }
}
