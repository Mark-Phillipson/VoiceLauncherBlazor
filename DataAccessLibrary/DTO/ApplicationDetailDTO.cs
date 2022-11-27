
using System.ComponentModel.DataAnnotations;

namespace VoiceLauncher.DTOs
{
    public partial class ApplicationDetailDTO
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(60)]
        public string ProcessName { get; set; } = "";
        [Required]
        [StringLength(255)]
        public string ApplicationTitle { get; set; } = "";
    }
}