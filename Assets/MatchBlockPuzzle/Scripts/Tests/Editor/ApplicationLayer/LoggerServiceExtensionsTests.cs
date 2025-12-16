using System;
using System.Collections.Generic;
using MatchPuzzle.Core.Interfaces;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MatchPuzzle.Tests.Editor.ApplicationLayer
{
    public class LoggerServiceExtensionsTests
    {
        [Test]
        public void LoggerExtensions_CallUnderlyingLoggerWithLevel()
        {
            var logger = new FakeLogger();

            logger.LogDebug("debug msg");
            logger.LogInformation("info msg");
            logger.LogWarning("warn msg");
            logger.LogError("error msg");
            logger.LogCritical("crit msg");

            Assert.AreEqual(5, logger.Messages.Count);
            Assert.AreEqual(LogLevel.Debug, logger.Messages[0].level);
            Assert.AreEqual(LogLevel.Critical, logger.Messages[4].level);
        }

        [Test]
        public void LoggerExtensions_HandleExceptionLogging()
        {
            var logger = new FakeLogger();
            var ex = new InvalidOperationException("boom");

            logger.LogException(ex, "context");

            Assert.AreEqual(1, logger.Messages.Count);
            Assert.AreEqual(LogLevel.Error, logger.Messages[0].level);
            StringAssert.Contains("boom", logger.Messages[0].message);
        }

        [Test]
        public void LoggerExtensions_WithNullLogger_DoNotThrow()
        {
            ILoggerService logger = null;
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex("hello"));
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("warn"));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("error"));
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Exception: e"));

            Assert.DoesNotThrow(() => {
                logger.LogDebug("hello");
                logger.LogWarning("warn", context: null);
                logger.LogError("error", context: null);
                logger.LogException(new Exception("e"));
            });

            LogAssert.NoUnexpectedReceived();
        }

        private class FakeLogger : ILoggerService
        {
            public List<(LogLevel level, string message)> Messages { get; } = new List<(LogLevel, string)>();
            public LogLevel MinimumLevel { get; set; }
            public bool IsEnabled { get; private set; } = true;

            public void SetEnabled(bool enabled) => IsEnabled = enabled;

            public void Log(LogLevel level, string message, UnityEngine.Object context = null, Exception exception = null)
            {
                Messages.Add((level, message));
            }
        }
    }
}
