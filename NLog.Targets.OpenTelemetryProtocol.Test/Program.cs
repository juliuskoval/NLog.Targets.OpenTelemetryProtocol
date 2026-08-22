using System;
using System.Threading;

namespace NLog.Targets.OpenTelemetryProtocol.Test
{
    public static class Program
    {
        public static void Main()
        {
            var logger = LogManager.GetCurrentClassLogger();

            var message = "testing";

            using var currentActivity = new System.Diagnostics.Activity("Hello World").Start();

            logger.Fatal("message: {messageField}", message);

            Thread.Sleep(10000);
        }
    }
}