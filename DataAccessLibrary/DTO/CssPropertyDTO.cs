
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.DTO
{
    public partial class CssPropertyDTO
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string PropertyName { get; set; } = "";
        [StringLength(255)]
        public string? Description { get; set; }
    }
}