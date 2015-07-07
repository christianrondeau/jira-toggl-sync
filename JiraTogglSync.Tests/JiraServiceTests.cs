using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JiraTogglSync.Services;

namespace JiraTogglSync.Tests
{
	[TestClass]
    public class JiraServiceTests
    {
        [TestMethod]
        public void CanBeCreated()
        {
            new JiraService("https://atlassian.net", "christianrondeau", "mypassword", "RetainRemainingEstimate");
        }
    }
}
