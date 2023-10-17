using System.Diagnostics.Tracing;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace NLog.Targets.OpenTelemetryProtocol;

public class EnrichingProcessor : BaseProcessor<LogRecord>
{
    public override void OnEnd(LogRecord data)
    {
        data.FormattedMessage = data.Body;
    }
}