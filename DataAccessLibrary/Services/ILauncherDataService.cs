using DataAccessLibrary.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace DataAccessLibrary.Services;

public interface ILauncherDataService
{
    Task<List<LauncherDTO>> GetAllLaunchersAsync(int CategoryID);
    Task<List<LauncherDTO>> GetFavoriteLaunchersAsync();
    Task<List<LauncherDTO>> SearchLaunchersAsync(string serverSearchTerm);
    Task<LauncherDTO?> AddLauncher(LauncherDTO launcherDTO);
    Task<LauncherDTO?> GetLauncherById(int Id);
    Task<LauncherDTO?> UpdateLauncher(LauncherDTO launcherDTO, string username);
    Task DeleteLauncher(int Id);
    void ClearCache();
}
