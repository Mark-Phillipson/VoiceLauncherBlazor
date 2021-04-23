using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Models.KnowbrainerCommands
{
    public class CommandSet
    {
		public virtual List<TargetApplication> TargetApplications { get; set; }
		public virtual List<SpeechList> SpeechLists { get; set; }
		public string KBFilename { get; set; }
		public string DragonFilename { get; set; }
	}
}
