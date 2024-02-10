
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using DataAccessLibrary.DTOs;
using DataAccessLibrary.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DataAccessLibrary.Repositories
{
    public class CursorlessCheatsheetItemRepository : ICursorlessCheatsheetItemRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;

        public CursorlessCheatsheetItemRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory;
            this._mapper = mapper;
        }
        public async Task<IEnumerable<CursorlessCheatsheetItemDTO>> GetAllCursorlessCheatsheetItemsAsync(int maxRows = 400)
        {
            using var context = _contextFactory.CreateDbContext();
            var CursorlessCheatsheetItems = await context.CursorlessCheatsheetItems
                //.Where(v => v.?==?)
                //.OrderBy(v => v.?)
                .Take(maxRows)
                .ToListAsync();
            IEnumerable<CursorlessCheatsheetItemDTO> CursorlessCheatsheetItemsDTO = _mapper.Map<List<CursorlessCheatsheetItem>, IEnumerable<CursorlessCheatsheetItemDTO>>(CursorlessCheatsheetItems);
            return CursorlessCheatsheetItemsDTO;
        }
        public async Task<IEnumerable<CursorlessCheatsheetItemDTO>> SearchCursorlessCheatsheetItemsAsync(string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var CursorlessCheatsheetItems = await context.CursorlessCheatsheetItems
                //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //)
                //.OrderBy(v => v.?)
                .Take(1000)
                .ToListAsync();
            IEnumerable<CursorlessCheatsheetItemDTO> CursorlessCheatsheetItemsDTO = _mapper.Map<List<CursorlessCheatsheetItem>, IEnumerable<CursorlessCheatsheetItemDTO>>(CursorlessCheatsheetItems);
            return CursorlessCheatsheetItemsDTO;
        }

        public async Task<CursorlessCheatsheetItemDTO> GetCursorlessCheatsheetItemByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result = await context.CursorlessCheatsheetItems.AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == id);
            if (result == null) return null;
            CursorlessCheatsheetItemDTO cursorlessCheatsheetItemDTO = _mapper.Map<CursorlessCheatsheetItem, CursorlessCheatsheetItemDTO>(result);
            return cursorlessCheatsheetItemDTO;
        }

        public async Task<CursorlessCheatsheetItemDTO> AddCursorlessCheatsheetItemAsync(CursorlessCheatsheetItemDTO cursorlessCheatsheetItemDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            CursorlessCheatsheetItem cursorlessCheatsheetItem = _mapper.Map<CursorlessCheatsheetItemDTO, CursorlessCheatsheetItem>(cursorlessCheatsheetItemDTO);
            var addedEntity = context.CursorlessCheatsheetItems.Add(cursorlessCheatsheetItem);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            CursorlessCheatsheetItemDTO resultDTO = _mapper.Map<CursorlessCheatsheetItem, CursorlessCheatsheetItemDTO>(cursorlessCheatsheetItem);
            return resultDTO;
        }

        public async Task<CursorlessCheatsheetItemDTO> UpdateCursorlessCheatsheetItemAsync(CursorlessCheatsheetItemDTO cursorlessCheatsheetItemDTO)
        {
            CursorlessCheatsheetItem cursorlessCheatsheetItem = _mapper.Map<CursorlessCheatsheetItemDTO, CursorlessCheatsheetItem>(cursorlessCheatsheetItemDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundCursorlessCheatsheetItem = await context.CursorlessCheatsheetItems.AsNoTracking().FirstOrDefaultAsync(e => e.Id == cursorlessCheatsheetItem.Id);

                if (foundCursorlessCheatsheetItem != null)
                {
                    var mappedCursorlessCheatsheetItem = _mapper.Map<CursorlessCheatsheetItem>(cursorlessCheatsheetItem);
                    context.CursorlessCheatsheetItems.Update(mappedCursorlessCheatsheetItem);
                    await context.SaveChangesAsync();
                    CursorlessCheatsheetItemDTO resultDTO = _mapper.Map<CursorlessCheatsheetItem, CursorlessCheatsheetItemDTO>(mappedCursorlessCheatsheetItem);
                    return resultDTO;
                }
            }
            return null;
        }
        public async Task DeleteCursorlessCheatsheetItemAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundCursorlessCheatsheetItem = context.CursorlessCheatsheetItems.FirstOrDefault(e => e.Id == id);
            if (foundCursorlessCheatsheetItem == null)
            {
                return;
            }
            context.CursorlessCheatsheetItems.Remove(foundCursorlessCheatsheetItem);
            await context.SaveChangesAsync();
        }
    }
}