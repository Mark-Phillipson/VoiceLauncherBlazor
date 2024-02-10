
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.DTOs;

namespace DataAccessLibrary.Services
{
    public interface ICursorlessCheatsheetItemDataService
    {
        Task<List<CursorlessCheatsheetItemDTO>> GetAllCursorlessCheatsheetItemsAsync();
        Task<List<CursorlessCheatsheetItemDTO>> SearchCursorlessCheatsheetItemsAsync(string serverSearchTerm);
        Task<CursorlessCheatsheetItemDTO> AddCursorlessCheatsheetItem(CursorlessCheatsheetItemDTO cursorlessCheatsheetItemDTO);
        Task<CursorlessCheatsheetItemDTO> GetCursorlessCheatsheetItemById(int id);
        Task<CursorlessCheatsheetItemDTO> UpdateCursorlessCheatsheetItem(CursorlessCheatsheetItemDTO cursorlessCheatsheetItemDTO, string username);
        Task DeleteCursorlessCheatsheetItem(int id);
    }
}
