﻿using DataAccessLibrary.Models.KnowbrainerCommands;
using DataAccessLibrary.XML_Import;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DataAccessLibrary.Services
{
	public class CommandSetService
	{
		readonly DataSet dataSetKB = new DataSet();
		readonly DataSet dataSetDragon = new DataSet();
		XElement MyCommands = new XElement("MyCommands");
		readonly CommandSet commandSet = new CommandSet();
		public CommandSetService(string? knowbrainerScriptFileName = null, string? dragonScriptFileName = null, bool viewNew = false)
		{
			if (knowbrainerScriptFileName != null)
			{
				dataSetKB = LoadDataSet(knowbrainerScriptFileName, viewNew, true);
			}
			if (dragonScriptFileName != null)
			{
				dataSetDragon = LoadDataSet(dragonScriptFileName, viewNew, false);
			}
		}
		public CommandSet GetCommandSet()
		{
			DataTable table = dataSetKB.Tables[1];
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
				var voiceCommands = GetVoiceCommands(targetApplication, true);
				targetApplication.VoiceCommands = voiceCommands;
				results.Add(targetApplication);
			}
			commandSet.TargetApplications = results;
			commandSet.SpeechLists = GetSpeechLists(true);
			// Now try Dragon
			table = dataSetDragon.Tables[1];
			DataColumnCollection dataColumnCollection = table.Columns;
			targetApplications = table.AsEnumerable().Select(row =>
			new TargetApplication
			{
				Commands_Id = row.Field<int>("Commands_Id"),
				Scope = row.Field<string>("type"),
				Company = row.Field<string>("company"),
				Module = row.Field<string>("module"),
				ModuleDescription = row.Field<string>("description"),
				WindowClass = dataColumnCollection.Contains("class") ? row.Field<string>("class") : null,
				WindowTitle = dataColumnCollection.Contains("caption") ? row.Field<string>("caption") : null,
				CommandSource = "Dragon",
				KnowbrainerCommands_Id = row.Field<int>("MyCommands_Id")
			}
			).ToList();
			foreach (var targetApplication in targetApplications)
			{
				if (targetApplication.Scope == "global")
				{
					targetApplication.ModuleDescription = "*GLOBAL*";
					targetApplication.Module = "*GLOBAL*";
				}
				var voiceCommands = GetVoiceCommands(targetApplication, false);
				targetApplication.VoiceCommands = voiceCommands;
				commandSet.TargetApplications.Add(targetApplication);
			}
			var speechLists = GetSpeechLists(false);
			if (speechLists != null)
			{
				commandSet.SpeechLists.AddRange(speechLists);
			}
			return commandSet;
		}
		List<SpeechList> GetSpeechLists(bool isKB)
		{
			DataTable? table;
			if (isKB)
			{
				table = dataSetKB.Tables[5];
			}
			else
			{
				if (dataSetDragon == null)
				{
					return new List<SpeechList>();
				}
				table = dataSetDragon.Tables["List"];
			}
			if (table == null)
			{
				return new List<SpeechList>();
			}
			List<SpeechList> speechLists = table.AsEnumerable().Select(static row =>
				   new SpeechList
				   {
					   Lists_Id = row.Field<int>("Lists_Id"),
					   Name = row.Field<string>("name")!,
					   List_Id = row.Field<int>("List_Id")
				   }).ToList();
			List<SpeechList> results = new List<SpeechList>();
			foreach (var speechList in speechLists)
			{
				speechList.ListValues = GetListValues(speechList, isKB);
				results.Add(speechList);
			}
			return results;
		}
		List<ListValue> GetListValues(SpeechList speechList, bool isKB)
		{
			if (isKB)
			{
				DataTable table = dataSetKB.Tables[6];
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
			else
			{
				DataTable? table = dataSetDragon.Tables["Value"];
				if (table == null)
				{
					return new List<ListValue>();
				}
				List<ListValue> listValues = table.AsEnumerable()
					.Where(v => v.Field<int>("List_Id") == speechList.List_Id)
					.Select(row =>
				   new ListValue
				   {
					   List_Id = row.Field<int>("List_Id"),
					   Value_Text = row.Field<string>("Value_Text")
				   }
				).ToList();
				return listValues;

			}
		}
		private DataSet LoadDataSet(string filename, bool viewNew = false, bool isKB = false)
		{
			if (isKB)
			{
				if (Environment.MachineName == "J40L4V3" && filename == null && viewNew == false)
				{
					filename = @"C:\Users\MPhil\AppData\Roaming\KnowBrainer\KnowBrainerCommands\MyKBCommands.xml";
				}
				if (Environment.MachineName == "J40L4V3" && filename == null && viewNew == true)
				{
					filename = @"C:\Users\MPhil\AppData\Roaming\KnowBrainer\KnowBrainerCommands\MyKBCommandsNewCommands.xml";
				}
				if (filename == null && viewNew)
				{
					filename = @"D:\home\site\wwwroot\wwwroot\MyKBCommandsNewCommands.xml";
				}
				if (filename == null && viewNew == false)
				{
					filename = @"D:\home\site\wwwroot\wwwroot\MyKBCommands.xml";
				}
				try
				{
					if (filename != null)
					{
						FileManagement.LoadXMLDocument(filename, dataSetKB, true, commandSet);
					}
				}
				catch (Exception exception)
				{
					throw new Exception($"There was a problem finding the XML file {filename} {exception.Message}");
				}
				commandSet.KBFilename = Path.GetFileName(filename);
				return dataSetKB;
			}
			else
			{
				if (Environment.MachineName == "J40L4V3" && filename == null)
				{
					filename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\MyCommands.xml";
				}
				else if (filename == null)
				{
					filename = @"D:\home\site\wwwroot\wwwroot\MyCommands.xml";
				}
			}
			try
			{
				FileManagement.LoadXMLDocument(filename, dataSetDragon, false, commandSet);
				MyCommands = XElement.Load(filename);
			}
			catch (Exception exception)
			{
				throw new Exception($"There was a problem finding the XML file {filename} {exception.Message}");
			}
			commandSet.DragonFilename = Path.GetFileName(filename);
			return dataSetDragon;
		}
		private List<VoiceCommand> GetVoiceCommands(TargetApplication targetApplication, bool isKB)
		{
			DataTable table;
			if (isKB)
			{
				table = dataSetKB.Tables[2];
			}
			else
			{
				table = dataSetDragon.Tables[2];
			}
			List<VoiceCommand> voiceCommands = table.AsEnumerable()
				.Where(v => v.Field<int>("Commands_Id") == targetApplication.Commands_Id)
				.Select(row =>
			new VoiceCommand
			{
				Command_id = row.Field<int>("Command_Id"),
				Description = row.Field<string>("description"),
				Enabled = row.Field<string>("enabled")!.ToLower() == "true" ? true : false,
				Group = row.Field<string>("group"),
				Name = row.Field<string>("name"),
				States = isKB ? null : row.Field<string>("states"),
				Commands_id = row.Field<int>("Commands_Id"),
				TargetApplication = targetApplication
			}).ToList();
			List<VoiceCommand> results = new List<VoiceCommand>();
			foreach (var voiceCommand in voiceCommands)
			{
				voiceCommand.VoiceCommandContents = GetVoiceCommandContents(voiceCommand, isKB);
				results.Add(voiceCommand);
			}
			return results;
		}
		private List<VoiceCommandContent> GetVoiceCommandContents(VoiceCommand voiceCommand, bool isKb)
		{
			DataTable table;
			if (isKb)
			{
				table = dataSetKB.Tables[3];
				try
				{
					List<VoiceCommandContent> voiceCommandContents = table.AsEnumerable()
					.Where(v => v.Field<int>("Command_Id") == voiceCommand.Command_id)
					.Select(row =>
					 new VoiceCommandContent
					 {
						 Command_id = row.Field<int>("Command_Id"),
						 Content = isKb ? row.Field<string>("content_Text") : row.Field<string>("Command"),
						 Type = row.Field<string>("type")
					 }
					).ToList();
					return voiceCommandContents;

				}
				catch (Exception exception)
				{
					throw new Exception($" problem getting voice command content {exception.Message}");
				}
			}
			else
			{
				table = dataSetDragon.Tables[3];
				var result = from elements in MyCommands.Descendants("Commands").Descendants("Command")
							 where (string?)elements.Attribute("name") == voiceCommand.Name
							 select (string?)elements.Descendants("contents").FirstOrDefault();

				var type = from elements in MyCommands.Descendants("Commands").Descendants("Command")
						   where (string?)elements.Attribute("name") == voiceCommand.Name
						   select elements.Descendants("contents").Attributes("type");

				var value = result.FirstOrDefault();
				VoiceCommandContent voiceCommandContent = new VoiceCommandContent();
				voiceCommandContent.Content = value;
				voiceCommandContent.Type = type.FirstOrDefault()?.FirstOrDefault()?.Value;
				var returnValue = new List<VoiceCommandContent>();
				returnValue.Add(voiceCommandContent);
				return returnValue;

			}
		}
	}
}
