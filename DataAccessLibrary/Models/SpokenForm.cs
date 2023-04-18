using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    [Index(nameof(SpokenFormText), nameof(WindowsSpeechVoiceCommandId), IsUnique = true)]
    [Table("SpokenForm")]
    public class SpokenForm
    {
        public int Id { get; set; }
        [StringLength(100)]
        [Required]
        public string SpokenFormText { get; set; }
        public int WindowsSpeechVoiceCommandId { get; set; }
        public WindowsSpeechVoiceCommand WindowsSpeechVoiceCommand { get; set; }
    }

}
