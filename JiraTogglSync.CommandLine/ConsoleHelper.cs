using System;

namespace JiraTogglSync.CommandLine;

public class ConsoleHelper
{
	public static bool Confirm(string message)
	{
		while (true)
		{
			Console.WriteLine();
			Console.Write(message + " (y/n) ");

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