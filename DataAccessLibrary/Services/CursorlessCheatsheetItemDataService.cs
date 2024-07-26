using Ardalis.GuardClauses;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLibrary.Repositories;
using DataAccessLibrary.DTOs;


namespace DataAccessLibrary.Services
{
    public class CursorlessCheatsheetItemDataService : ICursorlessCheatsheetItemDataService
    {
        private readonly ICursorlessCheatsheetItemRepository _cursorlessCheatsheetItemRepository;
        private readonly ICursorlessCheatsheetItemJsonRepository cursorlessCheatsheetItemJsonRepository;

        public CursorlessCheatsheetItemDataService(ICursorlessCheatsheetItemRepository cursorlessCheatsheetItemRepository, ICursorlessCheatsheetItemJsonRepository cursorlessCheatsheetItemJsonRepository)
        {
            this._cursorlessCheatsheetItemRepository = cursorlessCheatsheetItemRepository;
            this.cursorlessCheatsheetItemJsonRepository = cursorlessCheatsheetItemJsonRepository;
        }
        public async Task<List<CursorlessCheatsheetItemDTO>> GetAllCursorlessCheatsheetItemsAsync(bool getFromJson = false)
        {

            IEnumerable<CursorlessCheatsheetItemDTO> CursorlessCheatsheetItems;
            if (getFromJson)
            {
                CursorlessCheatsheetItems = cursorlessCheatsheetItemJsonRepository.GetAllCursorlessCheatsheetItemsAsync(300);
                return CursorlessCheatsheetItems.ToList();
            }
            CursorlessCheatsheetItems = await _cursorlessCheatsheetItemRepository.GetAllCursorlessCheatsheetItemsAsync(300);
            return CursorlessCheatsheetItems.ToList();
        }
        public async Task<List<CursorlessCheatsheetItemDTO>> SearchCursorlessCheatsheetItemsAsync(string serverSearchTerm, bool getFromJson = false)
        {
            IEnumerable<CursorlessCheatsheetItemDTO> CursorlessCheatsheetItems;
            if (getFromJson)
            {
                CursorlessCheatsheetItems = await cursorlessCheatsheetItemJsonRepository.SearchCursorlessCheatsheetItemsAsync(serverSearchTerm);
                return CursorlessCheatsheetItems.ToList();
            }
            CursorlessCheatsheetItems = await _cursorlessCheatsheetItemRepository.SearchCursorlessCheatsheetItemsAsync(serverSearchTerm);
            return CursorlessCheatsheetItems.ToList();
        }

        public async Task<CursorlessCheatsheetItemDTO> GetCursorlessCheatsheetItemById(int id)
        {
            var cursorlessCheatsheetItem = await _cursorlessCheatsheetItemRepository.GetCursorlessCheatsheetItemByIdAsync(id);
            return cursorlessCheatsheetItem;
        }
        public async Task<CursorlessCheatsheetItemDTO> AddCursorlessCheatsheetItem(CursorlessCheatsheetItemDTO cursorlessCheatsheetItemDTO)
        {
            Guard.Against.Null(cursorlessCheatsheetItemDTO);
            var result = await _cursorlessCheatsheetItemRepository.AddCursorlessCheatsheetItemAsync(cursorlessCheatsheetItemDTO);
            if (result == null)
            {
                throw new Exception($"Add of cursorlessCheatsheetItem failed ID: {cursorlessCheatsheetItemDTO.Id}");
            }
            return result;
        }
        public async Task<CursorlessCheatsheetItemDTO> UpdateCursorlessCheatsheetItem(CursorlessCheatsheetItemDTO cursorlessCheatsheetItemDTO, string username)
        {
            Guard.Against.Null(cursorlessCheatsheetItemDTO);
            Guard.Against.Null(username);
            var result = await _cursorlessCheatsheetItemRepository.UpdateCursorlessCheatsheetItemAsync(cursorlessCheatsheetItemDTO);
            if (result == null)
            {
                throw new Exception($"Update of cursorlessCheatsheetItem failed ID: {cursorlessCheatsheetItemDTO.Id}");
            }
            return result;
        }

        public async Task DeleteCursorlessCheatsheetItem(int id)
        {
            await _cursorlessCheatsheetItemRepository.DeleteCursorlessCheatsheetItemAsync(id);
        }

        public Task<bool> ExportToJsonAsync()
        {
            var result = cursorlessCheatsheetItemJsonRepository.ExportToJsonAsync();
            return result;
        }
    }
}