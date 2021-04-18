using DataAccessLibrary.Models.KnowbrainerCommands;
using DataAccessLibrary.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace UnitTestProject
{
	[TestClass]
	public class TargetApplicationServiceTests
	{
		[TestMethod]
		public void GetTargetApplicationsTest()
		{
			TargetApplicationService targetApplicationService = new TargetApplicationService();
			var result=targetApplicationService.GetTargetApplications();
			Assert.AreEqual(80, result.Count);
		}
		[TestMethod]
		public void GetVoiceCommandsTest()
		{
			TargetApplicationService targetApplicationService = new TargetApplicationService();
			List<VoiceCommand> result= targetApplicationService.GetVoiceCommands();
			Assert.AreEqual(4108, result.Count);

		}
	}
}
