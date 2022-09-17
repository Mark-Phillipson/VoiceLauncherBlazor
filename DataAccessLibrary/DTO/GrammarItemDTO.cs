
using System.ComponentModel.DataAnnotations;

namespace VoiceLauncher.DTOs
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