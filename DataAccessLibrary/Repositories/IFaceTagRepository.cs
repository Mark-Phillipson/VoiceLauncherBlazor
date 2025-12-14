using DataAccessLibrary.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.Repositories
{
    public interface IFaceTagRepository
    {
        Task<IEnumerable<FaceTagDTO>> GetFaceTagsByImageIdAsync(int faceImageId);
        Task<FaceTagDTO?> GetFaceTagByIdAsync(int id);
        Task<FaceTagDTO?> AddFaceTagAsync(FaceTagDTO faceTagDTO);
        Task<FaceTagDTO?> UpdateFaceTagAsync(FaceTagDTO faceTagDTO);
        Task<bool> DeleteFaceTagAsync(int id);
        Task<IEnumerable<FaceTagDTO>> SearchFaceTagsByNameAsync(string firstName, int faceImageId);
    }
}
