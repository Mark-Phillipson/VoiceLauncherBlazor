using DataAccessLibrary.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace VoiceLauncher.Services
{
    public interface IIdiosyncrasyDataService
    {
        Task<List<IdiosyncrasyDTO>> GetAllIdiosyncrasiesAsync( );
        Task<List<IdiosyncrasyDTO>> SearchIdiosyncrasiesAsync(string serverSearchTerm);
        Task<IdiosyncrasyDTO> AddIdiosyncrasy(IdiosyncrasyDTO idiosyncrasyDTO);
        Task<IdiosyncrasyDTO> GetIdiosyncrasyById(int Id);
        Task<IdiosyncrasyDTO> UpdateIdiosyncrasy(IdiosyncrasyDTO idiosyncrasyDTO, string username);
        Task DeleteIdiosyncrasy(int Id);
    }
}
