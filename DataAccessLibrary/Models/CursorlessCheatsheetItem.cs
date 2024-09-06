using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
	public class CursorlessCheatsheetItem
	{
		public int Id { get; set; }
		[Required]
		public required string SpokenForm { get; set; }
		public string? Meaning { get; set; }
		public string? CursorlessType { get; set; }
		[StringLength(255)]
		public string? YoutubeLink { get; set; }
	}
}
