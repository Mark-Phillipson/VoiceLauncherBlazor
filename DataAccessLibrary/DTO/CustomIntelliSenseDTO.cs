
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.DTO
{
    public partial class CustomIntelliSenseDTO
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int LanguageId { get; set; }
        [Required]
        [StringLength(255)]
        public string DisplayValue { get; set; } = "";
        public string SendKeysValue { get; set; }
        [StringLength(255)]
        public string CommandType { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [StringLength(255)]
        public string Remarks { get; set; }
        public string Search { get; set; }
        public int? ComputerId { get; set; }
        [Required]
        [StringLength(30)]
        public string DeliveryType { get; set; } = "";
    }
}