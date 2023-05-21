
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.DTO
{
    public partial class IdiosyncrasyDTO
    {
        [Key]
        public int Id { get; set; }
        [StringLength(60)]
        public string FindString { get; set; }
        [StringLength(60)]
        public string ReplaceWith { get; set; }
        [StringLength(60)]
        public string StringFormattingMethod { get; set; } = "Just Replace";

    }
}