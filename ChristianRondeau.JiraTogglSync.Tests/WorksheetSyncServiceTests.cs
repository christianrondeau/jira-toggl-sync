using System;
using System.Collections.Generic;
using System.Linq;
using ChristianRondeau.JiraTogglSync.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Ploeh.SemanticComparison;

namespace ChristianRondeau.JiraTogglSync.Tests
{
	[TestClass]
	public class WorksheetSyncServiceTests
	{
		[TestMethod]
		public void CanGetUniqueJiraIncidentsFromWorkLogEntries()
		{
			var source = Substitute.For<IWorksheetSourceService>();
			source
				.GetEntries(new DateTime(2010, 01, 01), new DateTime(2010, 01, 14))
				.Returns(new[] {new WorkLogEntry {Description = "KEY-123: This is some stuff I'm doing"}});

			var target = Substitute.For<IWorksheetTargetService>();
			target
				.LoadIncidents(Arg.Any<IEnumerable<string>>())
				.Returns(new[] { new Issue { Key = "KEY-123", Summary = "Create the new gizmo" } });

			var service = new WorksheetSyncService(source, target, new[] { "KEY" });
			var suggestions = service.GetSuggestions(new DateTime(2010, 01, 01), new DateTime(2010, 01, 14)).ToArray();

			Assert.AreEqual(suggestions.Length, 1);
			new Likeness<Issue, Issue>(new Issue { Key = "KEY-123", Summary = "Create the new gizmo" }).ShouldEqual(suggestions[0]);
		}
	}
}