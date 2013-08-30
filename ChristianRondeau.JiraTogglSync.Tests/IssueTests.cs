using ChristianRondeau.JiraTogglSync.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChristianRondeau.JiraTogglSync.Tests
{
	[TestClass]
	public class IssueTests
	{
		[TestMethod]
		public void CanDisplayNicelyAsString()
		{
			Assert.AreEqual(
				new Issue
					{
						Key = "KEY-123",
						Summary = "My Issue Summary"
					}.ToString(),
				"KEY-123: My Issue Summary"
				);
		}
	}
}