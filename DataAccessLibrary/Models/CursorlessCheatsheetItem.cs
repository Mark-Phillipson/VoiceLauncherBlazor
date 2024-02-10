using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    public class CursorlessCheatsheetItem
    {
        public int Id { get; set; }
        [Required]
        public string SpokenForm { get; set; }
        public string Meaning { get; set; }
        public string CursorlessType { get; set; }
    }
}
