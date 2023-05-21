
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.DTO
{
    public partial class GrammarNameDTO
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(40)]
        public string NameOfGrammar { get; set; } = "";
    }
}