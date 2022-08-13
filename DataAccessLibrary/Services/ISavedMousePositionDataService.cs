
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.DTO;

namespace VoiceLauncher.Services
{
    public interface ISavedMousePositionDataService
    {
        Task<List<SavedMousePositionDTO>> GetAllSavedMousePositionsAsync( );
        Task<SavedMousePositionDTO> AddSavedMousePosition(SavedMousePositionDTO savedMousePositionDTO);
        Task<SavedMousePositionDTO> GetSavedMousePositionById(int Id);
        Task<SavedMousePositionDTO> UpdateSavedMousePosition(SavedMousePositionDTO savedMousePositionDTO, string username);
        Task DeleteSavedMousePosition(int Id);
    }
}
