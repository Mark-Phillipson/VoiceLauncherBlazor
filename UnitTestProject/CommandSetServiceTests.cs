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
		public void GetCommandSetTest()
		{
			CommandSetService commandSetService = new CommandSetService();
			var result=commandSetService.GetCommandSet();
			Assert.IsTrue(result.TargetApplications.Count>0);
			Assert.IsTrue(result.SpeechLists.Count>0);
		}
	}
}
