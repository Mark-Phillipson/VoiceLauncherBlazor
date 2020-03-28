using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoiceLauncherBlazor.Models
{
    public partial class MultipleLauncher
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [StringLength(70)]
        public string Description { get; set; }
    }
}
