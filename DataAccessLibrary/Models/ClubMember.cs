using System;

namespace DataAccessLibrary.Models
{
    public class ClubMember
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ContentType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
