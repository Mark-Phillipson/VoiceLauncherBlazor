using DataAccessLibrary.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace VoiceLauncher.Repositories
{
    public interface IIdiosyncrasyRepository
    {
        Task<IdiosyncrasyDTO?> AddIdiosyncrasyAsync(IdiosyncrasyDTO idiosyncrasyDTO);
        Task DeleteIdiosyncrasyAsync(int Id);
        Task<IEnumerable<IdiosyncrasyDTO>> GetAllIdiosyncrasiesAsync(int maxRows);
        Task<IEnumerable<IdiosyncrasyDTO>> SearchIdiosyncrasiesAsync(string serverSearchTerm);
        Task<IdiosyncrasyDTO?> GetIdiosyncrasyByIdAsync(int Id);
        Task<IdiosyncrasyDTO?> UpdateIdiosyncrasyAsync(IdiosyncrasyDTO idiosyncrasyDTO);
    }
}