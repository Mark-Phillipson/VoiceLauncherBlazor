
using System.ComponentModel.DataAnnotations;

namespace VoiceLauncher.DTOs
{
    public partial class HtmlTagDTO
    {
        [Key]
        public int Id { get; set; }
        [StringLength(255)]
        public string? Tag { get; set; }
        [StringLength(255)]
        public string? Description { get; set; }
        [StringLength(255)]
        public string? ListValue { get; set; }
        [Required]
        public bool Include { get; set; }
        [StringLength(255)]
        public string? SpokenForm { get; set; }
    }
}