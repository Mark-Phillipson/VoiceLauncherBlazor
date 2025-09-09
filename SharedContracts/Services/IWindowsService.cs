namespace SharedContracts.Services
{
    public interface IWindowsService
    {
        string GetActiveProcessName();
        string GetActiveWindowTitle();
    }
}
