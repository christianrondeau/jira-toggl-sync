using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JiraTogglSync.Services;

namespace JiraTogglSync.Tests
{
    [TestClass]
    public class TogglServiceTests
    {
        [TestMethod]
        public void CanBeCreated()
        {
            new TogglService("my-api-key", descriptionTemplate: "");
        }
    }
}
