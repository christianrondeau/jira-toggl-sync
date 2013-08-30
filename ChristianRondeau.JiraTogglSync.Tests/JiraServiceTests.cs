using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChristianRondeau.JiraTogglSync.Services;

namespace ChristianRondeau.JiraTogglSync.Tests
{
    [TestClass]
    public class JiraServiceTests
    {
        [TestMethod]
        public void CanBeCreated()
        {
            new JiraService("https://christianrondeau.atlassian.net", "christianrondeau", "mypassword");
        }
    }
}
