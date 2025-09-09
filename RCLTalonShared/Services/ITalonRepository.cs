using RCLTalonShared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RCLTalonShared.Services
{
    public interface ITalonRepository
    {
        Task SaveCommandsAsync(IEnumerable<TalonVoiceCommand> commands);
        Task<IEnumerable<TalonVoiceCommand>> GetCommandsAsync();
        Task SaveListsAsync(IEnumerable<TalonList> lists);
        Task<IEnumerable<TalonList>> GetListsAsync();
        Task DeleteAllAsync();
        Task<string> ExportAllJsonAsync();
        Task ImportFromJsonAsync(string json);
    }
}
