
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.DTO
{
    public partial class ValueToInsertDTO
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(255)]
        public string ValueToInsertValue { get; set; } = "";
        [Required]
        [StringLength(255)]
        public string Lookup { get; set; } = "";
        [StringLength(255)]
        public string Description { get; set; }
    }
}