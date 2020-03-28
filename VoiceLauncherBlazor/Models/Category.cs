using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoiceLauncherBlazor.Models
{
    public partial class Category
    {
        public Category()
        {
            CustomIntelliSense = new HashSet<CustomIntelliSense>();
            Launcher = new HashSet<Launcher>();
        }

        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [StringLength(30)]
        [Column("Category")]
        [Required(ErrorMessage = "The Category Name is required dick head!")]
        public string CategoryName { get; set; }
        [Column("Category_Type")]
        [StringLength(255)]
        [Required(ErrorMessage = "The Category Type is required dick head!")]
        public string CategoryType { get; set; }

        [InverseProperty("Category")]
        public virtual ICollection<CustomIntelliSense> CustomIntelliSense { get; set; }
        [InverseProperty("Category")]
        public virtual ICollection<Launcher> Launcher { get; set; }
    }
}
