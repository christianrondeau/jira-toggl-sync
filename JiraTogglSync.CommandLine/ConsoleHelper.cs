using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraTogglSync.CommandLine
{
	public class ConsoleHelper
	{
		public static bool Confirm(string message)
		{
			if (!message.TrimEnd().EndsWith("(y/n)"))
				message = message + " (y/n) ";

			Console.WriteLine();
			Console.Write(message);
			var input = Console.ReadLine();

			if (input == "y")
				return true;

			if (input == "n")
				return false;

			Console.Error.WriteLine("Wrong value was provided.");
			return Confirm(message);
		}
	}
}
