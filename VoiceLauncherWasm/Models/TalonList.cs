using System.ComponentModel.DataAnnotations;

namespace VoiceLauncherWasm.Models
{
    public class TalonList
    {
        public int Id { get; set; }

        [Required]
        public required string ListName { get; set; }

        [Required]
        public required string SpokenForm { get; set; }

        [Required]
        public required string ListValue { get; set; }

        public string? SourceFile { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime ImportedAt { get; set; }
    }
}