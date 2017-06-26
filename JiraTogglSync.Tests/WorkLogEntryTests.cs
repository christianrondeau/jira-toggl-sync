using System;
using System.Globalization;
using System.Threading;
using JiraTogglSync.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JiraTogglSync.Tests
{
	[TestClass]
	public class WorkLogEntryTests
	{
		[TestInitialize]
		public void GivenEnUsCulture()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
		}

		[TestMethod]
		public void CanDisplayNicelyAsString()
		{
			Assert.AreEqual("2014-03-25 - 01:30:00 - My Entry",
				new WorkLogEntry
				{
					Description = "My Entry",
					Start = new DateTime(2014, 03, 25),
					RoundedDuration = new TimeSpan(0, 1, 30, 0)
				}.ToString());
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

			Assert.AreEqual("2014-03-25 - 00:49:00 - My Entry",
				workLogEntry.ToString());
		}
	}
}
