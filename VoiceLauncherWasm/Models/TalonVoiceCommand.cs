namespace VoiceLauncherWasm.Models
{
    public class TalonVoiceCommand
    {
        public int Id { get; set; }
        public string Command { get; set; } = string.Empty;  // Renamed from VoiceCommand
        public string Script { get; set; } = string.Empty;   // Renamed from TalonScript  
        public string Application { get; set; } = string.Empty;
        public string Repository { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty; // Renamed from FilePath
        public string Mode { get; set; } = string.Empty;     // Added
        public string Tags { get; set; } = string.Empty;     // Added
        public DateTime DateCreated { get; set; } = DateTime.Now;
        
        // Keep old properties for backwards compatibility if needed
        public string VoiceCommand => Command;
        public string TalonScript => Script;
        public string FilePath => FileName;
    }
}