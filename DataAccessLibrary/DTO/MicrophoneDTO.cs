
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.DTO
{
    public partial class MicrophoneDTO
    {
        [Key]
        public int Id { get; set; }
        [StringLength(100)]
        [Required]
        public string MicrophoneName { get; set; } = "";
        [Required]
        public bool Default { get; set; }
    }
}