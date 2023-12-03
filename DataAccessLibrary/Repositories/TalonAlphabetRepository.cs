
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using System.Threading.Tasks;
using System.Collections.Generic;
using DataAccessLibrary.Models;
using System.Linq;
using System;
using DataAccessLibrary.DTO;

namespace DataAccessLibrary.Repositories
{
	public class TalonAlphabetRepository : ITalonAlphabetRepository
	{
		private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
		private readonly IMapper _mapper;

		public TalonAlphabetRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
		{
			_contextFactory = contextFactory;
			_mapper = mapper;
		}
		public async Task<IEnumerable<TalonAlphabetDTO>> GetAllTalonAlphabetsAsync(int maxRows = 400)
		{
			using var context = _contextFactory.CreateDbContext();
			var TalonAlphabets = await context.TalonAlphabets
				 //.Where(v => v.?==?)
				 //.OrderBy(v => v.?)
				 .Take(maxRows)
				 .ToListAsync();
			IEnumerable<TalonAlphabetDTO> TalonAlphabetsDTO = _mapper.Map<List<TalonAlphabet>, IEnumerable<TalonAlphabetDTO>>(TalonAlphabets);
			return TalonAlphabetsDTO;
		}
		public async Task<IEnumerable<TalonAlphabetDTO>> SearchTalonAlphabetsAsync(string serverSearchTerm)
		{
			using var context = _contextFactory.CreateDbContext();
			var TalonAlphabets = await context.TalonAlphabets
				 //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
				 //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
				 //)
				 //.OrderBy(v => v.?)
				 .Take(1000)
				 .ToListAsync();
			IEnumerable<TalonAlphabetDTO> TalonAlphabetsDTO = _mapper.Map<List<TalonAlphabet>, IEnumerable<TalonAlphabetDTO>>(TalonAlphabets);
			return TalonAlphabetsDTO;
		}

		public async Task<TalonAlphabetDTO> GetTalonAlphabetByIdAsync(int Id)
		{
			using var context = _contextFactory.CreateDbContext();
			var result = await context.TalonAlphabets.AsNoTracking()
			  .FirstOrDefaultAsync(c => c.Id == Id);
			if (result == null) return null;
			TalonAlphabetDTO talonAlphabetDTO = _mapper.Map<TalonAlphabet, TalonAlphabetDTO>(result);
			return talonAlphabetDTO;
		}

		public async Task<TalonAlphabetDTO> AddTalonAlphabetAsync(TalonAlphabetDTO talonAlphabetDTO)
		{
			using var context = _contextFactory.CreateDbContext();
			TalonAlphabet talonAlphabet = _mapper.Map<TalonAlphabetDTO, TalonAlphabet>(talonAlphabetDTO);
			var addedEntity = context.TalonAlphabets.Add(talonAlphabet);
			try
			{
				await context.SaveChangesAsync();
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				return null;
			}
			TalonAlphabetDTO resultDTO = _mapper.Map<TalonAlphabet, TalonAlphabetDTO>(talonAlphabet);
			return resultDTO;
		}

		public async Task<TalonAlphabetDTO> UpdateTalonAlphabetAsync(TalonAlphabetDTO talonAlphabetDTO)
		{
			TalonAlphabet talonAlphabet = _mapper.Map<TalonAlphabetDTO, TalonAlphabet>(talonAlphabetDTO);
			using (var context = _contextFactory.CreateDbContext())
			{
				var foundTalonAlphabet = await context.TalonAlphabets.AsNoTracking().FirstOrDefaultAsync(e => e.Id == talonAlphabet.Id);

				if (foundTalonAlphabet != null)
				{
					var mappedTalonAlphabet = _mapper.Map<TalonAlphabet>(talonAlphabet);
					context.TalonAlphabets.Update(mappedTalonAlphabet);
					await context.SaveChangesAsync();
					TalonAlphabetDTO resultDTO = _mapper.Map<TalonAlphabet, TalonAlphabetDTO>(mappedTalonAlphabet);
					return resultDTO;
				}
			}
			return null;
		}
		public async Task DeleteTalonAlphabetAsync(int Id)
		{
			using var context = _contextFactory.CreateDbContext();
			var foundTalonAlphabet = context.TalonAlphabets.FirstOrDefault(e => e.Id == Id);
			if (foundTalonAlphabet == null)
			{
				return;
			}
			context.TalonAlphabets.Remove(foundTalonAlphabet);
			await context.SaveChangesAsync();
		}
	}
}