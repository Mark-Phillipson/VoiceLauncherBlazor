using System.ComponentModel.DataAnnotations;

namespace VoiceLauncherWasm.Models
{
    public class TalonVoiceCommand
    {
        public int Id { get; set; }
        
        [Required]
        public required string Command { get; set; }

        [Required]
        public required string Script { get; set; }

        [Required]
        public required string Application { get; set; } = "global";
        
        public string? Title { get; set; }
        public string? Mode { get; set; }
        public string? OperatingSystem { get; set; }
        
        [Required]
        public required string FilePath { get; set; }
        
        public string? Repository { get; set; }
        public string? Tags { get; set; }
        public string? CodeLanguage { get; set; }
        public string? Language { get; set; }
        public string? Hostname { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}