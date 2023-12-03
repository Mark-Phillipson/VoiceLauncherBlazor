using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
	public class TalonAlphabet
	{
		public int Id { get; set; }
		[Required]
		[StringLength(20)]
		public string Letter { get; set; }
		[Url]
		[StringLength(255)]
		public string PictureUrl { get; set; }
		[Required]
		[StringLength(20)]
		public string DefaultLetter { get; set; }
		[Url]
		[StringLength(255)]
		public string DefaultPictureUrl { get; set; }
	}
}
