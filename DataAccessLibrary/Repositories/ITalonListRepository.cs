using DataAccessLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.Repositories
{
    public interface ITalonListRepository
    {
        Task<IEnumerable<TalonList>> GetAllTalonListsAsync();
    }
}
