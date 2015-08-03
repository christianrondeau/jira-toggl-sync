using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JiraTogglSync.Services;

namespace JiraTogglSync.Tests
{
	[TestClass]
    public class JiraRestServiceTests
    {
        [TestMethod]
        public void CanBeCreated()
        {
            new JiraRestService("https://atlassian.net", "christianrondeau", "mypassword", "RetainRemainingEstimate");
        }
    }
}
