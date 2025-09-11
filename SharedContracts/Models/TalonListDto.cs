using System;

namespace SharedContracts.Models
{
    public class TalonListDto
    {
        public int Id { get; set; }
        public string? ListName { get; set; }
        public string? SpokenForm { get; set; }
        public string? ListValue { get; set; }
        public string? SourceFile { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
