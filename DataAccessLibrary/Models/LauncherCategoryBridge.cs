using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    [Table("LauncherCategoryBridge")]
    public class LauncherCategoryBridge
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("LauncherID")]
        public int LauncherId { get; set; }

        [Column("CategoryID")]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(LauncherId))]
        [InverseProperty("LauncherCategoryBridges")]
        public virtual Launcher Launcher { get; set; } = null!;

        [ForeignKey(nameof(CategoryId))]
        [InverseProperty("LauncherCategoryBridges")]
        public virtual Category Category { get; set; } = null!;
    }
}
