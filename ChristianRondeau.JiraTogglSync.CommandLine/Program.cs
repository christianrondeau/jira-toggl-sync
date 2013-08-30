using System;
using ChristianRondeau.JiraTogglSync.Services;

namespace ChristianRondeau.JiraTogglSync.CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            var togglApiKey = ConfigurationHelper.GetEncryptedValueFromConfig("toggl-api-key", () => AskFor("Toggl API Key"));

            var toggl = new TogglService(togglApiKey);

            Console.WriteLine("Toggl: Connected as {0}", toggl.GetUserInformation());
        }

        private static string AskFor(string what)
        {
            Console.Write("Please enter your {0}: ", what);
            return Console.ReadLine();
        }
    }
}