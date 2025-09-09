using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.DTO;

namespace DataAccessLibrary;

public interface ICursorlessCheatsheetItemJsonRepository
{
    IEnumerable<CursorlessCheatsheetItemDTO> GetAllCursorlessCheatsheetItemsAsync(int maxRows = 400);
    Task<IEnumerable<CursorlessCheatsheetItemDTO>> SearchCursorlessCheatsheetItemsAsync(string serverSearchTerm);
    Task<bool> ExportToJsonAsync();
}