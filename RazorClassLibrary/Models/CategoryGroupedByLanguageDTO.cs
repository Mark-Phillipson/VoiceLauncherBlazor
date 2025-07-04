using DataAccessLibrary.DTO;

namespace RazorClassLibrary.Models
{
    public class CategoryGroupedByLanguageDTO
    {
        public int LanguageId { get; set; }
        public string LanguageName { get; set; } = string.Empty;
        public string? LanguageColour { get; set; }
        public List<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();
    }
}