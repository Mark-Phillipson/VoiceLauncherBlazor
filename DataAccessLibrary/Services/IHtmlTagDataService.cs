
using System.Collections.Generic;
using System.Threading.Tasks;

using VoiceLauncher.DTOs;

namespace VoiceLauncher.Services
{
    public interface IHtmlTagDataService
    {
        Task<List<HtmlTagDTO>> GetAllHtmlTagsAsync( );
        Task<List<HtmlTagDTO>> SearchHtmlTagsAsync(string serverSearchTerm);
        Task<HtmlTagDTO> AddHtmlTag(HtmlTagDTO htmlTagDTO);
        Task<HtmlTagDTO> GetHtmlTagById(int Id);
        Task<HtmlTagDTO> UpdateHtmlTag(HtmlTagDTO htmlTagDTO, string username);
        Task DeleteHtmlTag(int Id);
    }
}
