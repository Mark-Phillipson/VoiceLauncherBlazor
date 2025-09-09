using SharedContracts.Services;

namespace VoiceLauncherWasm.Services.TestStubs
{
    public class WindowsServiceStub : IWindowsService
    {
        public string GetActiveProcessName() => string.Empty;
        public string GetActiveWindowTitle() => string.Empty;
    }
}
