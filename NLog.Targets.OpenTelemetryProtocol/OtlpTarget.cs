using Google.Protobuf.Collections;
using NLog.Config;
using OpenTelemetry.Proto.Collector.Logs.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Logs.V1;
using OpenTelemetry.Proto.Resource.V1;

namespace NLog.Targets.OpenTelemetryProtocol
{
    [Target("OtlpTarget")]
    public class OtlpTarget : TargetWithLayout
    {
        private LogExporter _exporter;
        private ExportLogsServiceRequest _requestTemplate;

        public bool UseGrpc { get; set; } = true; //TODO

        public string Endpoint { get; set; } = "http://localhost:4317";

        [RequiredParameter]
        public string ProcessName { get; set; }

        [RequiredParameter]
        public string Path { get; set; }

        [RequiredParameter]
        public string ThreadId { get; set; }

        [RequiredParameter]
        public string ProcessId { get; set; }


        protected override void InitializeTarget()
        {
            _exporter = UseGrpc ? new GrpcLogExporter(Endpoint) : new HttpLogExporter(Endpoint);
            BuildExportRequestTemplate();

            base.InitializeTarget();
        }

        private void BuildExportRequestTemplate()
        {
            _requestTemplate = new ExportLogsServiceRequest();
            var resourceLogs = BuildResourceLogs();
            var scopeLogs = new ScopeLogs();

            resourceLogs.ScopeLogs.Add(scopeLogs);
            _requestTemplate.ResourceLogs.Add(resourceLogs);
        }

        private ResourceLogs BuildResourceLogs()
        {
            var resource = new Resource();
            var keyValues = new List<KeyValue>();
            
            // keyValues.Add(Helper.CreateKeyValue("Hostname", Environment.MachineName));
            // keyValues.Add(Helper.CreateKeyValue("Path", Environment.MachineName));
            // keyValues.Add(Helper.CreateKeyValue("Process", Environment.MachineName));
            // keyValues.Add(Helper.CreateKeyValue("pid", ProcessId));
            // keyValues.Add(Helper.CreateKeyValue("tid", ThreadId));
            
            keyValues.Add(Helper.CreateKeyValue("telemetry.sdk.name", "opentelemetry"));
            keyValues.Add(Helper.CreateKeyValue("telemetry.sdk.language", "dotnet"));
            keyValues.Add(Helper.CreateKeyValue("telemetry.sdk.version", "1.6.1-alpha.0.54"));
            keyValues.Add(Helper.CreateKeyValue("service.name", "nlog"));
            
            resource.Attributes.Add(keyValues);

            return new ResourceLogs
            {
                Resource = resource
            };
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var exportRequest = BuildExportRequest(logEvent);

            _exporter.Export(exportRequest);
        }

        private ExportLogsServiceRequest BuildExportRequest(LogEventInfo logEvent)
        {
            var request = _requestTemplate.Clone();
            var record = new LogRecord
            {
                TimeUnixNano = (ulong)logEvent.TimeStamp.ToUnixTimeNanoseconds(),
                SeverityText = logEvent.Level.ToString(),
                Body = new AnyValue{StringValue = logEvent.Message},
                SeverityNumber = SeverityNumber.Fatal,
                ObservedTimeUnixNano = (ulong)logEvent.TimeStamp.ToUnixTimeNanoseconds()
            };

            request.ResourceLogs[0].ScopeLogs[0].LogRecords.Add(record);

            return request;
        }

        protected override void CloseTarget()
        {
             _exporter.Dispose();
             base.CloseTarget();
        }
    }
}