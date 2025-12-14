using DataAccessLibrary.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.Repositories
{
    public interface IFaceImageRepository
    {
        Task<IEnumerable<FaceImageDTO>> GetAllFaceImagesAsync(int maxRows = 100);
        Task<FaceImageDTO?> GetFaceImageByIdAsync(int id);
        Task<FaceImageDTO?> AddFaceImageAsync(FaceImageDTO faceImageDTO);
        Task<FaceImageDTO?> UpdateFaceImageAsync(FaceImageDTO faceImageDTO);
        Task<bool> DeleteFaceImageAsync(int id);
        Task<IEnumerable<FaceImageDTO>> SearchFaceImagesByNameAsync(string searchTerm);
    }
}
