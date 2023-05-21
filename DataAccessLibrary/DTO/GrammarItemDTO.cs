
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.DTO
{
    public partial class GrammarItemDTO
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int GrammarNameId { get; set; }
        [Required]
        [StringLength(60)]
        public string Value { get; set; } = "";
    }
}