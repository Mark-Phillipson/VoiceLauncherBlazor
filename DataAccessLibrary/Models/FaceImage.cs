using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    /// <summary>
    /// Stores an image that contains faces for tagging.
    /// Note: ImageData stores base64-encoded images in the database for simplicity.
    /// For production with many large images, consider implementing a storage abstraction
    /// that can use file system, Azure Blob Storage, or AWS S3.
    /// </summary>
    public partial class FaceImage
    {
        public FaceImage()
        {
            FaceTags = new HashSet<FaceTag>();
        }

        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Image name is required!")]
        [StringLength(200)]
        public required string ImageName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public required string ImageData { get; set; } // Base64 encoded image

        [StringLength(50)]
        public string? ContentType { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;

        [InverseProperty("FaceImage")]
        public virtual ICollection<FaceTag> FaceTags { get; set; }
    }
}
