using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Models
{
    
    [Table("WindowsSpeechVoiceCommand")]
    public class WindowsSpeechVoiceCommand
    {
        public int Id { get; set; }
        [StringLength(100)]
        [Required]
        public string SpokenCommand { get; set; }
        [StringLength(1000)]
        public string Description { get; set; }
        [StringLength( 50 )]
        public string ApplicationName { get; set; }
        public bool AutoCreated { get; set; }

    }
}
