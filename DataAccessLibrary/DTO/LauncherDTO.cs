
using System.ComponentModel.DataAnnotations;

namespace VoiceLauncher.DTOs
{
    public partial class LauncherDTO
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = "";
        [StringLength(255)]
        public string CommandLine { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public int? ComputerId { get; set; }
    }
}