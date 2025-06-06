﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    public partial class HtmlTag
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [StringLength(255)]
        public required string Tag { get; set; }
        [StringLength(255)]
        public string? Description { get; set; }
        [StringLength(255)]
        public required string ListValue { get; set; }
        public bool Include { get; set; }
        [StringLength(255)]
        public string? SpokenForm { get; set; }
    }
}
