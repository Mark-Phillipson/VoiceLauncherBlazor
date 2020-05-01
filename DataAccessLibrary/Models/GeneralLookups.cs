using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    public partial class GeneralLookup
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Required]
        [Column("Item_Value")]
        [StringLength(255)]
        public string ItemValue { get; set; }
        [Required]
        [StringLength(255)]
        public string Category { get; set; }
        public int? SortOrder { get; set; }
        [StringLength(255)]
        public string DisplayValue { get; set; }
    }
}
