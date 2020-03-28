using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoiceLauncherBlazor.Models
{
    public partial class PropertyTabPositions
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Required]
        [StringLength(60)]
        public string ObjectName { get; set; }
        [Required]
        [StringLength(60)]
        public string PropertyName { get; set; }
        public int NumberOfTabs { get; set; }
    }
}
