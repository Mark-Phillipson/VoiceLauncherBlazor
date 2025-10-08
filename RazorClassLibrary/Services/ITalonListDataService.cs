using DataAccessLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RazorClassLibrary.Services
{
    public interface ITalonListDataService
    {
        Task<IEnumerable<TalonList>> GetAllTalonListsAsync();
    }
}
