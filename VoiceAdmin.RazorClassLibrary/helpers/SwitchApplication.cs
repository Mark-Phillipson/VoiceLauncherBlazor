using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RazorClassLibrary.helpers
{
	public static class SwitchApplication
	{
		[DllImport("user32.dll")]
		static extern bool SetForegroundWindow(IntPtr hWnd);
		public static void SwitchToApplication(string application)
		{
			Process[] processes = Process.GetProcessesByName(application);
			if (processes.Length > 0)
			{
				IntPtr handle = processes[0].MainWindowHandle;
				SetForegroundWindow(handle);
			}
		}
	}
}
