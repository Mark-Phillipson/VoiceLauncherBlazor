namespace DataAccessLibrary.Models
{
	public class VisualStudioCommand
	{
		public int Id { get; set; }
		public required string Caption { get; set; }
		public required string Command { get; set; }
	}
}
