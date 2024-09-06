using DataAccessLibrary.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace VoiceLauncher.Services
{
    public interface IHtmlTagDataService
    {
        Task<List<HtmlTagDTO>> GetAllHtmlTagsAsync();
        Task<List<HtmlTagDTO>> SearchHtmlTagsAsync(string serverSearchTerm);
        Task<HtmlTagDTO?> AddHtmlTag(HtmlTagDTO htmlTagDTO);
        Task<HtmlTagDTO?> GetHtmlTagById(int Id);
        Task<HtmlTagDTO?> UpdateHtmlTag(HtmlTagDTO htmlTagDTO, string username);
        Task DeleteHtmlTag(int Id);
    }
}
