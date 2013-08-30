using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChristianRondeau.JiraTogglSync.Services;

namespace ChristianRondeau.JiraTogglSync.Tests
{
    [TestClass]
    public class TogglServiceTests
    {
        [TestMethod]
        public void CanBeCreated()
        {
            new TogglService("my-api-key");
        }
    }
}
