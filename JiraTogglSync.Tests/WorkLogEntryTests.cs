using System;
using JiraTogglSync.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JiraTogglSync.Tests
{
	[TestClass]
	public class WorkLogEntryTests
	{
		[TestMethod]
		public void CanDisplayNicelyAsString()
		{
			Assert.AreEqual("2014-03-25 - 01:30:00 - My Entry",
				new WorkLogEntry
					{
						Description = "My Entry",
						Start = new DateTime(2014, 03, 25),
						RoundedDuration = new TimeSpan(0, 1, 30, 0)
					}.ToString()
				);
		}
	}
}
