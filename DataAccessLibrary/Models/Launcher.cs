using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    public partial class Launcher
    {
        public Launcher()
        {
            LaunchersMultipleLauncherBridges = new HashSet<LauncherMultipleLauncherBridge>();
            LauncherCategoryBridges = new HashSet<LauncherCategoryBridge>();
        }
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = null!;
        [StringLength(255)]
        public required string CommandLine { get; set; } = null!;
        [StringLength(255)]
        public string? WorkingDirectory { get; set; }
        [StringLength(255)]
        public string? Arguments { get; set; }
        // Keep CategoryId temporarily for migration purposes
        [Column("CategoryID")]
        public int CategoryId { get; set; }
        [Column("ComputerID")]
        public int? ComputerId { get; set; }
        [StringLength(30)]
        public string Colour { get; set; } = "#000080";
        [StringLength(100)]
        public string? Icon { get; set; }
        public bool Favourite { get; set; }
        public int SortOrder { get; set; }
        
        // Keep the Category property temporarily for migration
        [ForeignKey(nameof(CategoryId))]
        [InverseProperty(nameof(Models.Category.Launchers))]
        public virtual Category Category { get; set; } = null!;
        
        [ForeignKey(nameof(ComputerId))]
        [InverseProperty(nameof(Models.Computer.Launcher))]
        public virtual Computer? Computer { get; set; }
        public virtual ICollection<LauncherMultipleLauncherBridge> LaunchersMultipleLauncherBridges { get; set; }
        
        // Add the new collection property for the bridge
        [InverseProperty(nameof(LauncherCategoryBridge.Launcher))]
        public virtual ICollection<LauncherCategoryBridge> LauncherCategoryBridges { get; set; }
    }
}
