using System.Threading.Tasks;

namespace DataAccessLibrary.Services
{
    public interface ITalonVoiceCommandDataService
    {
        Task<int> ImportFromTalonFilesAsync(string rootFolder);
    }
}
