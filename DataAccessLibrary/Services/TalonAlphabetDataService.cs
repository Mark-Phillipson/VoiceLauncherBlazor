using Ardalis.GuardClauses;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLibrary.Repositories;
using DataAccessLibrary.DTO;


namespace DataAccessLibrary.Services
{
	public class TalonAlphabetDataService : ITalonAlphabetDataService
	{
		private readonly ITalonAlphabetRepository _talonAlphabetRepository;

		public TalonAlphabetDataService(ITalonAlphabetRepository talonAlphabetRepository)
		{
			_talonAlphabetRepository = talonAlphabetRepository;
		}
		public async Task<List<TalonAlphabetDTO>> GetAllTalonAlphabetsAsync()
		{
			var TalonAlphabets = await _talonAlphabetRepository.GetAllTalonAlphabetsAsync(300);
			return TalonAlphabets.ToList();
		}
		public async Task<List<TalonAlphabetDTO>> SearchTalonAlphabetsAsync(string serverSearchTerm)
		{
			var TalonAlphabets = await _talonAlphabetRepository.SearchTalonAlphabetsAsync(serverSearchTerm);
			return TalonAlphabets.ToList();
		}

		public async Task<TalonAlphabetDTO?> GetTalonAlphabetById(int Id)
		{
			var talonAlphabet = await _talonAlphabetRepository.GetTalonAlphabetByIdAsync(Id);
			return talonAlphabet;
		}
		public async Task<TalonAlphabetDTO?> AddTalonAlphabet(TalonAlphabetDTO talonAlphabetDTO)
		{
			Guard.Against.Null(talonAlphabetDTO);
			var result = await _talonAlphabetRepository.AddTalonAlphabetAsync(talonAlphabetDTO);
			if (result == null)
			{
				throw new Exception($"Add of talonAlphabet failed ID: {talonAlphabetDTO.Id}");
			}
			return result;
		}
		public async Task<TalonAlphabetDTO?> UpdateTalonAlphabet(TalonAlphabetDTO talonAlphabetDTO, string username)
		{
			Guard.Against.Null(talonAlphabetDTO);
			Guard.Against.Null(username);
			var result = await _talonAlphabetRepository.UpdateTalonAlphabetAsync(talonAlphabetDTO);
			if (result == null)
			{
				throw new Exception($"Update of talonAlphabet failed ID: {talonAlphabetDTO.Id}");
			}
			return result;
		}

		public async Task DeleteTalonAlphabet(int Id)
		{
			await _talonAlphabetRepository.DeleteTalonAlphabetAsync(Id);
		}
	}
}