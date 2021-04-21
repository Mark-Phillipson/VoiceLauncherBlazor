namespace DataAccessLibrary.Models.KnowbrainerCommands
{
	public class VoiceCommandContent
	{
		public int Command_id { get; set; }
		public VoiceCommand VoiceCommand { get; set; }
		public string Type { get; set; }
		public string Content { get; set; }
	}
}
