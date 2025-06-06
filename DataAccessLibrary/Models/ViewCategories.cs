﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    public partial class ViewCategories
    {
        [Column("ID")]
        public int Id { get; set; }
        [StringLength(30)]
        public required string Category { get; set; }
        [Column("Category_Type")]
        [StringLength(255)]
        public required string CategoryType { get; set; }
    }
}
