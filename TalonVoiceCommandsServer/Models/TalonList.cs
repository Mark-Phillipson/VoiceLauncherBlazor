using System;
using System.ComponentModel.DataAnnotations;

namespace TalonVoiceCommandsServer.Models
{
    public class TalonList
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        [Required]
        public required string ListName { get; set; }

        [StringLength(100)]
        [Required]
        public required string SpokenForm { get; set; }
        [StringLength(700)]
        [Required]
        public required string ListValue { get; set; }

        [StringLength(250)]
        public string? SourceFile { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime ImportedAt { get; set; }
    }
}
