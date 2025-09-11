
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.DTOs
{
    public partial class TransactionTypeMappingDTO
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string MyTransactionType { get; set; } = "";
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "";
    }
}