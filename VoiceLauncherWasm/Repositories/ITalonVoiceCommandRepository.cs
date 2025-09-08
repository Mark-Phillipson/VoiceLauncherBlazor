using VoiceLauncherWasm.Models;

namespace VoiceLauncherWasm.Repositories
{
    public interface ITalonVoiceCommandRepository
    {
        Task<IEnumerable<TalonVoiceCommand>> GetAllAsync();
        Task<TalonVoiceCommand?> GetByIdAsync(int id);
        Task<int> AddAsync(TalonVoiceCommand command);
        Task UpdateAsync(TalonVoiceCommand command);
        Task DeleteAsync(int id);
        Task ClearAllAsync();
        Task<IEnumerable<TalonVoiceCommand>> SearchAsync(string searchTerm);
        Task<IEnumerable<TalonVoiceCommand>> SearchCommandNamesOnlyAsync(string searchTerm);
        Task<IEnumerable<TalonVoiceCommand>> SearchScriptOnlyAsync(string searchTerm);
        Task<int> GetCountAsync();
        Task<int> ImportCommandsAsync(IEnumerable<TalonVoiceCommand> commands);
    }
}