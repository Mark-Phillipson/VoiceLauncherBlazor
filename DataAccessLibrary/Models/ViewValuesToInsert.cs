﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    public partial class ViewValuesToInsert
    {
        [Column("ID")]
        public int Id { get; set; }
        [Required]
        [StringLength(255)]
        public required string ValueToInsert { get; set; }
        [Required]
        [StringLength(255)]
        public required string Lookup { get; set; }
        [StringLength(255)]
        public string? Description { get; set; }
    }
}
