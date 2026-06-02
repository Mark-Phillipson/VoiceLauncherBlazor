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

                // Allow tests to start isolated instances even if a user instance is running
                try
                {
                    // Prefer ProcessStartInfo.Environment (available on .NET Core / .NET 5+)
                    try
                    {
                        psi.Environment["VOICE_LAUNCHER_ALLOW_MULTIPLE_INSTANCES"] = "1";
                    }
                    catch
                    {
                        // Fallback for older API surface
                        psi.EnvironmentVariables["VOICE_LAUNCHER_ALLOW_MULTIPLE_INSTANCES"] = "1";
                    }
                }
                catch { }

                // Redirect output so we can capture crashes / exception messages from the child process
                try { psi.RedirectStandardOutput = true; psi.RedirectStandardError = true; } catch { }
                process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start WinFormsApp process");
                Task<string> stdoutTask = Task.FromResult(string.Empty);
                Task<string> stderrTask = Task.FromResult(string.Empty);
                try
                {
                    stdoutTask = process.StandardOutput.ReadToEndAsync();
                    stderrTask = process.StandardError.ReadToEndAsync();
                }
                catch { }

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
                if (process.HasExited)
                {
                    var outText = string.Empty;
                    var errText = string.Empty;
                    try { outText = await stdoutTask; } catch { }
                    try { errText = await stderrTask; } catch { }
                    var combined = $"WinFormsApp exited after rapid IPC messages (possible crash)\nExitCode={process.ExitCode}\nStdout:\n{outText}\nStderr:\n{errText}";
                    Assert.False(process.HasExited, combined);
                }
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
