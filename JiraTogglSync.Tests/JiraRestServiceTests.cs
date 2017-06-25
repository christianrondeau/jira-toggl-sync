using System;
using JiraTogglSync.Services;
using NUnit.Framework;

namespace JiraTogglSync.Tests
{
    public class JiraRestServiceTests
    {
        [Test]
        public void CanBeCreated()
        {
            new JiraRestService("https://atlassian.net", "christianrondeau", "mypassword", "RetainRemainingEstimate");
        }
    }
}
