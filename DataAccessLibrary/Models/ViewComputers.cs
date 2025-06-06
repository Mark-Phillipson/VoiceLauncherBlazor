﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    public partial class ViewComputers
    {
        [Column("ID")]
        public int Id { get; set; }
        [Required]
        [StringLength(20)]
        public required string ComputerName { get; set; }
    }
}
