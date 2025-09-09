namespace SharedContracts.Models
{
    public class TargetApplicationDto
    {
        public string? CommandSource { get; set; }
    }

    public class VoiceCommandDto
    {
        public string? Name { get; set; }
        public TargetApplicationDto? TargetApplication { get; set; }
    }

    public class VoiceCommandContentDto
    {
        public string? Type { get; set; }
        public string? Content { get; set; }
    }
}
