using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
	public class TalonAlphabet
	{
		public int Id { get; set; }
		[Required]
		[StringLength(20)]
		public required string Letter { get; set; }
		[Url]
		[StringLength(255)]
		public required string PictureUrl { get; set; }
		[Required]
		[StringLength(20)]
		public required string DefaultLetter { get; set; }
		[Url]
		[StringLength(255)]
		public string? DefaultPictureUrl { get; set; }
	}
}
