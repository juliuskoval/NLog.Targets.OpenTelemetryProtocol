using System.Diagnostics.Tracing;
using NLog.Config;
using NLog.Layouts;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

// using OpenTelemetry.Proto.Collector.Logs.V1;
// using OpenTelemetry.Proto.Common.V1;
// using OpenTelemetry.Proto.Logs.V1;
// using OpenTelemetry.Proto.Resource.V1;

namespace NLog.Targets.OpenTelemetryProtocol
{
    [Target("OtlpTarget")]
    public class OtlpTarget : TargetWithLayout
    {
        private OpenTelemetry.Logs.Logger _logger;

        public bool UseGrpc { get; set; } = true;

        public string Endpoint { get; set; } = "http://localhost:4317";

        public Layout Resources { get; set; }

        protected override void InitializeTarget()
        {
            var resourceBuilder = CreateResourceBuilder();
            var processor = CreateProcessor();
            
            _logger = Sdk
                .CreateLoggerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddProcessor(new EnrichingProcessor())
                .AddProcessor(processor)
                .Build()
                .GetLogger();
            
            base.InitializeTarget();
        }

        private ResourceBuilder CreateResourceBuilder()
        {
            var resourceBuilder = ResourceBuilder.CreateEmpty();
            
            if (Resources is not null)
            {
                var resources = RenderLogEvent(Resources, LogEventInfo.CreateNullEvent()).Split(";");
                var attributes = new List<KeyValuePair<string, object>>();
                
                foreach (var resource in resources)
                {
                    var kvp = resource.Split("=");
                    attributes.Add(new KeyValuePair<string, object>(kvp[0], kvp[1]));
                }

                resourceBuilder.AddAttributes(attributes);
            }
            return resourceBuilder;
        }

        private BatchLogRecordExportProcessor CreateProcessor()
        {
            var options = new OtlpExporterOptions();
            options.Endpoint = new Uri(Endpoint);
            options.Protocol = UseGrpc ? OtlpExportProtocol.Grpc : OtlpExportProtocol.HttpProtobuf;
                
            var otlpLogExporterType = Type.GetType("OpenTelemetry.Exporter.OtlpLogExporter, OpenTelemetry.Exporter.OpenTelemetryProtocol, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7bd6737fe5b67e3c");
            var ctor = otlpLogExporterType.GetConstructor(new []{typeof(OtlpExporterOptions)});
            
            BaseExporter<LogRecord> otlpExporter = (BaseExporter<LogRecord>)ctor.Invoke(new[]{options});
            
            return new BatchLogRecordExportProcessor(
                otlpExporter);
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var data = new LogRecordData
            {
                Body = logEvent.Message,
                SeverityText = logEvent.Level.ToString(), //TODO
                Timestamp = logEvent.TimeStamp
            };
            
            _logger.EmitLog(data);
        }
    }
}