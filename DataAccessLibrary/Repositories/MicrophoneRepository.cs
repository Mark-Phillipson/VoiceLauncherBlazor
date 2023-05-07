
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using VoiceLauncher.DTOs;
using DataAccessLibrary.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace VoiceLauncher.Repositories
{
    public class MicrophoneRepository : IMicrophoneRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;

        public MicrophoneRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory;
            this._mapper = mapper;
        }
        public async Task<IEnumerable<MicrophoneDTO>> GetAllMicrophonesAsync(int maxRows = 400)
        {
            using var context = _contextFactory.CreateDbContext();
            var Microphones = await context.Microphones
                //.Where(v => v.?==?)
                //.OrderBy(v => v.?)
                .Take(maxRows)
                .ToListAsync();
            IEnumerable<MicrophoneDTO> MicrophonesDTO = _mapper.Map<List<Microphone>, IEnumerable<MicrophoneDTO>>(Microphones);
            return MicrophonesDTO;
        }
        public async Task<IEnumerable<MicrophoneDTO>> SearchMicrophonesAsync(string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var Microphones = await context.Microphones
                //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //)
                //.OrderBy(v => v.?)
                .Take(1000)
                .ToListAsync();
            IEnumerable<MicrophoneDTO> MicrophonesDTO = _mapper.Map<List<Microphone>, IEnumerable<MicrophoneDTO>>(Microphones);
            return MicrophonesDTO;
        }

        public async Task<MicrophoneDTO> GetMicrophoneByIdAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result = await context.Microphones.AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == Id);
            if (result == null) return null;
            MicrophoneDTO microphoneDTO = _mapper.Map<Microphone, MicrophoneDTO>(result);
            return microphoneDTO;
        }

        public async Task<MicrophoneDTO> AddMicrophoneAsync(MicrophoneDTO microphoneDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            Microphone microphone = _mapper.Map<MicrophoneDTO, Microphone>(microphoneDTO);
            var addedEntity = context.Microphones.Add(microphone);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            MicrophoneDTO resultDTO = _mapper.Map<Microphone, MicrophoneDTO>(microphone);
            return resultDTO;
        }

        public async Task<MicrophoneDTO> UpdateMicrophoneAsync(MicrophoneDTO microphoneDTO)
        {
            Microphone microphone = _mapper.Map<MicrophoneDTO, Microphone>(microphoneDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundMicrophone = await context.Microphones.AsNoTracking().FirstOrDefaultAsync(e => e.Id == microphone.Id);

                if (foundMicrophone != null)
                {
                    var mappedMicrophone = _mapper.Map<Microphone>(microphone);
                    context.Microphones.Update(mappedMicrophone);
                    await context.SaveChangesAsync();
                    MicrophoneDTO resultDTO = _mapper.Map<Microphone, MicrophoneDTO>(mappedMicrophone);
                    return resultDTO;
                }
            }
            return null;
        }
        public async Task DeleteMicrophoneAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundMicrophone = context.Microphones.FirstOrDefault(e => e.Id == Id);
            if (foundMicrophone == null)
            {
                return;
            }
            context.Microphones.Remove(foundMicrophone);
            await context.SaveChangesAsync();
        }
    }
}