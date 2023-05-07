using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    public class Microphone
    {
        public int Id { get; set; }
        [StringLength(100)]
        [Required]
        public string MicrophoneName { get; set; } = "";
        public bool Default { get; set; }
    }
}
