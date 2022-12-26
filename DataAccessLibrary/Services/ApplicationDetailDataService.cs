using Ardalis.GuardClauses;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceLauncher.Repositories;
using VoiceLauncher.DTOs;


namespace VoiceLauncher.Services
{
    public class ApplicationDetailDataService : IApplicationDetailDataService
    {
        private readonly IApplicationDetailRepository _applicationDetailRepository;

        public ApplicationDetailDataService(IApplicationDetailRepository applicationDetailRepository)
        {
            this._applicationDetailRepository = applicationDetailRepository;
        }
        public async Task<List<ApplicationDetailDTO>> GetAllApplicationDetailsAsync()
        {
            var ApplicationDetails = await _applicationDetailRepository.GetAllApplicationDetailsAsync(300);
            return ApplicationDetails.ToList();
        }
        public async Task<List<ApplicationDetailDTO>> SearchApplicationDetailsAsync(string serverSearchTerm)
        {
            var ApplicationDetails = await _applicationDetailRepository.SearchApplicationDetailsAsync(serverSearchTerm);
            return ApplicationDetails.ToList();
        }

        public async Task<ApplicationDetailDTO> GetApplicationDetailById(int Id)
        {
            var applicationDetail = await _applicationDetailRepository.GetApplicationDetailByIdAsync(Id);
            return applicationDetail;
        }
        public async Task<ApplicationDetailDTO> AddApplicationDetail(ApplicationDetailDTO applicationDetailDTO)
        {
            Guard.Against.Null(applicationDetailDTO);
            var result = await _applicationDetailRepository.AddApplicationDetailAsync(applicationDetailDTO);
            if (result == null)
            {
                throw new Exception($"Add of applicationDetail failed ID: {applicationDetailDTO.Id}");
            }
            return result;
        }
        public async Task<ApplicationDetailDTO> UpdateApplicationDetail(ApplicationDetailDTO applicationDetailDTO, string username)
        {
            Guard.Against.Null(applicationDetailDTO);
            Guard.Against.Null(username);
            var result = await _applicationDetailRepository.UpdateApplicationDetailAsync(applicationDetailDTO);
            if (result == null)
            {
                throw new Exception($"Update of applicationDetail failed ID: {applicationDetailDTO.Id}");
            }
            return result;
        }

        public async Task DeleteApplicationDetail(int Id)
        {
            await _applicationDetailRepository.DeleteApplicationDetailAsync(Id);
        }
    }
}