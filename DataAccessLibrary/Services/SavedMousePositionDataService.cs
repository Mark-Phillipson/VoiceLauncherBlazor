using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;
using Ardalis.GuardClauses;
using DataAccessLibrary.Repositories;
using DataAccessLibrary.DTO;

namespace VoiceLauncher.Services
{
    public class SavedMousePositionDataService : ISavedMousePositionDataService
    {
        private readonly ISavedMousePositionRepository _savedMousePositionRepository;

        public SavedMousePositionDataService(ISavedMousePositionRepository savedMousePositionRepository)
        {
            this._savedMousePositionRepository = savedMousePositionRepository;
        }
        public async Task<List<SavedMousePositionDTO>> GetAllSavedMousePositionsAsync()
        {
            var SavedMousePositions = await _savedMousePositionRepository.GetAllSavedMousePositionsAsync(300);
            return SavedMousePositions.ToList();
        }
        public async Task<SavedMousePositionDTO?> GetSavedMousePositionById(int Id)
        {
            var savedMousePosition = await _savedMousePositionRepository.GetSavedMousePositionByIdAsync(Id);
            return savedMousePosition;
        }
        public async Task<SavedMousePositionDTO?> AddSavedMousePosition(SavedMousePositionDTO savedMousePositionDTO)
        {
            Guard.Against.Null(savedMousePositionDTO);
            var result = await _savedMousePositionRepository.AddSavedMousePositionAsync(savedMousePositionDTO);
            if (result == null)
            {
                throw new Exception($"Add of savedMousePosition failed ID: {savedMousePositionDTO.Id}");
            }
            return result;
        }
        public async Task<SavedMousePositionDTO?> UpdateSavedMousePosition(SavedMousePositionDTO savedMousePositionDTO, string username)
        {
            Guard.Against.Null(savedMousePositionDTO);
            Guard.Against.Null(username);
            var result = await _savedMousePositionRepository.UpdateSavedMousePositionAsync(savedMousePositionDTO);
            if (result == null)
            {
                throw new Exception($"Update of savedMousePosition failed ID: {savedMousePositionDTO.Id}");
            }
            return result;
        }

        public async Task DeleteSavedMousePosition(int Id)
        {
            await _savedMousePositionRepository.DeleteSavedMousePositionAsync(Id);
        }
    }
}