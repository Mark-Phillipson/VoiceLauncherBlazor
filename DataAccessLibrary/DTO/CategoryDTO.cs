
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.DTO
{
    public partial class CategoryDTO
    {
        [Key]
        public int Id { get; set; }
        [StringLength(30)]
        public string CategoryName { get; set; } = "A New Category";
        [StringLength(255)]
        public string? CategoryType { get; set; } = "IntelliSense Command";
        [Required]
        public bool Sensitive { get; set; }
        public int CountOfLaunchers { get; set; }
        public int? CountOfCustomIntellisense { get; set; }
        public string Colour { get; set; } = "#000000";
        [StringLength(50)]
        public string? Icon { get; set; }

    }
}