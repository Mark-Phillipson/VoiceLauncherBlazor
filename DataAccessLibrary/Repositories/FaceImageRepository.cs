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
    public class FaceImageRepository : IFaceImageRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;

        public FaceImageRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FaceImageDTO>> GetAllFaceImagesAsync(int maxRows = 100)
        {
            using var context = _contextFactory.CreateDbContext();
            var faceImages = await context.FaceImages
                .Include(fi => fi.FaceTags)
                .OrderByDescending(fi => fi.UploadDate)
                .Take(maxRows)
                .ToListAsync();
            return _mapper.Map<List<FaceImage>, IEnumerable<FaceImageDTO>>(faceImages);
        }

        public async Task<FaceImageDTO?> GetFaceImageByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result = await context.FaceImages
                .Include(fi => fi.FaceTags)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (result == null) return null;
            return _mapper.Map<FaceImage, FaceImageDTO>(result);
        }

        public async Task<FaceImageDTO?> AddFaceImageAsync(FaceImageDTO faceImageDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            FaceImage faceImage = _mapper.Map<FaceImageDTO, FaceImage>(faceImageDTO);
            var addedEntity = context.FaceImages.Add(faceImage);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            return _mapper.Map<FaceImage, FaceImageDTO>(faceImage);
        }

        public async Task<FaceImageDTO?> UpdateFaceImageAsync(FaceImageDTO faceImageDTO)
        {
            FaceImage faceImage = _mapper.Map<FaceImageDTO, FaceImage>(faceImageDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundFaceImage = await context.FaceImages.AsNoTracking().FirstOrDefaultAsync(e => e.Id == faceImage.Id);
                if (foundFaceImage == null)
                {
                    return null;
                }
                context.FaceImages.Update(faceImage);
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
            return _mapper.Map<FaceImage, FaceImageDTO>(faceImage);
        }

        public async Task<bool> DeleteFaceImageAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var faceImage = await context.FaceImages.FindAsync(id);
            if (faceImage == null)
            {
                return false;
            }
            context.FaceImages.Remove(faceImage);
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

        public async Task<IEnumerable<FaceImageDTO>> SearchFaceImagesByNameAsync(string searchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var faceImages = await context.FaceImages
                .Include(fi => fi.FaceTags)
                .Where(x => EF.Functions.Like(x.ImageName, $"%{searchTerm}%"))
                .OrderBy(x => x.ImageName)
                .ToListAsync();
            return _mapper.Map<List<FaceImage>, IEnumerable<FaceImageDTO>>(faceImages);
        }
    }
}
