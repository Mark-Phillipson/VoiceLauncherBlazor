namespace VoiceLauncherWasm.Models
{
    public class TalonList
    {
        public int Id { get; set; }
        public string ListName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Repository { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}