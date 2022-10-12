
using System.Collections.Generic;
using System.Threading.Tasks;

using VoiceLauncher.DTOs;

namespace VoiceLauncher.Repositories
{
    public interface IHtmlTagRepository
    {
        Task<HtmlTagDTO> AddHtmlTagAsync(HtmlTagDTO htmlTagDTO);
        Task DeleteHtmlTagAsync(int Id);
        Task<IEnumerable<HtmlTagDTO>> GetAllHtmlTagsAsync(int maxRows);
        Task<IEnumerable<HtmlTagDTO>> SearchHtmlTagsAsync(string serverSearchTerm);
        Task<HtmlTagDTO?> GetHtmlTagByIdAsync(int Id);
        Task<HtmlTagDTO?> UpdateHtmlTagAsync(HtmlTagDTO htmlTagDTO);
    }
}