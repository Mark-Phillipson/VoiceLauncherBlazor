using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.DTO
{
	public partial class TalonAlphabetDTO
	{
		[Key]
		public int Id { get; set; }
		[Required]
		[StringLength(20)]
		public string Letter { get; set; } = "";
		[StringLength(255)]
		public string PictureUrl { get; set; }
		[Required]
		[StringLength(20)]
		public string DefaultLetter { get; set; } = "";
		[StringLength(255)]
		public string DefaultPictureUrl { get; set; }
	}
}