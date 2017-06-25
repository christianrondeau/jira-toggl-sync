using System;
using JiraTogglSync.Services;
using NUnit.Framework;

namespace JiraTogglSync.Tests
{
    public class TogglServiceTests
    {
        [Test]
        public void CanBeCreated()
        {
            new TogglRepository("my-api-key", descriptionTemplate: "");
        }
    }
}
