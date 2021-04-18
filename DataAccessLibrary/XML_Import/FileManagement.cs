using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace DataAccessLibrary.XML_Import
{
	public static class FileManagement
	{
		public static bool LoadXMLDocument(string filename, DataSet dataSet)
		{
			XDocument document = XDocument.Load(filename);
			var commands = document.Descendants("Command").Count();
			var lists = document.Descendants("List").Count();
			if (commands == 0 || lists == 0)
			{
				return false;
			}
			dataSet.Clear();
			dataSet.ReadXmlSchema(filename);
			dataSet.ReadXml(filename);
			if (document == null)
			{
				return false;
			}
			return true;
		}
		public static void ExportSingleCommand(DataSet dataSet, string scope, string module, string name)
		{
			// Get a clone of the original DataSet.
			DataSet cloneSet = dataSet.Clone();

			var commandsId = -1;
			var commandId = -1;
			// Insert code to work with clone of the DataSet.
			foreach (DataRow row in dataSet.Tables[Mapping.KnowbrainerCommandsTable].Rows)
			{
				cloneSet.Tables[Mapping.KnowbrainerCommandsTable].ImportRow(row);
			}
			foreach (DataRow row in dataSet.Tables[Mapping.CommandsTable].Rows)
			{
				if ((string)row.ItemArray[Mapping.Scope] == scope && string.IsNullOrWhiteSpace(module))
				{
					cloneSet.Tables[Mapping.CommandsTable].ImportRow(row);
					commandsId = (int)row.ItemArray[Mapping.PrimaryKey_Commands];
				}
				else if ((string)row.ItemArray[Mapping.Scope] == scope && (string)row.ItemArray[Mapping.Module] == module)
				{
					cloneSet.Tables[Mapping.CommandsTable].ImportRow(row);
					commandsId = (int)row.ItemArray[Mapping.PrimaryKey_Commands];
				}
				if (commandsId > -1)
				{
					break;
				}
			}
			foreach (DataRow row in dataSet.Tables[Mapping.CommandTable].Rows)
			{
				if ((string)row.ItemArray[Mapping.ScriptName] == name && (int)row.ItemArray[Mapping.CommandsFK] == commandsId)
				{
					cloneSet.Tables[Mapping.CommandTable].ImportRow(row);
					commandId = (int)row.ItemArray[Mapping.PrimaryKey_Command];
				}
			}
			foreach (DataRow row in dataSet.Tables[Mapping.ContentTable].Rows)
			{
				if ((int)row.ItemArray[Mapping.CommandFK] == commandId)
				{
					cloneSet.Tables[Mapping.ContentTable].ImportRow(row);
				}
			}
			if (ListManagement.HasLists(name))
			{
				var listId = -1;
				List<string> lists = ListManagement.GetListsName(name);
				foreach (DataRow row in dataSet.Tables[Mapping.ListsTable].Rows)
				{
					cloneSet.Tables[Mapping.ListsTable].ImportRow(row);
				}
				foreach (DataRow row in dataSet.Tables[Mapping.ListTable].Rows)
				{
					foreach (var list in lists.Distinct().ToList())
					{
						if ((string)row.ItemArray[Mapping.Name] == list)
						{
							cloneSet.Tables[Mapping.ListTable].ImportRow(row);
							listId = (int)row.ItemArray[Mapping.PrimaryKey];
							foreach (DataRow valueRow in dataSet.Tables[Mapping.ValueTable].Rows)
							{
								if ((int)valueRow.ItemArray[Mapping.ListFK] == listId)
								{
									cloneSet.Tables[Mapping.ValueTable].ImportRow(valueRow);
								}
							}
						}
					}
				}
			}
			name = name.Replace("<", "(");
			name = name.Replace(">", ")");
			var exportFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $@"\{name}.xml";
			cloneSet.WriteXml(exportFile);
			//MessageBox.Show($"The script has been saved in a separate XML file at the following location: {exportFile}", "Script Exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
			//dataSet.WriteXmlSchema(@"C:\Users\MPhil\AppData\Roaming\KnowBrainer\KnowBrainerCommands\testingScheme.xml");
		}
	}
}
