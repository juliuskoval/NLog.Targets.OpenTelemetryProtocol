using System;
using System.Collections.Generic;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets.OpenTelemetryProtocol;
using NLog.Targets.OpenTelemetryProtocol.Exceptions;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace NLog.Targets
{
    [Target("OtlpTarget")]
    public class OtlpTarget : TargetWithLayout
    {
        private OpenTelemetry.Logs.Logger _logger;

        private BatchLogRecordExportProcessor _processor;

        private const string OriginalFormatName = "{OriginalFormat}";

        private List<KeyValuePair<string, Layout>> _attributes = new List<KeyValuePair<string, Layout>>();

        public bool UseHttp { get; set; } = false;

        public string Endpoint { get; set; }

        public string Headers { get; set; }

        public Layout Resources { get; set; }

        public Layout ServiceName { get; set; }

        public Layout Attributes { get; set; }

        public int ScheduledDelayMilliseconds { get; set; } = 5000;

        public bool UseDefaultResources { get; set; } = true;

        public bool IncludeFormattedMessage { get; set; } = false;

        protected override void InitializeTarget()
        {
            if (Attributes is not null)
                ParseAttributes();

            _processor = CreateProcessor();
            var resourceBuilder = CreateResourceBuilder();
            
            _logger = Sdk
                .CreateLoggerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddProcessor(new LogRecordProcessor(IncludeFormattedMessage))
                .AddProcessor(_processor)
                .Build()
                .GetLogger();
            
            base.InitializeTarget();
        }

        private void ParseAttributes()
        {
            try
            {
                var attributes = Attributes.ToString().Split(';');

                foreach (var attribute in attributes)
                {
                    var kvp = attribute.Split('=');
                    _attributes.Add(new KeyValuePair<string, Layout>(kvp[0], kvp[1]));
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "OtlpTarget(Name={0}) - An exception occured when parsing attributes.", Name);
                throw new FailedToParseAttributesException("Failed to parse attributes");
            }
        }

        private BatchLogRecordExportProcessor CreateProcessor()
        {
            try
            {
                var options = new OtlpExporterOptions();

                if (!String.IsNullOrEmpty(Headers))
                    options.Headers = Headers;

                if (UseHttp)
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;

                if (!String.IsNullOrEmpty(Endpoint))
                    options.Endpoint = new Uri(Endpoint);

                BaseExporter<LogRecord> otlpExporter = new OtlpLogExporter(options);
                return new BatchLogRecordExportProcessor(
                    otlpExporter,
                    ScheduledDelayMilliseconds = this.ScheduledDelayMilliseconds);
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "OtlpTarget(Name={0}) - An exception occured when creating an export processor.", Name);
                throw new FailedToCreateProcessorException("Failed to create an export processor");
            }
        }
        
        private ResourceBuilder CreateResourceBuilder()
        {
            var resourceBuilder = UseDefaultResources ? ResourceBuilder.CreateDefault() : ResourceBuilder.CreateEmpty();

            try
            {
                if (Resources is not null)
                {
                    var resources = RenderLogEvent(Resources, LogEventInfo.CreateNullEvent()).Split(';');
                    var attributes = new List<KeyValuePair<string, object>>();

                    foreach (var resource in resources)
                    {
                        var kvp = resource.Split('=');
                        attributes.Add(new KeyValuePair<string, object>(kvp[0], kvp[1]));
                    }

                    resourceBuilder.AddAttributes(attributes);
                }

                var serviceName = RenderLogEvent(ServiceName, LogEventInfo.CreateNullEvent());
                if (!String.IsNullOrEmpty(serviceName))
                    resourceBuilder.AddService(serviceName);
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "OtlpTarget(Name={0}) - An exception occured when parsing resources.", Name);
                throw new FailedToParseResourcesException("Failed to parse resources.");
            }

            return resourceBuilder;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var data = new LogRecordData
            {
                SeverityText = logEvent.Level.ToString(),
                Timestamp = logEvent.TimeStamp,
                Body = logEvent.FormattedMessage
            };

            var attributes = new LogRecordAttributeList { { OriginalFormatName, logEvent.Message } };

            if (logEvent.Parameters is not null && logEvent.Parameters.Length > 0)
                AppendParameters(logEvent, ref attributes);

            if (_attributes.Count > 0)
            {
                foreach (var kvp in _attributes)
                {
                    attributes.Add(kvp.Key, RenderLogEvent(kvp.Value, LogEventInfo.CreateNullEvent()));
                }
            }

            _logger.EmitLog(data, attributes);
        }

        private void AppendParameters(LogEventInfo logEvent, ref LogRecordAttributeList attributes)
        {
            if (logEvent.Properties.Count > 0) 
            {
                foreach (var kvp in logEvent.Properties)
                {
                    attributes.Add(kvp.Key.ToString(), kvp.Value);
                }
                return;
            }

            for (int i = 0; i < logEvent.Parameters.Length; i++)
            {
                attributes.Add($"{i}", logEvent.Parameters[i]);
            }
        }
    }
}