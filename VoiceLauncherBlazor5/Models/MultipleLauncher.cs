using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace VoiceLauncherBlazor.Models
{
    public partial class MultipleLauncher
    {
        public MultipleLauncher()
        {
            LaunchersMultipleLauncherBridges = new HashSet<LauncherMultipleLauncherBridge>();
        }
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [StringLength(70)]
        [Required]
        public string Description { get; set; }
        public virtual ICollection<LauncherMultipleLauncherBridge> LaunchersMultipleLauncherBridges { get; set; }
    }
}
