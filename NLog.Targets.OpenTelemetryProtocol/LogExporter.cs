using NLog.Common;
using OpenTelemetry;
using OpenTelemetry.Proto.Collector.Logs.V1;

namespace NLog.Targets.OpenTelemetryProtocol
{
    internal abstract class LogExporter : IDisposable
    {
        internal void Export(ExportLogsServiceRequest request)
        {
            // Prevents the exporter's gRPC and HTTP operations from being instrumented.
            using var scope = SuppressInstrumentationScope.Begin();

            try
            {
                SendExportRequest(request);
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "An exception occured when exporting OpenTelemetry logs.");
            }
        }

        protected abstract void SendExportRequest(ExportLogsServiceRequest request);

        public abstract void Dispose();
    }
}
