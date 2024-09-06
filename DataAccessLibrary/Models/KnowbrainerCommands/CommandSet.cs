using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Models.KnowbrainerCommands
{
	public class CommandSet
	{
		public virtual List<TargetApplication> TargetApplications { get; set; } = new List<TargetApplication>();
		public virtual List<SpeechList> SpeechLists { get; set; } = new List<SpeechList>();
		public string? KBFilename { get; set; }
		public int KBScripts { get; set; }
		public int KBLists { get; set; }
		public string? DragonFilename { get; set; }
		public int DragonScripts { get; set; }
		public int DragonLists { get; set; }
	}
}
