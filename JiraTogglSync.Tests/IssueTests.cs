using JiraTogglSync.Services;
using NUnit.Framework;

namespace JiraTogglSync.Tests
{
	public class IssueTests
	{
		[Test]
		public void CanDisplayNicelyAsString()
		{
			Assert.AreEqual(
				new Issue { Key = "KEY-123", Summary = "My Issue Summary" }.ToString(),
				"KEY-123: My Issue Summary"
				);
		}
	}
}