using Ardalis.GuardClauses;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceLauncher.Repositories;
using DataAccessLibrary.DTO;

namespace VoiceLauncher.Services
{
    public class CustomWindowsSpeechCommandDataService : ICustomWindowsSpeechCommandDataService
    {
        private readonly ICustomWindowsSpeechCommandRepository _customWindowsSpeechCommandRepository;

        public CustomWindowsSpeechCommandDataService(ICustomWindowsSpeechCommandRepository customWindowsSpeechCommandRepository)
        {
            this._customWindowsSpeechCommandRepository = customWindowsSpeechCommandRepository;
        }
        public async Task<List<CustomWindowsSpeechCommandDTO>> GetAllCustomWindowsSpeechCommandsAsync()
        {
            var CustomWindowsSpeechCommands = await _customWindowsSpeechCommandRepository.GetAllCustomWindowsSpeechCommandsAsync(300);
            return CustomWindowsSpeechCommands.ToList();
        }
        public async Task<List<CustomWindowsSpeechCommandDTO>> SearchCustomWindowsSpeechCommandsAsync(string serverSearchTerm)
        {
            var CustomWindowsSpeechCommands = await _customWindowsSpeechCommandRepository.SearchCustomWindowsSpeechCommandsAsync(serverSearchTerm);
            return CustomWindowsSpeechCommands.ToList();
        }

        public async Task<CustomWindowsSpeechCommandDTO> GetCustomWindowsSpeechCommandById(int Id)
        {
            var customWindowsSpeechCommand = await _customWindowsSpeechCommandRepository.GetCustomWindowsSpeechCommandByIdAsync(Id);
            return customWindowsSpeechCommand;
        }
        public async Task<CustomWindowsSpeechCommandDTO> AddCustomWindowsSpeechCommand(CustomWindowsSpeechCommandDTO customWindowsSpeechCommandDTO)
        {
            Guard.Against.Null(customWindowsSpeechCommandDTO);
            var result = await _customWindowsSpeechCommandRepository.AddCustomWindowsSpeechCommandAsync(customWindowsSpeechCommandDTO);
            if (result == null)
            {
                throw new Exception($"Add of customWindowsSpeechCommand failed ID: {customWindowsSpeechCommandDTO.Id}");
            }
            return result;
        }
        public async Task<CustomWindowsSpeechCommandDTO> UpdateCustomWindowsSpeechCommand(CustomWindowsSpeechCommandDTO customWindowsSpeechCommandDTO, string username)
        {
            Guard.Against.Null(customWindowsSpeechCommandDTO);
            Guard.Against.Null(username);
            var result = await _customWindowsSpeechCommandRepository.UpdateCustomWindowsSpeechCommandAsync(customWindowsSpeechCommandDTO);
            if (result == null)
            {
                throw new Exception($"Update of customWindowsSpeechCommand failed ID: {customWindowsSpeechCommandDTO.Id}");
            }
            return result;
        }

        public async Task DeleteCustomWindowsSpeechCommand(int Id)
        {
            await _customWindowsSpeechCommandRepository.DeleteCustomWindowsSpeechCommandAsync(Id);
        }
    }
}