namespace VoiceLauncherWasm.Models
{
    public class TalonVoiceCommand
    {
        public int Id { get; set; }
        public string VoiceCommand { get; set; } = string.Empty;
        public string TalonScript { get; set; } = string.Empty;
        public string Application { get; set; } = string.Empty;
        public string Repository { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}