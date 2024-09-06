using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    public partial class ViewMultipleLauncher
    {
        [Column("ID")]
        public int Id { get; set; }
        [StringLength(70)]
        public required string Description { get; set; }
    }
}
