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
			Assert.AreEqual("3/25/2014 - 01:30:00 - My Entry",
				new WorkLogEntry
					{
						Description = "My Entry",
						Start = new DateTime(2014, 03, 25),
						RoundedDuration = new TimeSpan(0, 1, 30, 0)
					}.ToString()
				);
		}

		[TestMethod]
		public void CanRound()
		{
			var workLogEntry = new WorkLogEntry
			{
				Description = "My Entry",
				Start = new DateTime(2014, 03, 25, 12, 34, 53),
				Stop = new DateTime(2014, 03, 25, 13, 21, 27),
			};

			workLogEntry.Round(7);

			Assert.AreEqual("3/25/2014 - 00:49:00 - My Entry",
				workLogEntry.ToString());
		}
	}
}
