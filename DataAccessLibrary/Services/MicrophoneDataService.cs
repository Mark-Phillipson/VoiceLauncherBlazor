using Ardalis.GuardClauses;
using AutoMapper;
using DataAccessLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceLauncher.Repositories;


namespace VoiceLauncher.Services
{
    public class MicrophoneDataService : IMicrophoneDataService
    {
        private readonly IMicrophoneRepository _microphoneRepository;

        public MicrophoneDataService(IMicrophoneRepository microphoneRepository)
        {
            _microphoneRepository = microphoneRepository;
        }
        public async Task<List<MicrophoneDTO>> GetAllMicrophonesAsync()
        {
            var Microphones = await _microphoneRepository.GetAllMicrophonesAsync(300);
            return Microphones.ToList();
        }
        public async Task<List<MicrophoneDTO>> SearchMicrophonesAsync(string serverSearchTerm)
        {
            var Microphones = await _microphoneRepository.SearchMicrophonesAsync(serverSearchTerm);
            return Microphones.ToList();
        }

        public async Task<MicrophoneDTO?> GetMicrophoneById(int Id)
        {
            var microphone = await _microphoneRepository.GetMicrophoneByIdAsync(Id);
            return microphone;
        }
        public async Task<MicrophoneDTO?> AddMicrophone(MicrophoneDTO microphoneDTO)
        {
            Guard.Against.Null(microphoneDTO);
            var result = await _microphoneRepository.AddMicrophoneAsync(microphoneDTO);
            if (result == null)
            {
                throw new Exception($"Add of microphone failed ID: {microphoneDTO.Id}");
            }
            return result;
        }
        public async Task<MicrophoneDTO?> UpdateMicrophone(MicrophoneDTO microphoneDTO, string username)
        {
            Guard.Against.Null(microphoneDTO);
            Guard.Against.Null(username);
            var result = await _microphoneRepository.UpdateMicrophoneAsync(microphoneDTO);
            if (result == null)
            {
                throw new Exception($"Update of microphone failed ID: {microphoneDTO.Id}");
            }
            return result;
        }

        public async Task DeleteMicrophone(int Id)
        {
            await _microphoneRepository.DeleteMicrophoneAsync(Id);
        }
    }
}