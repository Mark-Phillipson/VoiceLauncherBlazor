using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    [Table("__MigrationHistory")]
    public partial class MigrationHistory
    {
        [Key]
        [StringLength(150)]
        public required string MigrationId { get; set; }
        [Key]
        [StringLength(300)]
        public required string ContextKey { get; set; }
        [Required]
        public required byte[] Model { get; set; }
        [Required]
        [StringLength(32)]
        public required string ProductVersion { get; set; }
    }
}
