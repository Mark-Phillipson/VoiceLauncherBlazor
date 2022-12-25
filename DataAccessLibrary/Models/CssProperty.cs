using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    public class CssProperty
    {
        public int Id { get; set; }
        [StringLength( 100 )]
        [Required]
        public string PropertyName { get; set; }
        [StringLength( 255 )]
        public string Description { get; set; }
    }
}