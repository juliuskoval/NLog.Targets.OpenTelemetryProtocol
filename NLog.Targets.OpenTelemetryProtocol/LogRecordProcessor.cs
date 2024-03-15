using OpenTelemetry;
using OpenTelemetry.Logs;

namespace NLog.Targets.OpenTelemetryProtocol
{
    internal class LogRecordProcessor : BaseProcessor<LogRecord>
    {
        public LogRecordProcessor(bool includeFormattedMessage)
        {
            _includeFormattedMessage = includeFormattedMessage;
        }

        private readonly bool _includeFormattedMessage;   

        public override void OnEnd(LogRecord data)
        {
            if (_includeFormattedMessage)
                data.FormattedMessage = data.Body;
        }
    }
}