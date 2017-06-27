﻿using System;
using System.Collections.Generic;
using System.Linq;
using JiraTogglSync.Services;
using NSubstitute;
using NUnit.Framework;
using Ploeh.SemanticComparison;

namespace JiraTogglSync.Tests
{
	public class WorksheetSyncServiceTests
	{
		[Test]
		public void CanGetUniqueJiraIncidentsFromWorkLogEntries()
		{
			var source = Substitute.For<IExternalWorksheetRepository>();
			source
				.GetEntries(new DateTime(2010, 01, 01), new DateTime(2010, 01, 14), Arg.Any<List<string>>())
				.Returns(new[] {new WorkLogEntry {Description = "KEY-123: This is some stuff I'm doing"}});

			var target = Substitute.For<IJiraRepository>();
			target
				.LoadIssues(Arg.Any<IEnumerable<string>>())
				.Returns(new[] { new Issue { Key = "KEY-123", Summary = "Create the new gizmo" } });

			var service = new WorksheetSyncService(source, target);
			var suggestions = service.GetSuggestions(new DateTime(2010, 01, 01), new DateTime(2010, 01, 14)).ToArray();

			Assert.AreEqual(suggestions.Length, 1);
			Assert.AreEqual(suggestions[0].Key, "KEY-123");
			Assert.AreEqual(suggestions[0].Summary, "Create the new gizmo");
			Assert.AreEqual(suggestions[0].WorkLog.Count, 1);
			Assert.AreEqual(suggestions[0].WorkLog[0].Description, "This is some stuff I'm doing");
		}
	}
}