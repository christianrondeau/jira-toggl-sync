using ChristianRondeau.JiraTogglSync.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChristianRondeau.JiraTogglSync.Tests
{
	[TestClass]
	public class WorkLogEntryTests
	{
		[TestMethod]
		public void CanDisplayNicelyAsString()
		{
			Assert.AreEqual(
				new WorkLogEntry
					{
						Description = "My Entry"
					},
				"My Entry"
				);
		}
	}
}
