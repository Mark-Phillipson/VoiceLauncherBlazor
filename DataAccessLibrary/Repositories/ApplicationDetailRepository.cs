
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using DataAccessLibrary.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using DataAccessLibrary.DTO;

namespace VoiceLauncher.Repositories
{
    public class ApplicationDetailRepository : IApplicationDetailRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;

        public ApplicationDetailRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
        }
        public async Task<IEnumerable<ApplicationDetailDTO>> GetAllApplicationDetailsAsync(int maxRows = 400)
        {
            using var context = _contextFactory.CreateDbContext();
            var ApplicationDetails = await context.ApplicationDetails
                //.Where(v => v.?==?)
                //.OrderBy(v => v.?)
                .Take(maxRows)
                .ToListAsync();
            IEnumerable<ApplicationDetailDTO> ApplicationDetailsDTO = _mapper.Map<List<ApplicationDetail>, IEnumerable<ApplicationDetailDTO>>(ApplicationDetails);
            return ApplicationDetailsDTO;
        }
        public async Task<IEnumerable<ApplicationDetailDTO>> SearchApplicationDetailsAsync(string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var ApplicationDetails = await context.ApplicationDetails
                //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //)
                //.OrderBy(v => v.?)
                .Take(1000)
                .ToListAsync();
            IEnumerable<ApplicationDetailDTO> ApplicationDetailsDTO = _mapper.Map<List<ApplicationDetail>, IEnumerable<ApplicationDetailDTO>>(ApplicationDetails);
            return ApplicationDetailsDTO;
        }

        public async Task<ApplicationDetailDTO?> GetApplicationDetailByIdAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result = await context.ApplicationDetails.AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == Id);
            if (result == null) return new ApplicationDetailDTO();
            ApplicationDetailDTO applicationDetailDTO = _mapper.Map<ApplicationDetail, ApplicationDetailDTO>(result);
            return applicationDetailDTO;
        }

        public async Task<ApplicationDetailDTO?> AddApplicationDetailAsync(ApplicationDetailDTO applicationDetailDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            ApplicationDetail applicationDetail = _mapper.Map<ApplicationDetailDTO, ApplicationDetail>(applicationDetailDTO);
            var addedEntity = context.ApplicationDetails.Add(applicationDetail);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return new ApplicationDetailDTO();
            }
            ApplicationDetailDTO resultDTO = _mapper.Map<ApplicationDetail, ApplicationDetailDTO>(applicationDetail);
            return resultDTO;
        }

        public async Task<ApplicationDetailDTO?> UpdateApplicationDetailAsync(ApplicationDetailDTO applicationDetailDTO)
        {
            ApplicationDetail applicationDetail = _mapper.Map<ApplicationDetailDTO, ApplicationDetail>(applicationDetailDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundApplicationDetail = await context.ApplicationDetails.AsNoTracking().FirstOrDefaultAsync(e => e.Id == applicationDetail.Id);

                if (foundApplicationDetail != null)
                {
                    var mappedApplicationDetail = _mapper.Map<ApplicationDetail>(applicationDetail);
                    context.ApplicationDetails.Update(mappedApplicationDetail);
                    await context.SaveChangesAsync();
                    ApplicationDetailDTO resultDTO = _mapper.Map<ApplicationDetail, ApplicationDetailDTO>(mappedApplicationDetail);
                    return resultDTO;
                }
            }
            return null;
        }
        public async Task DeleteApplicationDetailAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundApplicationDetail = context.ApplicationDetails.FirstOrDefault(e => e.Id == Id);
            if (foundApplicationDetail == null)
            {
                return;
            }
            context.ApplicationDetails.Remove(foundApplicationDetail);
            await context.SaveChangesAsync();
        }
    }
}