using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    public partial class ViewCustomIntelliSense
    {
        [Column("ID")]
        public int Id { get; set; }
        [Column("LanguageID")]
        public int LanguageId { get; set; }
        [Required]
        [Column("Display_Value")]
        [StringLength(255)]
        public required string DisplayValue { get; set; }
        [Column("SendKeys_Value")]
        [StringLength(4000)]
        public string? SendKeysValue { get; set; }
        [Column("Command_Type")]
        [StringLength(255)]
        public string? CommandType { get; set; }
        [Column("CategoryID")]
        public int CategoryId { get; set; }
        [StringLength(255)]
        public string? Remarks { get; set; }
        public string? Search { get; set; }
        [Column("ComputerID")]
        public int? ComputerId { get; set; }
        [Required]
        [StringLength(30)]
        public required string DeliveryType { get; set; }
    }
}
