
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.DTO
{
    public partial class PromptDTO
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(3000)]
        public string PromptText { get; set; } = "";
        [StringLength(2000)]
        public string? Description { get; set; }
        [Required]
        public bool IsDefault { get; set; }
    }
}