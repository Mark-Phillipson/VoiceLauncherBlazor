using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    public partial class Logins
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Required]
        [StringLength(30)]
        public string Name { get; set; }
        [StringLength(255)]
        public string Username { get; set; }
        [StringLength(255)]
        public string Password { get; set; }
        [StringLength(255)]
        public string Description { get; set; }
    }
}
