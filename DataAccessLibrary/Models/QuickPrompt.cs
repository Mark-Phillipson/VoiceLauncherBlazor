using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    [Table("QuickPrompts")]
    public class QuickPrompt
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string Type { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string Command { get; set; }
        
        [Required]
        [StringLength(4000)]
        public required string PromptText { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastModifiedDate { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
