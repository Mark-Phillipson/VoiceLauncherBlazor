using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public interface ISqlDataAccess
    {
        Task<List<T>> LoadData<T, U>(string sql, U parameters);
        Task<T> LoadSingleData<T, U>(string sql, U parameters);
        Task SaveData<T>(string sql, T parameters);

        string ConnectionStringName { get; set; }
    }
}
