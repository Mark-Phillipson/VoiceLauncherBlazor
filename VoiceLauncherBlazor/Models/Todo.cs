using System.ComponentModel.DataAnnotations;

namespace VoiceLauncherBlazor.Models
{
    public class Todo
    {
        public int Id { get; set; }
        [StringLength(255)]
        [Required]
        public string Title { get; set; }
        [StringLength(1000)]
        public string Description { get; set; }
        public bool Completed { get; set; }
        [StringLength(255)]
        public string Project { get; set; }

    }
}
