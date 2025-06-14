using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    public class TalonVoiceCommand
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        [Required]
        public required string Command { get; set; }

        [StringLength(1000)]
        [Required]
        public required string Script { get; set; }

        [StringLength(100)]
        [Required]
        public required string Application { get; set; } = "global";        [StringLength(100)]
        public string? Mode { get; set; }

        [StringLength(50)]
        public string? OperatingSystem { get; set; }        [StringLength(250)]
        [Required]
        public required string FilePath { get; set; }        [StringLength(100)]
        public string? Repository { get; set; }

        [StringLength(200)]
        public string? Tags { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
