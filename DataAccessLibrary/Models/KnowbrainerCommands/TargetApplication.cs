using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Models.KnowbrainerCommands
{
	public class TargetApplication
	{
		public TargetApplication()
		{
			VoiceCommands = new HashSet<VoiceCommand>();
		}
		// Primary Key
		public int Commands_Id { get; set; }
		public string Scope { get; set; }
		public string Module { get; set; }
		public string Company { get; set; }
		public string ModuleDescription { get; set; }
		public string WindowTitle { get; set; }
		public string WindowClass { get; set; }
		public int KnowbrainerCommands_Id { get; set; }
		public virtual ICollection<VoiceCommand> VoiceCommands { get; set; }

	}
}
