using Ardalis.GuardClauses;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLibrary.Repositories;
using DataAccessLibrary.DTO;


namespace DataAccessLibrary.Services
{
    public class CssPropertyDataService : ICssPropertyDataService
    {
        private readonly ICssPropertyRepository _cssPropertyRepository;

        public CssPropertyDataService(ICssPropertyRepository cssPropertyRepository)
        {
            this._cssPropertyRepository = cssPropertyRepository;
        }
        public async Task<List<CssPropertyDTO>> GetAllCssPropertiesAsync(int pageNumber, int pageSize, string serverSearchTerm)
        {
            var CssProperties = await _cssPropertyRepository.GetAllCssPropertiesAsync(pageNumber, pageSize, serverSearchTerm);
            return CssProperties.ToList();
        }
        public async Task<List<CssPropertyDTO>> SearchCssPropertiesAsync(string serverSearchTerm)
        {
            var CssProperties = await _cssPropertyRepository.SearchCssPropertiesAsync(serverSearchTerm);
            return CssProperties.ToList();
        }

        public async Task<CssPropertyDTO?> GetCssPropertyById(int Id)
        {
            var cssProperty = await _cssPropertyRepository.GetCssPropertyByIdAsync(Id);
            return cssProperty;
        }
        public async Task<CssPropertyDTO?> AddCssProperty(CssPropertyDTO cssPropertyDTO)
        {
            Guard.Against.Null(cssPropertyDTO);
            var result = await _cssPropertyRepository.AddCssPropertyAsync(cssPropertyDTO);
            if (result == null)
            {
                throw new Exception($"Add of cssProperty failed ID: {cssPropertyDTO.Id}");
            }
            return result;
        }
        public async Task<CssPropertyDTO?> UpdateCssProperty(CssPropertyDTO cssPropertyDTO, string username)
        {
            Guard.Against.Null(cssPropertyDTO);
            Guard.Against.Null(username);
            var result = await _cssPropertyRepository.UpdateCssPropertyAsync(cssPropertyDTO);
            if (result == null)
            {
                throw new Exception($"Update of cssProperty failed ID: {cssPropertyDTO.Id}");
            }
            return result;
        }

        public async Task DeleteCssProperty(int Id)
        {
            await _cssPropertyRepository.DeleteCssPropertyAsync(Id);
        }
        public async Task<int> GetTotalCount()
        {
            int result = await _cssPropertyRepository.GetTotalCountAsync();
            return result;
        }
    }
}