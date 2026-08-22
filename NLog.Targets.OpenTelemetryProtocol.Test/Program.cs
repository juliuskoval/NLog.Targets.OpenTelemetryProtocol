using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
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

            logger.Fatal("message: {messageField}", new List<KeyValuePair<string, object?>>
            {
                new KeyValuePair<string, object?>("a", "c"),
                new ("b", 1),
                new KeyValuePair<string, object?>("c", true)
            });

            Thread.Sleep(10000);
        }
    }
}