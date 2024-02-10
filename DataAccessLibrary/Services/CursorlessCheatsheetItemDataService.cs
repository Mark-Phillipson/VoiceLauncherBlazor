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

        public CursorlessCheatsheetItemDataService(ICursorlessCheatsheetItemRepository cursorlessCheatsheetItemRepository)
        {
            this._cursorlessCheatsheetItemRepository = cursorlessCheatsheetItemRepository;
        }
        public async Task<List<CursorlessCheatsheetItemDTO>> GetAllCursorlessCheatsheetItemsAsync()
        {
            var CursorlessCheatsheetItems = await _cursorlessCheatsheetItemRepository.GetAllCursorlessCheatsheetItemsAsync(300);
            return CursorlessCheatsheetItems.ToList();
        }
        public async Task<List<CursorlessCheatsheetItemDTO>> SearchCursorlessCheatsheetItemsAsync(string serverSearchTerm)
        {
            var CursorlessCheatsheetItems = await _cursorlessCheatsheetItemRepository.SearchCursorlessCheatsheetItemsAsync(serverSearchTerm);
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
    }
}