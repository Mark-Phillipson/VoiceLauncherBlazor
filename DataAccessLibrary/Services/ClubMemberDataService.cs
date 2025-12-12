using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models;

namespace DataAccessLibrary.Services
{
    public class ClubMemberDataService : IClubMemberDataService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        public ClubMemberDataService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<ClubMember>> GetAllClubMembersAsync(int maximumRows = 300)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ClubMembers.OrderBy(c => c.FirstName).Take(maximumRows).ToListAsync();
        }

        public async Task<ClubMember?> GetClubMemberAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ClubMembers.Where(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task<string> SaveClubMemberAsync(ClubMember member)
        {
            using var context = _contextFactory.CreateDbContext();
            if (member.Id > 0)
            {
                context.ClubMembers.Update(member);
            }
            else
            {
                context.ClubMembers.Add(member);
            }
            try
            {
                await context.SaveChangesAsync();
                return "Saved Successfully";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> DeleteClubMemberAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var member = await context.ClubMembers.Where(c => c.Id == id).FirstOrDefaultAsync();
            if (member != null)
            {
                context.ClubMembers.Remove(member);
                await context.SaveChangesAsync();
                return "Deleted Successfully";
            }
            return "Record not found";
        }
    }
}
