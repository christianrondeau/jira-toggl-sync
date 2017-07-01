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
			if (Console.ReadKey().KeyChar == 'y')
			{
				return true;
			}
			return false;
		}
	}
}
