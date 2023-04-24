using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{

  [Table("WindowsSpeechVoiceCommand")]
  public class WindowsSpeechVoiceCommand
  {
    public WindowsSpeechVoiceCommand()
    {
      SpokenForms = new HashSet<SpokenForm>();
      CustomWindowsSpeechCommands = new HashSet<CustomWindowsSpeechCommand>();
    }
    public int Id { get; set; }
    public string SpokenCommand { get; set; }// To be depreciated But still used to create the first record
    [StringLength(1000)]
    public string Description { get; set; }
    [StringLength(50)]
    public string ApplicationName { get; set; }
    public bool AutoCreated { get; set; }
    public virtual ICollection<SpokenForm> SpokenForms { get; set; }
    public virtual ICollection<CustomWindowsSpeechCommand> CustomWindowsSpeechCommands { get; set; }
  }
}
