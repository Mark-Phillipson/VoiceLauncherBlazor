namespace SharedContracts.Models
{
    public class CommandSetDto
    {
        public List<SpeechListDto>? SpeechLists { get; set; }
    }

    public class SpeechListDto
    {
        public string? Name { get; set; }
        public List<ListValueDto>? ListValues { get; set; }
    }

    public class ListValueDto
    {
        public string? Value_Text { get; set; }
    }
}
