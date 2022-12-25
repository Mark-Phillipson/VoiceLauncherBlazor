using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    public partial class Language
    {
        public Language()
        {
            CustomIntelliSense = new HashSet<CustomIntelliSense>();
        }

        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Required]
        [StringLength(25)]
        [Column("Language")]
        public string LanguageName { get; set; }
        public bool Active { get; set; }
        [StringLength(40)]
        public string Colour { get; set; }
        [InverseProperty("Language")]
        public virtual ICollection<CustomIntelliSense> CustomIntelliSense { get; set; }
    }
}
