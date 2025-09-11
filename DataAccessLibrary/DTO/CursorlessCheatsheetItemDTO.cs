
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.DTOs
{
    public partial class CursorlessCheatsheetItemDTO
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string SpokenForm { get; set; } = "Take [target]";
        [StringLength(255)]
        public string Meaning { get; set; } = "Some Action in Visual Studio Code";
        [StringLength(255)]
        public string CursorlessType { get; set; } = "action";
        public string? YoutubeLink { get; set; }
    }
}