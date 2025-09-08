using System.Threading.Tasks;
using System.Collections.Generic;

namespace DataAccessLibrary.Services
{
    public interface ITalonVoiceCommandDataService
    {
        Task<int> ImportFromTalonFilesAsync(string rootFolder);
    Task<List<DataAccessLibrary.Models.TalonVoiceCommand>> GetAllCommandsForFiltersAsync();
    }
}
