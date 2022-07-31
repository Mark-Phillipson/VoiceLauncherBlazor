
using System.Collections.Generic;
using System.Threading.Tasks;

using VoiceLauncher.DTOs;

namespace DataAccessLibrary.Repositories
{
    public interface ISavedMousePositionRepository
    {
        Task<SavedMousePositionDTO> AddSavedMousePositionAsync(SavedMousePositionDTO savedMousePositionDTO);
        Task DeleteSavedMousePositionAsync(int Id);
        Task<IEnumerable<SavedMousePositionDTO>> GetAllSavedMousePositionsAsync(int maxRows);
        Task<SavedMousePositionDTO> GetSavedMousePositionByIdAsync(int Id);
        Task<SavedMousePositionDTO> UpdateSavedMousePositionAsync(SavedMousePositionDTO savedMousePositionDTO);
    }
}