using AutoMapper;
using Microsoft.EntityFrameworkCore;
using DataAccessLibrary.Models;
using DataAccessLibrary.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.Repositories
{
    public class FaceTagRepository : IFaceTagRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;

        public FaceTagRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FaceTagDTO>> GetFaceTagsByImageIdAsync(int faceImageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var faceTags = await context.FaceTags
                .Where(ft => ft.FaceImageId == faceImageId)
                .ToListAsync();
            return _mapper.Map<List<FaceTag>, IEnumerable<FaceTagDTO>>(faceTags);
        }

        public async Task<FaceTagDTO?> GetFaceTagByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result = await context.FaceTags.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (result == null) return null;
            return _mapper.Map<FaceTag, FaceTagDTO>(result);
        }

        public async Task<FaceTagDTO?> AddFaceTagAsync(FaceTagDTO faceTagDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            FaceTag faceTag = _mapper.Map<FaceTagDTO, FaceTag>(faceTagDTO);
            var addedEntity = context.FaceTags.Add(faceTag);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            return _mapper.Map<FaceTag, FaceTagDTO>(faceTag);
        }

        public async Task<FaceTagDTO?> UpdateFaceTagAsync(FaceTagDTO faceTagDTO)
        {
            FaceTag faceTag = _mapper.Map<FaceTagDTO, FaceTag>(faceTagDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundFaceTag = await context.FaceTags.AsNoTracking().FirstOrDefaultAsync(e => e.Id == faceTag.Id);
                if (foundFaceTag == null)
                {
                    return null;
                }
                context.FaceTags.Update(faceTag);
                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    return null;
                }
            }
            return _mapper.Map<FaceTag, FaceTagDTO>(faceTag);
        }

        public async Task<bool> DeleteFaceTagAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var faceTag = await context.FaceTags.FindAsync(id);
            if (faceTag == null)
            {
                return false;
            }
            context.FaceTags.Remove(faceTag);
            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return false;
            }
        }

        public async Task<IEnumerable<FaceTagDTO>> SearchFaceTagsByNameAsync(string firstName, int faceImageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var faceTags = await context.FaceTags
                .Where(x => x.FaceImageId == faceImageId && 
                           EF.Functions.Like(x.FirstName, $"%{firstName}%"))
                .ToListAsync();
            return _mapper.Map<List<FaceTag>, IEnumerable<FaceTagDTO>>(faceTags);
        }
    }
}
