
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using DataAccessLibrary.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;
using DataAccessLibrary.DTO;

namespace DataAccessLibrary.Repositories
{
    public class SavedMousePositionRepository : ISavedMousePositionRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;
        private readonly ILoggerFactory _loggerFactory;

        public SavedMousePositionRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper, ILoggerFactory loggerFactory)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _loggerFactory = loggerFactory;
        }
        public async Task<IEnumerable<SavedMousePositionDTO>> GetAllSavedMousePositionsAsync(int maxRows = 400)
        {
            using var context = _contextFactory.CreateDbContext();
            List<SavedMousePosition> savedMousePositions;
            try
            {
                savedMousePositions = await context.SavedMousePosition
            //.Where(v => v.?==?)
            //.OrderBy(v => v.?)
            .Take(maxRows)
            .ToListAsync();

            }
            catch (Exception exception)
            {
                var logger = _loggerFactory.CreateLogger<SavedMousePositionRepository>();
                logger.LogError(exception.Message);
                throw;
            }
            IEnumerable<SavedMousePositionDTO> SavedMousePositionsDTO = _mapper.Map<List<SavedMousePosition>, IEnumerable<SavedMousePositionDTO>>(savedMousePositions);
            return SavedMousePositionsDTO;
        }

        public async Task<SavedMousePositionDTO> GetSavedMousePositionByIdAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result = await context.SavedMousePosition.AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == Id);
            if (result == null) return null;
            SavedMousePositionDTO savedMousePositionDTO = _mapper.Map<SavedMousePosition, SavedMousePositionDTO>(result);
            return savedMousePositionDTO;
        }

        public async Task<SavedMousePositionDTO> AddSavedMousePositionAsync(SavedMousePositionDTO savedMousePositionDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            SavedMousePosition savedMousePosition = _mapper.Map<SavedMousePositionDTO, SavedMousePosition>(savedMousePositionDTO);
            var addedEntity = context.SavedMousePosition.Add(savedMousePosition);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            SavedMousePositionDTO resultDTO = _mapper.Map<SavedMousePosition, SavedMousePositionDTO>(savedMousePosition);
            return resultDTO;
        }

        public async Task<SavedMousePositionDTO> UpdateSavedMousePositionAsync(SavedMousePositionDTO savedMousePositionDTO)
        {
            SavedMousePosition savedMousePosition = _mapper.Map<SavedMousePositionDTO, SavedMousePosition>(savedMousePositionDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundSavedMousePosition = await context.SavedMousePosition.AsNoTracking().FirstOrDefaultAsync(e => e.Id == savedMousePosition.Id);

                if (foundSavedMousePosition != null)
                {
                    var mappedSavedMousePosition = _mapper.Map<SavedMousePosition>(savedMousePosition);
                    context.SavedMousePosition.Update(mappedSavedMousePosition);
                    await context.SaveChangesAsync();
                    SavedMousePositionDTO resultDTO = _mapper.Map<SavedMousePosition, SavedMousePositionDTO>(mappedSavedMousePosition);
                    return resultDTO;
                }
            }
            return null;
        }
        public async Task DeleteSavedMousePositionAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundSavedMousePosition = context.SavedMousePosition.FirstOrDefault(e => e.Id == Id);
            if (foundSavedMousePosition == null)
            {
                return;
            }
            context.SavedMousePosition.Remove(foundSavedMousePosition);
            await context.SaveChangesAsync();
        }
    }
}