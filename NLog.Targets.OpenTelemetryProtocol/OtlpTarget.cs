using System;
using System.Collections.Generic;
using System.Globalization;
using NLog.Common;
using NLog.Config;
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
    public class OtlpTarget : TargetWithContext
    {
        private OpenTelemetry.Logs.Logger _logger;

        private BatchLogRecordExportProcessor _processor;

        private const string OriginalFormatName = "{OriginalFormat}";

        public bool IncludeEventParameters { get; set; }

        public Layout<bool> UseHttp { get; set; } = false;

        public Layout Endpoint { get; set; }

        public Layout Headers { get; set; }

        public Layout ServiceName { get; set; }

        public Layout<int> ScheduledDelayMilliseconds { get; set; } = 5000;

        public Layout<int> MaxQueueSize { get; set; } = 2048;

        public Layout<int> MaxExportBatchSize { get; set; } = 512;

        public Layout<bool> UseDefaultResources { get; set; } = true;

        public Layout<bool> IncludeFormattedMessage { get; set; } = false;

        [ArrayParameter(typeof(TargetPropertyWithContext), "resource")]
        public IList<TargetPropertyWithContext> Resources { get; } = new List<TargetPropertyWithContext>();

        [ArrayParameter(typeof(TargetPropertyWithContext), "attribute")]
        public IList<TargetPropertyWithContext> Attributes => ContextProperties;

        public OtlpTarget()
        {
            Layout = "${message}";
            IncludeEventProperties = true;
        }

        protected override void InitializeTarget()
        {
            var endpoint = RenderLogEvent(Endpoint, LogEventInfo.CreateNullEvent());
            var useHttp = RenderLogEvent(UseHttp, LogEventInfo.CreateNullEvent());
            var headers = RenderLogEvent(Headers, LogEventInfo.CreateNullEvent());
            var maxQueueSize = RenderLogEvent(MaxQueueSize, LogEventInfo.CreateNullEvent(), 2048);
            var maxExportBatchSize = RenderLogEvent(MaxExportBatchSize, LogEventInfo.CreateNullEvent(), 512);
            var scheduledDelayMilliseconds = RenderLogEvent(ScheduledDelayMilliseconds, LogEventInfo.CreateNullEvent(), 5000);
            var includeFormattedMessage = RenderLogEvent(IncludeFormattedMessage, LogEventInfo.CreateNullEvent());

            _processor = CreateProcessor(endpoint, useHttp, headers, maxQueueSize, maxExportBatchSize, scheduledDelayMilliseconds);
            var resourceBuilder = CreateResourceBuilder();
            
            _logger = Sdk
                .CreateLoggerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddProcessor(new LogRecordProcessor(includeFormattedMessage))
                .AddProcessor(_processor)
                .Build()
                .GetLogger();
            
            base.InitializeTarget();
        }

        private BatchLogRecordExportProcessor CreateProcessor(string endpoint, bool useHttp, string headers, int maxQueueSize, int maxExportBatchSize, int scheduledDelayMilliseconds)
        {
            try
            {
                var options = new OtlpExporterOptions();

                if (!String.IsNullOrEmpty(headers))
                    options.Headers = headers;

                if (useHttp)
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;

                if (!String.IsNullOrEmpty(endpoint))
                    options.Endpoint = new Uri(endpoint);

                BaseExporter<LogRecord> otlpExporter = new OtlpLogExporter(options);
                return new BatchLogRecordExportProcessor(
                    otlpExporter,
                    maxQueueSize: maxQueueSize,
                    maxExportBatchSize: maxExportBatchSize,
                    scheduledDelayMilliseconds: scheduledDelayMilliseconds);
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "OtlpTarget(Name={0}) - An exception occured when creating an export processor.", Name);
                throw new FailedToCreateProcessorException("Failed to create an export processor");
            }
        }
        
        private ResourceBuilder CreateResourceBuilder()
        {
            var defaultLogEvent = LogEventInfo.CreateNullEvent();

            var useDefaultResources = RenderLogEvent(UseDefaultResources, defaultLogEvent, true);
            var resourceBuilder = useDefaultResources ? ResourceBuilder.CreateDefault() : ResourceBuilder.CreateEmpty();

            try
            {
                var resources = new List<KeyValuePair<string, object>>();
                foreach (var resource in Resources)
                {
                    var resourceValue = resource.RenderValue(defaultLogEvent);
                    resources.Add(new KeyValuePair<string, object>(resource.Name, resourceValue));
                }

                if (resources.Count > 0)
                    resourceBuilder.AddAttributes(resources);

                var serviceName = RenderLogEvent(ServiceName, defaultLogEvent);
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
            var formattedMessage = RenderLogEvent(Layout, logEvent);

            var data = new LogRecordData()
            {
                SeverityText = logEvent.Level.ToString(),
                Severity = ResolveSeverity(logEvent.Level),
                Timestamp = logEvent.TimeStamp,
                Body = formattedMessage,
            };

            var attributes = new LogRecordAttributeList();
            AppendAttributes(logEvent, ref attributes);
            _logger.EmitLog(data, attributes);
        }

        private void AppendAttributes(LogEventInfo logEvent, ref LogRecordAttributeList attributes)
        {
            if (logEvent.Exception != null)
                attributes.RecordException(logEvent.Exception);

            attributes.Add(OriginalFormatName, logEvent.Message);

            if (ShouldIncludeProperties(logEvent))
            {
                var properties = GetAllProperties(logEvent);
                foreach (var property in properties)
                {
                    attributes.Add(property.Key, property.Value);
                }
            }
            else
            {
                if (IncludeEventParameters && logEvent.Parameters?.Length > 0)
                {
                    for (int i = 0; i < logEvent.Parameters.Length; i++)
                    {
                        attributes.Add(ResolveParameterKey(i), logEvent.Parameters[i]);
                    }
                }

                if (ContextProperties?.Count > 0)
                {
                    for (int i = 0; i< ContextProperties.Count; i++)
                    {
                        var property = ContextProperties[i];
                        var value = property.RenderValue(logEvent);
                        if (!property.IncludeEmptyValue && (value is null || string.Empty.Equals(value)))
                            continue;

                        attributes.Add(property.Name, value);
                    }
                }
            }
        }

        private static string ResolveParameterKey(int index)
        {
            switch (index)
            {
                case 0: return "0";
                case 1: return "1";
                case 2: return "2";
                case 3: return "3";
                case 4: return "4";
                case 5: return "5";
                case 6: return "6";
                case 7: return "7";
                case 8: return "8";
                case 9: return "9";
                default: return index.ToString(CultureInfo.InvariantCulture);
            }
        }

        private static LogRecordSeverity ResolveSeverity(LogLevel logLevel)
        {
            if (s_levelMap.TryGetValue(logLevel.Ordinal, out var severity))
                return severity;
            else
                return LogRecordSeverity.Info;
        }

        private static readonly Dictionary<int, LogRecordSeverity> s_levelMap = new Dictionary<int, LogRecordSeverity>
        {
            { LogLevel.Fatal.Ordinal, LogRecordSeverity.Fatal },
            { LogLevel.Error.Ordinal, LogRecordSeverity.Error },
            { LogLevel.Warn.Ordinal, LogRecordSeverity.Warn },
            { LogLevel.Info.Ordinal, LogRecordSeverity.Info },
            { LogLevel.Debug.Ordinal, LogRecordSeverity.Debug },
            { LogLevel.Trace.Ordinal, LogRecordSeverity.Trace },
        };
    }
}