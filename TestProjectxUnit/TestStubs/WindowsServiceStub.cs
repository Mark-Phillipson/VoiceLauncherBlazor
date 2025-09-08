using RazorClassLibrary.Services;

namespace TestProjectxUnit.TestStubs
{
    public class WindowsServiceStub : IWindowsService
    {
        public string GetActiveProcessName()
        {
            return "TestProcess";
        }

        public string GetActiveWindowTitle()
        {
            return "Test Window";
        }
    }
}
