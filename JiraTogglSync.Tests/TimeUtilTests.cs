using System;
using System.Collections.Generic;
using FluentAssertions;
using JiraTogglSync.Services;
using NUnit.Framework;

namespace JiraTogglSync.Tests;

public class TimeUtilTests
{
	private TimeUtil _util;

	[SetUp]
	public void SetUp()
	{
		_util = new TimeUtil();
	}

	private static readonly IEnumerable<TestCaseData> RoundDateTestCases = new[]
	{
		new TestCaseData(new DateTime(2022, 2, 7, 8, 42, 13, DateTimeKind.Utc), new DateTime(2022, 2, 7, 8, 40, 0, DateTimeKind.Utc)),
		new TestCaseData(new DateTime(2022, 2, 7, 8, 42, 43, DateTimeKind.Utc), new DateTime(2022, 2, 7, 8, 45, 0, DateTimeKind.Utc)),
	};

	[Test]
	[TestCaseSource(nameof(RoundDateTestCases))]
	public void RoundDateTimeToCloses_ShouldRoundTime(DateTime original, DateTime expectedTime)
	{
		var actual = _util.RoundDateTimeToCloses(original, TimeSpan.FromMinutes(5));
		actual.Should().Be(expectedTime);
	}
}