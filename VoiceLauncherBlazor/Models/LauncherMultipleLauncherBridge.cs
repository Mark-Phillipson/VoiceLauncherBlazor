using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoiceLauncherBlazor.Models
{
    public partial class LauncherMultipleLauncherBridge
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Column("LauncherID")]
        public int LauncherId { get; set; }
        [Column("MultipleLauncherID")]
        public int MultipleLauncherId { get; set; }
    }
}
