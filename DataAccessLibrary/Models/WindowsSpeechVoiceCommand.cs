namespace DataAccessLibrary.Models
{
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;


  [Table("WindowsSpeechVoiceCommand")]
  public partial class WindowsSpeechVoiceCommand
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public WindowsSpeechVoiceCommand()
    {
      SpokenForms = new HashSet<SpokenForm>();
      CustomWindowsSpeechCommands = new HashSet<CustomWindowsSpeechCommand>();
    }

    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string SpokenCommand { get; set; }// To be depreciated 

    [StringLength(1000)]
    public string Description { get; set; }
    public string ApplicationName { get; set; }
    public bool AutoCreated { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public virtual ICollection<SpokenForm> SpokenForms { get; set; }
    public virtual ICollection<CustomWindowsSpeechCommand> CustomWindowsSpeechCommands { get; set; }
  }
}
