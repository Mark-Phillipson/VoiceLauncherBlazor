using System;

namespace SharedContracts.Models
{
    public class TalonVoiceCommandDto
    {
        public int Id { get; set; }
        public string? Command { get; set; }
        public string? Script { get; set; }
        public string? Application { get; set; }
        public string? Title { get; set; }
        public string? Mode { get; set; }
        public string? OperatingSystem { get; set; }
        public string? FilePath { get; set; }
        public string? Repository { get; set; }
        public string? Tags { get; set; }
        public string? CodeLanguage { get; set; }
        public string? Language { get; set; }
        public string? Hostname { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
