using System;
using System.Collections.Generic;

namespace SharedContracts.Models
{
    public class TalonVoiceCommandDto
    {
        public int Id { get; set; }
        public string Command { get; set; } = string.Empty;
        public string Script { get; set; } = string.Empty;
        public string Application { get; set; } = "global";
        public string? Title { get; set; }
        public string? Mode { get; set; }
        public string? OperatingSystem { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string? Repository { get; set; }
        public string? Tags { get; set; }
        public string? CodeLanguage { get; set; }
        public string? Language { get; set; }
        public string? Hostname { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
