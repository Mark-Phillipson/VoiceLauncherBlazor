using DataAccessLibrary.Models.KnowbrainerCommands;
using DataAccessLibrary.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnitTestProject
{
	[TestClass]
	public class CommandSetServiceTests
	{
		[TestMethod]
		public void GetCommandSetTestLoadingScripts()
		{
			var currentDirectory = Environment.CurrentDirectory;
			CommandSetService commandSetService = new CommandSetService($@"{currentDirectory}/Files/MyKBCommands.xml", $@"{currentDirectory}/Files/Productivity.xml",true);
			var result=commandSetService.GetCommandSet();
			Assert.IsTrue(result.TargetApplications.Count>0);
			Assert.IsTrue(result.SpeechLists.Count>0);
		}
	}
}
