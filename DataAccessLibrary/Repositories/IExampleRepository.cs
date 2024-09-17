
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DataAccessLibrary.DTOs;

namespace DataAccessLibrary.Repositories
{
    public interface IExampleRepository
    {
        Task<ExampleDTO?> AddExampleAsync(ExampleDTO exampleDTO);
        Task DeleteExampleAsync(int Id);
        Task<IEnumerable<ExampleDTO>> GetAllExamplesAsync(int pageNumber, int pageSize, string? serverSearchTerm);
        Task<IEnumerable<ExampleDTO>> SearchExamplesAsync(string serverSearchTerm);
        Task<ExampleDTO?> GetExampleByIdAsync(int Id);
        Task<ExampleDTO?> UpdateExampleAsync(ExampleDTO exampleDTO);
        Task<int> GetTotalCountAsync();
    }
}