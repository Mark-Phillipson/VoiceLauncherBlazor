using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    public partial class CurrentWindow
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        public int Handle { get; set; }
    }
}
