
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.DTOs;

namespace DataAccessLibrary.Repositories
{
    public interface ICursorlessCheatsheetItemRepository
    {
        Task<CursorlessCheatsheetItemDTO> AddCursorlessCheatsheetItemAsync(CursorlessCheatsheetItemDTO cursorlessCheatsheetItemDTO);
        Task DeleteCursorlessCheatsheetItemAsync(int id);
        Task<IEnumerable<CursorlessCheatsheetItemDTO>> GetAllCursorlessCheatsheetItemsAsync(int maxRows);
        Task<IEnumerable<CursorlessCheatsheetItemDTO>> SearchCursorlessCheatsheetItemsAsync(string serverSearchTerm);
        Task<CursorlessCheatsheetItemDTO> GetCursorlessCheatsheetItemByIdAsync(int id);
        Task<CursorlessCheatsheetItemDTO> UpdateCursorlessCheatsheetItemAsync(CursorlessCheatsheetItemDTO cursorlessCheatsheetItemDTO);
    }
}