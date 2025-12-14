using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    /// <summary>
    /// Stores information about a tagged face in an image
    /// </summary>
    public partial class FaceTag
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        public int FaceImageId { get; set; }

        [Required(ErrorMessage = "First name is required!")]
        [StringLength(100)]
        public required string FirstName { get; set; }

        [Required]
        public double X { get; set; } // X coordinate (percentage)

        [Required]
        public double Y { get; set; } // Y coordinate (percentage)

        [Required]
        public double Width { get; set; } // Width (percentage)

        [Required]
        public double Height { get; set; } // Height (percentage)

        [ForeignKey("FaceImageId")]
        [InverseProperty("FaceTags")]
        public virtual FaceImage? FaceImage { get; set; }
    }
}
