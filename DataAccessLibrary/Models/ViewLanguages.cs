using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    public partial class ViewLanguages
    {
        [Column("ID")]
        public int Id { get; set; }
        [Required]
        [StringLength(25)]
        public required string Language { get; set; }
        public bool Active { get; set; }
    }
}
