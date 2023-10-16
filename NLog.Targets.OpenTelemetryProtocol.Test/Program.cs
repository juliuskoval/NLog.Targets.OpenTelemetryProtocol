using System.Threading;

namespace NLog.Targets.OpenTelemetryProtocol.Test
{
    public static class Program
    {
        public static void Main()
        {
            var logger = LogManager.GetCurrentClassLogger();

            var message = "testing";
            logger.Fatal("message: {0}", message);
    
            Thread.Sleep(10000);
            
        }
    }
}