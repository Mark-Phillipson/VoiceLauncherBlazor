using System;

namespace SharedContracts.Models
{
    public class TalonListDto
    {
        public int Id { get; set; }
        public string ListName { get; set; } = string.Empty;
        public string SpokenForm { get; set; } = string.Empty;
        public string ListValue { get; set; } = string.Empty;
        public string? SourceFile { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
