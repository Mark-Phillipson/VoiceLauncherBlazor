using DataAccessLibrary.Models.KnowbrainerCommands;
using DataAccessLibrary.XML_Import;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataAccessLibrary.Services
{
	public class TargetApplicationService
	{
		readonly DataSet dataSet = new DataSet();
		public List<TargetApplication> GetTargetApplications(string filename = null)
		{
			DataTable table = LoadDataTable(ref filename, 1);
			List<TargetApplication> targetApplications = table.AsEnumerable().Select(row =>
			new TargetApplication
			{
				Commands_Id = row.Field<int>("Commands_Id"),
				Scope = row.Field<string>("scope"),
				Company = row.Field<string>("company"),
				Module = row.Field<string>("module"),
				ModuleDescription = row.Field<string>("moduleDescription"),
				WindowClass = row.Field<string>("windowClass"),
				WindowTitle = row.Field<string>("windowTitle"),
				KnowbrainerCommands_Id = row.Field<int>("KnowbrainerCommands_Id")
			}
			).ToList();

			return targetApplications;
		}

		private DataTable LoadDataTable(ref string filename, int tableNumber)
		{
			if (Environment.MachineName == "DESKTOP-UROO8T1" || filename == null)
			{
				filename = @"C:\Users\MPhil\AppData\Roaming\KnowBrainer\KnowBrainerCommands\MyKBCommands - Copy.xml";
			}
			FileManagement.LoadXMLDocument(filename, dataSet);
			var table = dataSet.Tables[tableNumber];
			return table;
		}

		public List<VoiceCommand> GetVoiceCommands(string filename = null)
		{
			DataTable table = LoadDataTable(ref filename, 2);
			List<VoiceCommand> voiceCommands = table.AsEnumerable().Select(row =>
			new VoiceCommand
			{
				Command_id = row.Field<int>("Command_Id"),
				Description = row.Field<string>("description"),
				Enabled = row.Field<string>("enabled").ToLower()=="yes"?true:false,
				Group = row.Field<string>("group"),
				Name = row.Field<string>("name"),
				Commands_id = row.Field<int>("Commands_Id")
			}).ToList();
			return voiceCommands;
		}
	}
}
