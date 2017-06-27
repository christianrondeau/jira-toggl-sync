using System;
using System.Collections.Generic;
using System.Linq;
using JiraTogglSync.Services;
using NUnit.Framework;

namespace JiraTogglSync.Tests
{
    public class TogglRepositoryTests
    {

        //         Description                  JIRA Project Keys        
        [TestCase("KEY-123 Doing some work",    new string [] {},        ExpectedResult = null ,      TestName = "Should not match if keys are not provided")]
        [TestCase("KEY-123 Doing some work",    new [] {"KEY"},          ExpectedResult = "KEY-123" , TestName = "Should match if one key matches exactly")]
        [TestCase("Doing some work KEY-123",    new [] {"LOCK", "KEY"},  ExpectedResult = "KEY-123" , TestName = "Should match if one of the keys matches")]
        [TestCase("[KEY-123] Doing some work",  new [] {"LOCK", "DOOR"}, ExpectedResult = null      , TestName = "Should not match of non of the keys match")]
        [TestCase("[KEY-123] [DOOR-345] work",  new [] {"KEY",  "DOOR"}, ExpectedResult = "KEY-123" , TestName = "Should match first key if more than key matches")]
        [TestCase("[MYKEY-123] some work",      new [] {"KEY"},          ExpectedResult = null ,      TestName = "Should not match partial keys")]
        public string ExtractIssueKeyTests(string description, string[] jiraProjectKeys)
        {
            var result = TogglRepository.ExtractIssueKey(description, jiraProjectKeys.ToList());
            return result;
        }

    }
}
