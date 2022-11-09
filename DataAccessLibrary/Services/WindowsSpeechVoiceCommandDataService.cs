using Ardalis.GuardClauses;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLibrary.Repositories;
using DataAccessLibrary.DTO;

namespace DataAccessLibrary.Services
{
    public class WindowsSpeechVoiceCommandDataService : IWindowsSpeechVoiceCommandDataService
    {
        private readonly IWindowsSpeechVoiceCommandRepository _windowsSpeechVoiceCommandRepository;

        public WindowsSpeechVoiceCommandDataService(IWindowsSpeechVoiceCommandRepository windowsSpeechVoiceCommandRepository)
        {
            _windowsSpeechVoiceCommandRepository = windowsSpeechVoiceCommandRepository;
        }
        public async Task<List<WindowsSpeechVoiceCommandDTO>> GetAllWindowsSpeechVoiceCommandsAsync(bool showAutoCreated,int maxRows= 16)
        {
            var WindowsSpeechVoiceCommands = await _windowsSpeechVoiceCommandRepository.GetAllWindowsSpeechVoiceCommandsAsync(maxRows, showAutoCreated);
            return WindowsSpeechVoiceCommands.ToList();
        }
        public async Task<List<WindowsSpeechVoiceCommandDTO>> SearchWindowsSpeechVoiceCommandsAsync(string serverSearchTerm)
        {
            var WindowsSpeechVoiceCommands = await _windowsSpeechVoiceCommandRepository.SearchWindowsSpeechVoiceCommandsAsync(serverSearchTerm);
            return WindowsSpeechVoiceCommands.ToList();
        }

        public async Task<WindowsSpeechVoiceCommandDTO> GetWindowsSpeechVoiceCommandById(int Id)
        {
            var windowsSpeechVoiceCommand = await _windowsSpeechVoiceCommandRepository.GetWindowsSpeechVoiceCommandByIdAsync(Id);
            return windowsSpeechVoiceCommand;
        }
        public async Task<WindowsSpeechVoiceCommandDTO> AddWindowsSpeechVoiceCommand(WindowsSpeechVoiceCommandDTO windowsSpeechVoiceCommandDTO)
        {
            Guard.Against.Null(windowsSpeechVoiceCommandDTO);
            var result = await _windowsSpeechVoiceCommandRepository.AddWindowsSpeechVoiceCommandAsync(windowsSpeechVoiceCommandDTO);
            if (result == null)
            {
                throw new Exception($"Add of windowsSpeechVoiceCommand failed ID: {windowsSpeechVoiceCommandDTO.Id}");
            }
            return result;
        }
        public async Task<WindowsSpeechVoiceCommandDTO> UpdateWindowsSpeechVoiceCommand(WindowsSpeechVoiceCommandDTO windowsSpeechVoiceCommandDTO, string username)
        {
            Guard.Against.Null(windowsSpeechVoiceCommandDTO);
            Guard.Against.Null(username);
            var result = await _windowsSpeechVoiceCommandRepository.UpdateWindowsSpeechVoiceCommandAsync(windowsSpeechVoiceCommandDTO);
            if (result == null)
            {
                throw new Exception($"Update of windowsSpeechVoiceCommand failed ID: {windowsSpeechVoiceCommandDTO.Id}");
            }
            return result;
        }

        public async Task DeleteWindowsSpeechVoiceCommand(int Id)
        {
            await _windowsSpeechVoiceCommandRepository.DeleteWindowsSpeechVoiceCommandAsync(Id);
        }
        public async Task<WindowsSpeechVoiceCommandDTO> GetLatestAdded()
        {
            var result = await _windowsSpeechVoiceCommandRepository.GetLatestAdded();
            return result;
        }
    }
}