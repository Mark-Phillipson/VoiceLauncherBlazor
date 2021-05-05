using DataAccessLibrary.Models.KnowbrainerCommands;
using DataAccessLibrary.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace UnitTestProject
{
	[TestClass]
	public class CommandSetServiceTests
	{
		[TestMethod]
		public void GetCommandSetTestDragonScripts()
		{
			CommandSetService commandSetService = new CommandSetService( null , @"C:\Users\MPhil\OneDrive\Documents\Productivity.xml",true);
			var result=commandSetService.GetCommandSet();
			Assert.IsTrue(result.TargetApplications.Count>0);
			Assert.IsTrue(result.SpeechLists.Count>0);
		}
		[TestMethod]
		public void GetCommandSetTestKnowBrainerScripts()
		{
			CommandSetService commandSetService = new CommandSetService(@"C:\Users\MPhil\AppData\Roaming\KnowBrainer\KnowBrainerCommands\MyKBCommands_2016.xml",  null ,true);
			var result=commandSetService.GetCommandSet();
			Assert.IsTrue(result.TargetApplications.Count>0);
			Assert.IsTrue(result.SpeechLists.Count>0);
		}
	}
}
