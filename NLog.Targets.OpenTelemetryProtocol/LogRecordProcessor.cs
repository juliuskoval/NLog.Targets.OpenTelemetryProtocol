using System;
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
            data.CategoryName = String.Empty; //Otlp exporter throws an exception if CategoryName is null, so this is a (hopefully) temporary workaround.

            if (_includeFormattedMessage)
                data.FormattedMessage = data.Body;

        }
    }
}