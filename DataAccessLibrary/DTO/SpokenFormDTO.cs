using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.DTO
{
  public partial class SpokenFormDTO
  {
    [Key]
    public int Id { get; set; }
    [Required]
    [StringLength(100)]
    public string SpokenFormText { get; set; } = "";
    [Required]
    public int WindowsSpeechVoiceCommandId { get; set; }
  }
}