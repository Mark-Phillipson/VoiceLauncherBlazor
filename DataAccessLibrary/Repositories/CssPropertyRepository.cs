
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using DataAccessLibrary.Models;
using DataAccessLibrary.DTO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DataAccessLibrary.Repositories
{
    public class CssPropertyRepository : ICssPropertyRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;

        public CssPropertyRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory;
            this._mapper = mapper;
        }
        public async Task<IEnumerable<CssPropertyDTO>> GetAllCssPropertiesAsync(int pageNumber, int pageSize, string? serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            List<CssProperty> CssProperties;
            if (!string.IsNullOrWhiteSpace(serverSearchTerm))
            {
                CssProperties = await context.CssProperties
                                        .Where(v =>
                    (v.PropertyName != null && v.PropertyName.ToLower().Contains(serverSearchTerm))
                     || (v.Description != null && v.Description.ToLower().Contains(serverSearchTerm))
                    )

                    //.OrderBy(v => v.?)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            else
            {
                CssProperties = await context.CssProperties
                    //.OrderBy(v => v.?)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            IEnumerable<CssPropertyDTO> CssPropertiesDTO = _mapper.Map<List<CssProperty>, IEnumerable<CssPropertyDTO>>(CssProperties);
            return CssPropertiesDTO;
        }
        public async Task<IEnumerable<CssPropertyDTO>> SearchCssPropertiesAsync(string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var CssProperties = await context.CssProperties
                //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //)
                //.OrderBy(v => v.?)
                .Take(1000)
                .ToListAsync();
            IEnumerable<CssPropertyDTO> CssPropertiesDTO = _mapper.Map<List<CssProperty>, IEnumerable<CssPropertyDTO>>(CssProperties);
            return CssPropertiesDTO;
        }

        public async Task<CssPropertyDTO?> GetCssPropertyByIdAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result = await context.CssProperties.AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == Id);
            if (result == null) return null;
            CssPropertyDTO cssPropertyDTO = _mapper.Map<CssProperty, CssPropertyDTO>(result);
            return cssPropertyDTO;
        }

        public async Task<CssPropertyDTO?> AddCssPropertyAsync(CssPropertyDTO cssPropertyDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            CssProperty cssProperty = _mapper.Map<CssPropertyDTO, CssProperty>(cssPropertyDTO);
            var addedEntity = context.CssProperties.Add(cssProperty);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            CssPropertyDTO resultDTO = _mapper.Map<CssProperty, CssPropertyDTO>(cssProperty);
            return resultDTO;
        }

        public async Task<CssPropertyDTO?> UpdateCssPropertyAsync(CssPropertyDTO cssPropertyDTO)
        {
            CssProperty cssProperty = _mapper.Map<CssPropertyDTO, CssProperty>(cssPropertyDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundCssProperty = await context.CssProperties.AsNoTracking().FirstOrDefaultAsync(e => e.Id == cssProperty.Id);

                if (foundCssProperty != null)
                {
                    var mappedCssProperty = _mapper.Map<CssProperty>(cssProperty);
                    context.CssProperties.Update(mappedCssProperty);
                    await context.SaveChangesAsync();
                    CssPropertyDTO resultDTO = _mapper.Map<CssProperty, CssPropertyDTO>(mappedCssProperty);
                    return resultDTO;
                }
            }
            return null;
        }
        public async Task DeleteCssPropertyAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundCssProperty = context.CssProperties.FirstOrDefault(e => e.Id == Id);
            if (foundCssProperty == null)
            {
                return;
            }
            context.CssProperties.Remove(foundCssProperty);
            await context.SaveChangesAsync();
        }
        public async Task<int> GetTotalCountAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.CssProperties.CountAsync();
        }

    }
}