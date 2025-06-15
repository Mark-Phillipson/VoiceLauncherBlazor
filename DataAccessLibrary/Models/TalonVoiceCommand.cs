using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    public class TalonVoiceCommand
    {
        [Key]
        public int Id { get; set; }        [StringLength(200)]
        [Required]
        public required string Command { get; set; }

        [StringLength(2000)]
        [Required]
        public required string Script { get; set; }

        [StringLength(200)]
        [Required]
        public required string Application { get; set; } = "global";        [StringLength(300)]
        public string? Mode { get; set; }

        [StringLength(100)]
        public string? OperatingSystem { get; set; }        [StringLength(500)]
        [Required]
        public required string FilePath { get; set; }        [StringLength(200)]
        public string? Repository { get; set; }

        [StringLength(500)]
        public string? Tags { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
