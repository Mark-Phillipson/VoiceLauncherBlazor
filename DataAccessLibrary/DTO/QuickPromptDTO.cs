using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.DTO
{
    public partial class QuickPromptDTO
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Type { get; set; } = "";
        
        [Required]
        [StringLength(100)]
        public string Command { get; set; } = "";
        
        [Required]
        [StringLength(4000)]
        public string PromptText { get; set; } = "";
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastModifiedDate { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
