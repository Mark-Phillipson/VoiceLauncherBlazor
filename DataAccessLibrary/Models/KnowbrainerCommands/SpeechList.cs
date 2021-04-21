using System.Collections.Generic;

namespace DataAccessLibrary.Models.KnowbrainerCommands
{
	public class SpeechList
	{
		public SpeechList()
		{
			ListValues = new HashSet<ListValue>();
		}
		public int List_Id { get; set; }
		public string Name { get; set; }
		public int Lists_Id { get; set; }
		public virtual ICollection<ListValue> ListValues { get; set; }
	}
}
