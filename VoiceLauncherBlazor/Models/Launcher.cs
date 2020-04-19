using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoiceLauncherBlazor.Models
{
    public partial class Launcher
    {
        public Launcher()
        {
            LaunchersMultipleLauncherBridges = new HashSet<LauncherMultipleLauncherBridge>();
        }
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [StringLength(255)]
        public string CommandLine { get; set; }
        [Column("CategoryID")]
        public int CategoryId { get; set; }
        [Column("ComputerID")]
        public int? ComputerId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        [InverseProperty(nameof(Models.Category.Launcher))]
        public virtual Category Category { get; set; }
        [ForeignKey(nameof(ComputerId))]
        [InverseProperty(nameof(Models.Computer.Launcher))]
        public virtual Computer Computer { get; set; }
        public virtual ICollection<LauncherMultipleLauncherBridge> LaunchersMultipleLauncherBridges { get; set; }
    }
}
