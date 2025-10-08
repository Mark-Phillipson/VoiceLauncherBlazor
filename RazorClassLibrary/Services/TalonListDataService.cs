using DataAccessLibrary.Models;
using DataAccessLibrary.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RazorClassLibrary.Services
{
    public class TalonListDataService : ITalonListDataService
    {
        private readonly ITalonListRepository _repository;
        public TalonListDataService(ITalonListRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TalonList>> GetAllTalonListsAsync()
        {
            return await _repository.GetAllTalonListsAsync();
        }
    }
}
