
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.DTOs;

namespace DataAccessLibrary.Services
{
    public interface ICursorlessCheatsheetItemDataService
    {
        Task<List<CursorlessCheatsheetItemDTO>> GetAllCursorlessCheatsheetItemsAsync(bool getFromJson = false);
        Task<List<CursorlessCheatsheetItemDTO>> SearchCursorlessCheatsheetItemsAsync(string serverSearchTerm, bool getFromJson = false);
        Task<CursorlessCheatsheetItemDTO> AddCursorlessCheatsheetItem(CursorlessCheatsheetItemDTO cursorlessCheatsheetItemDTO);
        Task<CursorlessCheatsheetItemDTO> GetCursorlessCheatsheetItemById(int id);
        Task<CursorlessCheatsheetItemDTO> UpdateCursorlessCheatsheetItem(CursorlessCheatsheetItemDTO cursorlessCheatsheetItemDTO, string username);
        Task DeleteCursorlessCheatsheetItem(int id);
        Task<bool> ExportToJsonAsync();
    }
}
