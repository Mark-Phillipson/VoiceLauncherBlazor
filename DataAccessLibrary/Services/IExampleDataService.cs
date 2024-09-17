
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.DTOs;

namespace DataAccessLibrary.Services
{
    public interface IExampleDataService
    {
        Task<List<ExampleDTO>> GetAllExamplesAsync(int pageNumber, int pageSize, string? serverSearchTerm);
        Task<List<ExampleDTO>> SearchExamplesAsync(string serverSearchTerm);
        Task<ExampleDTO?> AddExample(ExampleDTO exampleDTO);
        Task<ExampleDTO?> GetExampleById(int Id);
        Task<ExampleDTO> UpdateExample(ExampleDTO exampleDTO, string? username);
        Task DeleteExample(int Id);
        Task<int> GetTotalCount();
    }
}