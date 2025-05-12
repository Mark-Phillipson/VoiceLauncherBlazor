using DataAccessLibrary.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace DataAccessLibrary.Repositories;

public interface ILauncherRepository
{
    Task<LauncherDTO?> AddLauncherAsync(LauncherDTO launcherDTO);
    Task DeleteLauncherAsync(int Id);
    Task<IEnumerable<LauncherDTO>> GetAllLaunchersAsync(int CategoryID);
    Task<IEnumerable<LauncherDTO>> GetFavoriteLaunchersAsync();
    Task<IEnumerable<LauncherDTO>> SearchLaunchersAsync(string serverSearchTerm);
    Task<LauncherDTO?> GetLauncherByIdAsync(int Id);
    Task<LauncherDTO?> UpdateLauncherAsync(LauncherDTO launcherDTO);
    Task UpdateLauncherCategoriesAsync(int id, HashSet<int> selectedCategoryIds);
}