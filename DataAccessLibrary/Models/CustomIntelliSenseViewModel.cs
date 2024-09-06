using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    public class CustomIntelliSenseViewModel
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Column("LanguageID")]
        public int LanguageId { get; set; }
        [Display(Name = "Language Name")]
        public required string LanguageName { get; set; }
        [Required]
        [Column("Display_Value")]
        [StringLength(255)]
        public string? DisplayValue { get; set; }
        [Required(AllowEmptyStrings = false)]
        [Column("SendKeys_Value")]
        public string? SendKeysValue { get; set; }
        [Column("Command_Type")]
        [StringLength(255)]
        public string? CommandType { get; set; }
        [Column("CategoryID")]
        public int CategoryId { get; set; }
        [Display(Name = "Category Name")]
        public string? CategoryName { get; set; }
        [StringLength(255)]
        public string? Remarks { get; set; }
        [Required(AllowEmptyStrings = false)]
        [StringLength(30)]
        public string? DeliveryType { get; set; }


    }
}

