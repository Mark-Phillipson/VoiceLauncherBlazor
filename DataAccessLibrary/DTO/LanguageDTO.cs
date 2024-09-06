using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.DTO
{
   public partial class LanguageDTO
   {
      [Key]
      public int Id { get; set; }
      [Required]
      [StringLength(25)]
      public required string LanguageName { get; set; } = "New Language";
      [Required]
      public bool Active { get; set; } = true;
      [StringLength(40)]
      public required string Colour { get; set; } = "#000000";
   }
}