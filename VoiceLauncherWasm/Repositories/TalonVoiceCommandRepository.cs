using VoiceLauncherWasm.Models;
using VoiceLauncherWasm.Services.IndexedDB;

namespace VoiceLauncherWasm.Repositories
{
    public class TalonVoiceCommandRepository : ITalonVoiceCommandRepository
    {
        private readonly IIndexedDBService _indexedDB;
        private const string STORE_NAME = "TalonVoiceCommands";

        public TalonVoiceCommandRepository(IIndexedDBService indexedDB)
        {
            _indexedDB = indexedDB;
        }

        public async Task<IEnumerable<TalonVoiceCommand>> GetAllAsync()
        {
            var commands = await _indexedDB.GetAllAsync<TalonVoiceCommand>(STORE_NAME);
            return commands.OrderByDescending(c => c.CreatedAt);
        }

        public async Task<TalonVoiceCommand?> GetByIdAsync(int id)
        {
            return await _indexedDB.GetByIdAsync<TalonVoiceCommand>(STORE_NAME, id);
        }

        public async Task<int> AddAsync(TalonVoiceCommand command)
        {
            command.CreatedAt = DateTime.UtcNow;
            var result = await _indexedDB.AddAsync(STORE_NAME, command);
            return Convert.ToInt32(result);
        }

        public async Task UpdateAsync(TalonVoiceCommand command)
        {
            await _indexedDB.UpdateAsync(STORE_NAME, command);
        }

        public async Task DeleteAsync(int id)
        {
            await _indexedDB.DeleteAsync(STORE_NAME, id);
        }

        public async Task ClearAllAsync()
        {
            await _indexedDB.ClearAsync(STORE_NAME);
        }

        public async Task<IEnumerable<TalonVoiceCommand>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var allCommands = await _indexedDB.GetAllAsync<TalonVoiceCommand>(STORE_NAME);
            var lowerTerm = searchTerm.ToLower();

            return allCommands.Where(c =>
                c.Command.ToLower().Contains(lowerTerm) ||
                c.Script.ToLower().Contains(lowerTerm) ||
                c.Application.ToLower().Contains(lowerTerm) ||
                (c.Mode != null && c.Mode.ToLower().Contains(lowerTerm)) ||
                (c.Title != null && c.Title.ToLower().Contains(lowerTerm)) ||
                (c.Repository != null && c.Repository.ToLower().Contains(lowerTerm))
            ).OrderByDescending(c => c.CreatedAt);
        }

        public async Task<IEnumerable<TalonVoiceCommand>> SearchCommandNamesOnlyAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var allCommands = await _indexedDB.GetAllAsync<TalonVoiceCommand>(STORE_NAME);
            var lowerTerm = searchTerm.ToLower();

            return allCommands.Where(c =>
                c.Command.ToLower().Contains(lowerTerm)
            ).OrderByDescending(c => c.CreatedAt);
        }

        public async Task<IEnumerable<TalonVoiceCommand>> SearchScriptOnlyAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var allCommands = await _indexedDB.GetAllAsync<TalonVoiceCommand>(STORE_NAME);
            var lowerTerm = searchTerm.ToLower();

            return allCommands.Where(c =>
                c.Script.ToLower().Contains(lowerTerm)
            ).OrderByDescending(c => c.CreatedAt);
        }

        public async Task<int> GetCountAsync()
        {
            return await _indexedDB.CountAsync(STORE_NAME);
        }

        public async Task<int> ImportCommandsAsync(IEnumerable<TalonVoiceCommand> commands)
        {
            // Clear existing commands first
            await ClearAllAsync();

            int count = 0;
            foreach (var command in commands)
            {
                await AddAsync(command);
                count++;
            }

            return count;
        }
    }
}