
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.DTO
{
    public partial class LauncherDTO
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = "";
        [StringLength(255)]
        public string CommandLine { get; set; } = "";
        [StringLength(255)]
        public string? WorkingDirectory { get; set; }
        [StringLength(255)]
        public string? Arguments { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public int? ComputerId { get; set; }
        public string Icon { get; set; } = "";
        public bool Favourite { get; set; }
        public int SortOrder { get; set; }

    }
}