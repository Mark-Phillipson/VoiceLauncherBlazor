using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoiceLauncherBlazor.Models
{
    public partial class ViewComputers
    {
        [Column("ID")]
        public int Id { get; set; }
        [Required]
        [StringLength(20)]
        public string ComputerName { get; set; }
    }
}
