using DataAccessLibrary.Models;
using Microsoft.VisualBasic.ApplicationServices;
using RazorClassLibrary.Pages;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WindowsInput;
using WindowsInput.Native;

namespace VoiceAdminMAUI.Pages
{
   public class Commands
   {
      [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
      public static extern nint FindWindow(string lpClassName,
string lpWindowName);

      // Activate an application window.
      [DllImport("USER32.DLL")]
      public static extern bool SetForegroundWindow(nint hWnd);
      [DllImport("user32.dll")]
      public static extern nint GetWindowThreadProcessId(nint hWnd, out uint ProcessId);

      [DllImport("user32.dll")]
      public static extern nint GetForegroundWindow();

      static uint MOUSEEVENTF_WHEEL = 0x800;


      [DllImport("user32.dll")]
      static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

      public Process currentProcess { get; set; }

      readonly InputSimulator inputSimulator = new InputSimulator();
      public string PerformCommand(string[] args)
      {
         string[] arguments;
         //MessageBox.Show("line 20");
         if (args.Count() < 2)
         {
            //arguments = new string[] { args[0], "Error Message: There is an error in the program!" };
            //arguments = new string[] { args[0], "explorer" };
            //arguments = new string[] { args[0], "show cursor" };
            //arguments = new string[] { args[0], "sapisvr" };
            //arguments = new string[] { args[0], "click" };
            //arguments = new string[] { args[0], "/startstoplistening" };
            //arguments = new string[] { args[0], "ScrollRight" };
            arguments = new string[] { args[0], "SearchIntelliSense", "Not Applicable","Snippet" };
            //arguments = new string[] { args[0], "StartContinuousDictation" };

         }
         else
         {
            arguments = args;
            arguments[1] = arguments[1].Replace("/", "").Trim();
            arguments[2] = arguments[2].Replace("/", "").Trim();

         }
         //MessageBox.Show("Got here line sixty two With argument1 " + arguments[1]+ "second argument"+arguments[2]);

         //if (arguments[1].Contains("SearchIntelliSense"))
         //{
         //   customIntelliSense.LanguageName = arguments[2].Replace("/", "").Trim();
         //   customIntelliSense.CategoryName = arguments[3].Replace("/", "").Trim();

         //   return "success";
         //}
         //else
         {
            return "Arguments did not match any commands! 1: " + arguments[1] + " 2: " + arguments[2];
         }
      }
   }
}
