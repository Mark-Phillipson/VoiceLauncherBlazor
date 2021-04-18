﻿using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models.KnowbrainerCommands
{
	public class VoiceCommand
	{
		public VoiceCommand()
		{

		}
		public int Command_id { get; set; }
		public string Description { get; set; }
		public string Name { get; set; }
		public string Group { get; set; }
		public bool Enabled { get; set; }
		[Display(Name = "Target Application")]
		public int Commands_id { get; set; }
		[Display(Name = "Target Application")]
		public TargetApplication TargetApplication { get; set; }
	}
}
