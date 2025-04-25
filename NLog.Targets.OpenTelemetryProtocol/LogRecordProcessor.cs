using OpenTelemetry;
using OpenTelemetry.Logs;

namespace NLog.Targets.OpenTelemetryProtocol
{
    internal class LogRecordProcessor : BaseProcessor<LogRecord>
    {
        public override void OnEnd(LogRecord data)
        {
            data.FormattedMessage = data.Body;
        }
    }
}