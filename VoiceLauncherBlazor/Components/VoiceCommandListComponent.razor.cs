using DataAccessLibrary.Models.KnowbrainerCommands;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace VoiceLauncherBlazor.Components
{
	public partial class VoiceCommandListComponent
	{
		[Parameter] public CommandSet CommandSet { get; set; }
		[Parameter] public VoiceCommand VoiceCommand { get; set; }
		[Parameter] public bool Show { get; set; }
		List<string> ListNames { get; set; } = new List<string>();
		protected override void OnParametersSet()
		{
			var name = VoiceCommand.Name;
			var startCharacter = "<";
			var endCharacter = ">";
			if (VoiceCommand.TargetApplication.CommandSource == "Dragon")
			{
				startCharacter = "[";
				endCharacter = "]";
			}
			ListNames = new List<string>();
			if (name.Contains(startCharacter) && name.Contains(endCharacter) )
			{
				do
				{
					var position1 = name.IndexOf(startCharacter);
					var position2 = name.IndexOf(endCharacter);
					position1++;
					var listName = name.Substring(position1, position2 - position1);
					ListNames.Add(listName);
					name = name.Substring(position2 + 1);
				} while (name.Contains(startCharacter) && name.Contains(endCharacter));
			}
		}
	}
}