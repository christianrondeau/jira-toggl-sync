using Atlassian.Jira;

namespace ChristianRondeau.JiraTogglSync.Services
{
    public class JiraService
    {
        private readonly Jira _jira;

        public JiraService(string instance, string username, string password)
        {
            _jira = new Jira(instance, username, password);
        }

        public string GetUserInformation()
        {
            return _jira.Url;
        }
    }
}
