using Toggl.Services;

namespace ChristianRondeau.JiraTogglSync.Services
{
    public class TogglService
    {
        private string _apiKey;

        public TogglService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public string GetUserInformation()
        {
            var userService = new UserService(_apiKey);
            var currentUser = userService.GetCurrent();
            return string.Format("{0} ({1})", currentUser.FullName, currentUser.Email);
        }
    }
}
