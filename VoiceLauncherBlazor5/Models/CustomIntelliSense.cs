using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoiceLauncherBlazor.Models
{
    public partial class CustomIntelliSense
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Column("LanguageID")]
        public int LanguageId { get; set; }
        [Required]
        [Column("Display_Value")]
        [StringLength(255)]
        public string DisplayValue { get; set; }
        [Required(AllowEmptyStrings = false)]
        [Column("SendKeys_Value")]
        public string SendKeysValue { get; set; }
        [Column("Command_Type")]
        [StringLength(255)]
        public string CommandType { get; set; }
        [Column("CategoryID")]
        public int CategoryId { get; set; }
        [StringLength(255)]
        public string Remarks { get; set; }
        public string Search { get; set; }
        [Column("ComputerID")]
        public int? ComputerId { get; set; }
        [Required(AllowEmptyStrings = false)]
        [StringLength(30)]
        public string DeliveryType { get; set; }

        [ForeignKey(nameof(CategoryId))]
        [InverseProperty(nameof(Models.Category.CustomIntelliSense))]
        public virtual Category Category { get; set; }
        [ForeignKey(nameof(ComputerId))]
        [InverseProperty(nameof(Models.Computer.CustomIntelliSense))]
        public virtual Computer Computer { get; set; }
        [ForeignKey(nameof(LanguageId))]
        [InverseProperty(nameof(Models.Language.CustomIntelliSense))]
        public virtual Language Language { get; set; }
    }
}
