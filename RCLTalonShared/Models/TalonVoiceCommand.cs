namespace RCLTalonShared.Models
{
    public class TalonVoiceCommand
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string VoiceCommand { get; set; } = string.Empty;
        public string TalonScript { get; set; } = string.Empty;
        public string Application { get; set; } = string.Empty;
        public string Repository { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public System.DateTime DateCreated { get; set; } = System.DateTime.UtcNow;
    }
}
