using DataAccessLibrary.Models.KnowbrainerCommands;
using DataAccessLibrary.XML_Import;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataAccessLibrary.Services
{
	public class CommandSetService
	{
		readonly DataSet dataSet = new DataSet();
		readonly CommandSet commandSet = new CommandSet();
		public CommandSetService(string filename = null,bool viewNew=false)
		{
			dataSet = LoadDataSet(ref filename,viewNew);
		}
		public CommandSet GetCommandSet()
		{
			DataTable table = dataSet.Tables[1];
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
			var results = new List<TargetApplication>();
			foreach (var targetApplication in targetApplications)
			{
				if (targetApplication.Scope == "global")
				{
					targetApplication.ModuleDescription = "*GLOBAL*";
					targetApplication.Module = "*GLOBAL*";
				}
				var voiceCommands = GetVoiceCommands(targetApplication);
				targetApplication.VoiceCommands = voiceCommands;
				results.Add(targetApplication);
			}
			commandSet.TargetApplications = results;
			commandSet.SpeechLists = GetSpeechLists();
			return commandSet;
		}
		List<SpeechList> GetSpeechLists()
		{
			DataTable table = dataSet.Tables[5];
			List<SpeechList> speechLists = table.AsEnumerable().Select(row =>
				   new SpeechList
				   {
					   Lists_Id = row.Field<int>("Lists_Id"),
					   Name = row.Field<string>("name"),
					   List_Id = row.Field<int>("List_Id")
				   }).ToList();
			List<SpeechList> results = new List<SpeechList>();
			foreach (var speechList in speechLists)
			{
				speechList.ListValues = GetListValues(speechList);
				results.Add(speechList);
			}
			return results;
		}
		List<ListValue> GetListValues(SpeechList speechList)
		{
			DataTable table = dataSet.Tables[6];
			List<ListValue> listValues = table.AsEnumerable()
				.Where(v => v.Field<int>("List_Id") == speechList.List_Id)
				.Select(row =>
			   new ListValue
			   {
				   List_Id = row.Field<int>("List_Id"),
				   Value_Text = row.Field<string>("value_Text")
			   }
			).ToList();
			return listValues;
		}
		private DataSet LoadDataSet(ref string filename,bool viewNew=false)
		{

			if (Environment.MachineName == "DESKTOP-UROO8T1" && filename == null && viewNew==false)
			{
				filename = @"C:\Users\MPhil\AppData\Roaming\KnowBrainer\KnowBrainerCommands\MyKBCommands.xml";
			}
			if (Environment.MachineName== "DESKTOP-UROO8T1" && filename== null  && viewNew==true)
			{
				filename = @"C:\Users\MPhil\AppData\Roaming\KnowBrainer\KnowBrainerCommands\MyKBCommandsNewCommands.xml";
			}
			if (filename== null  && viewNew)
			{
				filename = @"D:\home\site\wwwroot\wwwroot\MyKBCommandsNewCommands.xml";
			}
			if (filename== null  && viewNew==false)
			{
				filename = @"D:\home\site\wwwroot\wwwroot\MyKBCommands.xml";
			}
			try
			{
				FileManagement.LoadXMLDocument(filename, dataSet);
			}
			catch (Exception exception)
			{
				throw new Exception($"There was a problem finding the XML file {filename} {exception.Message}");
			}
			commandSet.Filename = filename;
			return dataSet;
		}

		private List<VoiceCommand> GetVoiceCommands(TargetApplication targetApplication)
		{
			DataTable table = dataSet.Tables[2];
			var parentTable = table.ParentRelations;
			List<VoiceCommand> voiceCommands = table.AsEnumerable()
				.Where(v => v.Field<int>("Commands_Id") == targetApplication.Commands_Id)
				.Select(row =>
			new VoiceCommand
			{
				Command_id = row.Field<int>("Command_Id"),
				Description = row.Field<string>("description"),
				Enabled = row.Field<string>("enabled").ToLower() == "true" ? true : false,
				Group = row.Field<string>("group"),
				Name = row.Field<string>("name"),
				Commands_id = row.Field<int>("Commands_Id"),
				TargetApplication = targetApplication
			}).ToList();
			List<VoiceCommand> results = new List<VoiceCommand>();
			foreach (var voiceCommand in voiceCommands)
			{
				voiceCommand.VoiceCommandContents = GetVoiceCommandContents(voiceCommand);
				results.Add(voiceCommand);
			}
			return results;
		}
		private List<VoiceCommandContent> GetVoiceCommandContents(VoiceCommand voiceCommand)
		{
			DataTable table = dataSet.Tables[3];
			List<VoiceCommandContent> voiceCommandContents = table.AsEnumerable()
				.Where(v => v.Field<int>("Command_Id") == voiceCommand.Command_id)
				.Select(row =>
				 new VoiceCommandContent
				 {
					 Command_id = row.Field<int>("Command_Id"),
					 Content = row.Field<string>("content_Text"),
					 Type = row.Field<string>("type")
				 }
			).ToList();
			return voiceCommandContents;
		}
	}
}
