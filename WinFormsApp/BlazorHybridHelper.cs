using Microsoft.JSInterop;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace WinFormsApp
{
    [SupportedOSPlatform("windows")]
    public static class BlazorHybridHelper
    {
        // Windows API declarations for paste functionality
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const int VK_CONTROL = 0x11;
        private const int VK_V = 0x56;
        private const int KEYEVENTF_KEYUP = 0x0002;

        [JSInvokable]
        public static void TestMethod()
        {
            Console.WriteLine("TEST: JSInvokable method called successfully!");
        }

        [JSInvokable]
        public static async Task TriggerPasteToActiveWindow()
        {
            await Task.Delay(200); // Give time for focus to change
            
            try
            {
                Console.WriteLine("Starting paste operation...");
                
                // Try using SendKeys first (more reliable for Windows Forms apps)
                try
                {
                    Console.WriteLine("Attempting SendKeys approach...");
                    System.Windows.Forms.SendKeys.SendWait("^v");
                    Console.WriteLine("SendKeys paste sent successfully");
                    return;
                }
                catch (Exception sendKeysEx)
                {
                    Console.WriteLine($"SendKeys failed: {sendKeysEx.Message}, trying keybd_event...");
                }

                // Fallback to keybd_event approach
                await Task.Run(() =>
                {
                    try
                    {
                        // Get the currently focused window
                        IntPtr currentWindow = GetForegroundWindow();
                        Console.WriteLine($"Current window handle: {currentWindow}");

                        // Make sure the target window is focused
                        if (currentWindow != IntPtr.Zero)
                        {
                            SetForegroundWindow(currentWindow);
                            Thread.Sleep(100);
                        }

                        // Send Ctrl+V to paste
                        Console.WriteLine("Sending Ctrl+V key combination...");
                        keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero); // Press Ctrl
                        Thread.Sleep(50);
                        keybd_event(VK_V, 0, 0, UIntPtr.Zero); // Press V
                        Thread.Sleep(50);
                        keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Release V
                        Thread.Sleep(50);
                        keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Release Ctrl

                        Console.WriteLine("keybd_event paste sent successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"keybd_event error: {ex.Message}");
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Overall paste operation failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
