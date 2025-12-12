using DataAccessLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.Services
{
    public interface IClubMemberDataService
    {
        Task<List<ClubMember>> GetAllClubMembersAsync(int maximumRows = 300);
        Task<ClubMember?> GetClubMemberAsync(int id);
        Task<string> SaveClubMemberAsync(ClubMember member);
        Task<string> DeleteClubMemberAsync(int id);
    }
}
