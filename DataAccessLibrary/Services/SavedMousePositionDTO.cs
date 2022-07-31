
using System;
using System.ComponentModel.DataAnnotations;

namespace VoiceLauncher.DTOs
{
    public partial class SavedMousePositionDTO
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(255)]
        public string NamedLocation { get; set; } ="";
        [Required]
        public int X { get; set; }
        [Required]
        public int Y { get; set; }
        public DateTime? Created { get; set; }
    }   
}