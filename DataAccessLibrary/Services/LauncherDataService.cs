using Ardalis.GuardClauses;
using AutoMapper;
using DataAccessLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLibrary.Models;
using DataAccessLibrary.Repositories;

namespace DataAccessLibrary.Services
{
    public class LauncherDataService : ILauncherDataService
    {
        private readonly ILauncherRepository _launcherRepository;

        public LauncherDataService(ILauncherRepository launcherRepository)
        {
            _launcherRepository = launcherRepository;
        }
        public async Task<List<LauncherDTO>> GetAllLaunchersAsync(int CategoryID)
        {
            var Launchers = await _launcherRepository.GetAllLaunchersAsync(CategoryID);
            return Launchers.ToList();
        }
        public async Task<List<LauncherDTO>> SearchLaunchersAsync(string serverSearchTerm)
        {
            var Launchers = await _launcherRepository.SearchLaunchersAsync(serverSearchTerm);
            return Launchers.ToList();
        }

        public async Task<LauncherDTO?> GetLauncherById(int Id)
        {
            var launcher = await _launcherRepository.GetLauncherByIdAsync(Id);
            return launcher;
        }
        public async Task<LauncherDTO?> AddLauncher(LauncherDTO launcherDTO)
        {
            Guard.Against.Null(launcherDTO);
            var result = await _launcherRepository.AddLauncherAsync(launcherDTO);
            if (result == null)
            {
                throw new Exception($"Add of launcher failed ID: {launcherDTO.Id}");
            }
            return result;
        }
        public async Task<LauncherDTO?> UpdateLauncher(LauncherDTO launcherDTO, string username)
        {
            Guard.Against.Null(launcherDTO);
            Guard.Against.Null(username);
            var result = await _launcherRepository.UpdateLauncherAsync(launcherDTO);
            if (result == null)
            {
                throw new Exception($"Update of launcher failed ID: {launcherDTO.Id}");
            }
            return result;
        }

        public async Task DeleteLauncher(int Id)
        {
            await _launcherRepository.DeleteLauncherAsync(Id);
        }

        public async Task<List<LauncherDTO>> GetFavoriteLaunchersAsync()
        {
            var Launchers = await _launcherRepository.GetFavoriteLaunchersAsync();
            return Launchers.ToList();
        }
    }
}