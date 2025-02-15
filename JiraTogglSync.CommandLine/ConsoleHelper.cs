using System;
using System.Collections.Generic;
using System.Linq;
using JiraTogglSync.Services;
using Socolin.ANSITerminalColor;

namespace JiraTogglSync.CommandLine;

public class ConsoleHelper
{
	public static bool ConfirmAdd(ICollection<WorkLogEntry> workLogs)
	{
		return Confirm("NEW", workLogs, "ADD", AnsiColor.Foreground(Terminal256ColorCodes.GreenC2));
	}
	public static bool ConfirmDeleteDuplicates(ICollection<WorkLogEntry> workLogs)
	{
		return Confirm("DUPLICATE", workLogs, "DELETE", AnsiColor.Foreground(Terminal256ColorCodes.DarkRedC52));
	}
	public static bool ConfirmDeleteOrphaned(ICollection<WorkLogEntry> workLogs)
	{
		return Confirm("ORPHANED", workLogs, "DELETE", AnsiColor.Foreground(Terminal256ColorCodes.DarkRedC52));
	}
	public static bool ConfirmUpdate(ICollection<WorkLogEntry> workLogs)
	{
		return Confirm("CHANGED", workLogs, "nUPDATE", AnsiColor.Foreground(Terminal256ColorCodes.DarkOrange3C166));
	}

	public static bool Confirm(
		string kind,
		ICollection<WorkLogEntry> workLogs,
		string action,
		AnsiColor foreground
	)
	{
		while (true)
		{
			Console.WriteLine();
			Console.WriteLine($"\u2550\u2550\u2550 {foreground.Colorize(kind)} ({workLogs.Sum(x => x.TimeSpent.TotalHours):f2} h) \u2550\u2550\u2550");
			foreach (var workLogEntry in workLogs)
				Console.WriteLine(workLogEntry.ToString());
			Console.WriteLine($"{action} {workLogs.Count} {kind} work log entries ? (y/n)");

			var input = Console.ReadLine();
			switch (input)
			{
				case "y":
					return true;
				case "n":
					return false;
				default:
					Console.Error.WriteLine("Wrong value was provided.");
					break;
			}
		}
	}
}