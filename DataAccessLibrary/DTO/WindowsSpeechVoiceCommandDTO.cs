using DataAccessLibrary.Models;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.DTO
{
  public partial class WindowsSpeechVoiceCommandDTO
  {
    public WindowsSpeechVoiceCommandDTO()
    {
      SpokenForms = new HashSet<SpokenForm>();
    }
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string SpokenCommand { get; set; } = "";
    [StringLength(1000)]
    public string Description { get; set; }
    public string ApplicationName { get; set; } = "Global";
    public bool AutoCreated { get; set; }
    public string SendKeysValue { get; set; }
    public virtual ICollection<SpokenForm> SpokenForms { get; set; }

  }
}