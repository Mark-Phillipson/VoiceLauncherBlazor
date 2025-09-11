namespace VoiceLauncherWasm.Models
{
    public class TalonList
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ListName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Repository { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}