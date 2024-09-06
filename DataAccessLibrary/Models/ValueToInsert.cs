using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace DataAccessLibrary.Models
{

    [Table("ValuesToInsert")]
    public partial class ValuesToInsert

    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Required]
        [StringLength(255)]
        [Column("ValueToInsert")]
        public required string ValueToInsertValue { get; set; }
        [Required]
        [StringLength(255)]
        public required string Lookup { get; set; }
        [StringLength(255)]
        public string? Description { get; set; }
    }
}
