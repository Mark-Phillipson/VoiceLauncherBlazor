
using System.Collections.Generic;
using System.Threading.Tasks;

using VoiceLauncher.DTOs;

namespace VoiceLauncher.Repositories
{
    public interface IIdiosyncrasyRepository
    {
        Task<IdiosyncrasyDTO> AddIdiosyncrasyAsync(IdiosyncrasyDTO idiosyncrasyDTO);
        Task DeleteIdiosyncrasyAsync(int Id);
        Task<IEnumerable<IdiosyncrasyDTO>> GetAllIdiosyncrasiesAsync(int maxRows);
        Task<IEnumerable<IdiosyncrasyDTO>> SearchIdiosyncrasiesAsync(string serverSearchTerm);
        Task<IdiosyncrasyDTO> GetIdiosyncrasyByIdAsync(int Id);
        Task<IdiosyncrasyDTO> UpdateIdiosyncrasyAsync(IdiosyncrasyDTO idiosyncrasyDTO);
    }
}