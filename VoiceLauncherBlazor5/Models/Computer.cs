using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoiceLauncherBlazor.Models
{
    public partial class Computer
    {
        public Computer()
        {
            CustomIntelliSense = new HashSet<CustomIntelliSense>();
            Launcher = new HashSet<Launcher>();
        }

        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Required]
        [StringLength(20)]
        public string ComputerName { get; set; }

        [InverseProperty("Computer")]
        public virtual ICollection<CustomIntelliSense> CustomIntelliSense { get; set; }
        [InverseProperty("Computer")]
        public virtual ICollection<Launcher> Launcher { get; set; }
    }
}
