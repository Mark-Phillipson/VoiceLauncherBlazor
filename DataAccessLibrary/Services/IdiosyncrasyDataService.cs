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
    public class IdiosyncrasyDataService : IIdiosyncrasyDataService
    {
        private readonly IIdiosyncrasyRepository _idiosyncrasyRepository;

        public IdiosyncrasyDataService(IIdiosyncrasyRepository idiosyncrasyRepository)
        {
            this._idiosyncrasyRepository = idiosyncrasyRepository;
        }
        public async Task<List<IdiosyncrasyDTO>> GetAllIdiosyncrasiesAsync()
        {
            var Idiosyncrasies = await _idiosyncrasyRepository.GetAllIdiosyncrasiesAsync(300);
            return Idiosyncrasies.ToList();
        }
        public async Task<List<IdiosyncrasyDTO>> SearchIdiosyncrasiesAsync(string serverSearchTerm)
        {
            var Idiosyncrasies = await _idiosyncrasyRepository.SearchIdiosyncrasiesAsync(serverSearchTerm);
            return Idiosyncrasies.ToList();
        }

        public async Task<IdiosyncrasyDTO> GetIdiosyncrasyById(int Id)
        {
            var idiosyncrasy = await _idiosyncrasyRepository.GetIdiosyncrasyByIdAsync(Id);
            return idiosyncrasy;
        }
        public async Task<IdiosyncrasyDTO> AddIdiosyncrasy(IdiosyncrasyDTO idiosyncrasyDTO)
        {
            Guard.Against.Null(idiosyncrasyDTO);
            var result = await _idiosyncrasyRepository.AddIdiosyncrasyAsync(idiosyncrasyDTO);
            if (result == null)
            {
                throw new Exception($"Add of idiosyncrasy failed ID: {idiosyncrasyDTO.Id}");
            }
            return result;
        }
        public async Task<IdiosyncrasyDTO> UpdateIdiosyncrasy(IdiosyncrasyDTO idiosyncrasyDTO, string username)
        {
            Guard.Against.Null(idiosyncrasyDTO);
            Guard.Against.Null(username);
            var result = await _idiosyncrasyRepository.UpdateIdiosyncrasyAsync(idiosyncrasyDTO);
            if (result == null)
            {
                throw new Exception($"Update of idiosyncrasy failed ID: {idiosyncrasyDTO.Id}");
            }
            return result;
        }

        public async Task DeleteIdiosyncrasy(int Id)
        {
            await _idiosyncrasyRepository.DeleteIdiosyncrasyAsync(Id);
        }
    }
}