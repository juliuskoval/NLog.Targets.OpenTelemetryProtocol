using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        private LoggerProvider _loggerProvider;

        private BatchLogRecordExportProcessor _processor;

        private OpenTelemetryEventListener _internalLoggerEventListener;

        private readonly ConcurrentDictionary<string, OpenTelemetry.Logs.Logger> _loggers = new(StringComparer.Ordinal);
        private readonly object _sync = new object();

        private const string OriginalFormatName = "{OriginalFormat}";

#if TEST
        public List<LogRecord> LogRecords;
#endif
        public bool IncludeEventParameters { get; set; }

        public bool IncludeFormattedMessage { get; set; }

        public Layout ResolveOptionsFromName { get; set; }

        public Layout<bool> UseHttp { get; set; } = false;

        public Layout Endpoint { get; set; }

        public Layout Headers { get; set; }

        public Layout ServiceName { get; set; }

        public Layout<int> ScheduledDelayMilliseconds { get; set; } = 5000;

        public Layout<int> MaxQueueSize { get; set; } = 2048;

        public Layout<int> MaxExportBatchSize { get; set; } = 512;

        public Layout<bool> UseDefaultResources { get; set; } = true;

        public HashSet<string> OnlyIncludeProperties { get; set; }

        [ArrayParameter(typeof(TargetPropertyWithContext), "resource")]
        public IList<TargetPropertyWithContext> Resources { get; } = new List<TargetPropertyWithContext>();

        [ArrayParameter(typeof(TargetPropertyWithContext), "attribute")]
        public IList<TargetPropertyWithContext> Attributes => ContextProperties;

        public bool DisableEventListener { get; set; }

        public OtlpTarget()
        {
            Layout = "${message}";
            IncludeEventProperties = true;
            OnlyIncludeProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        protected override void InitializeTarget()
        {
            var internalLoggerLevel = ResolveInternalLoggerLevel();
            if (internalLoggerLevel.HasValue && !DisableEventListener)
            {
                OpenTelemetryEventListener.EventLevel = internalLoggerLevel.Value;
                _internalLoggerEventListener = new OpenTelemetryEventListener(this);
            }

#if TEST
            LogRecords = new List<LogRecord>();
#endif

            var maxQueueSize = RenderLogEvent(MaxQueueSize, LogEventInfo.CreateNullEvent(), 2048);
            var maxExportBatchSize = RenderLogEvent(MaxExportBatchSize, LogEventInfo.CreateNullEvent(), 512);
            var scheduledDelayMilliseconds = RenderLogEvent(ScheduledDelayMilliseconds, LogEventInfo.CreateNullEvent(), 5000);

            var options = ResolveOtlpExporterOptionsFromName() ?? GetOtlpExporterOptions();

            _processor = CreateProcessor(options, maxQueueSize, maxExportBatchSize, scheduledDelayMilliseconds);
            var resourceBuilder = CreateResourceBuilder();
            
            _loggerProvider = Sdk
                .CreateLoggerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddProcessor(new LogRecordProcessor(IncludeFormattedMessage))
                .AddProcessor(_processor)
#if TEST
                .AddInMemoryExporter(LogRecords)
#endif
                .Build();

            base.InitializeTarget();
        }

        private static EventLevel? ResolveInternalLoggerLevel()
        {
            if (InternalLogger.IsDebugEnabled)
                return EventLevel.Verbose;
            else if (InternalLogger.IsInfoEnabled)
                return EventLevel.Informational;
            else if (InternalLogger.IsWarnEnabled)
                return EventLevel.Warning;
            else if (InternalLogger.IsErrorEnabled)
                return EventLevel.Error;
            else if (InternalLogger.IsFatalEnabled)
                return EventLevel.Critical;
            else
                return default(EventLevel?);
        }

        private OtlpExporterOptions ResolveOtlpExporterOptionsFromName()
        {
            var resolvedOptionsName = RenderLogEvent(ResolveOptionsFromName, LogEventInfo.CreateNullEvent());

            if (string.IsNullOrEmpty(resolvedOptionsName))
            {
                return null;
            }

            using IServiceScope scope = this.ResolveService<IServiceScopeFactory>().CreateScope();

            InternalLogger.Debug("{0} - Resolved OtlpExporterOptions from name '{1}'", this, resolvedOptionsName);

            var options = scope.ServiceProvider.GetService<IOptionsSnapshot<OtlpExporterOptions>>().Get(resolvedOptionsName);

            return options;
        }

        private OtlpExporterOptions GetOtlpExporterOptions()
        {
            var endpoint = RenderLogEvent(Endpoint, LogEventInfo.CreateNullEvent());
            var useHttp = RenderLogEvent(UseHttp, LogEventInfo.CreateNullEvent());
            var headers = RenderLogEvent(Headers, LogEventInfo.CreateNullEvent());

            var options = new OtlpExporterOptions();

            if (!String.IsNullOrEmpty(headers))
                options.Headers = headers;

            if (useHttp)
                options.Protocol = OtlpExportProtocol.HttpProtobuf;

            if (!String.IsNullOrEmpty(endpoint))
                options.Endpoint = new Uri(endpoint);

            return options;
        }

        private BatchLogRecordExportProcessor CreateProcessor(OtlpExporterOptions options, int maxQueueSize, int maxExportBatchSize, int scheduledDelayMilliseconds)
        {
            try
            {
                InternalLogger.Info("OtlpTarget(Name={0}) - Creating LogExporter with Protocol={1} to EndPoint={2}", Name, options.Protocol, options.Endpoint);
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
                    resources.Add(new KeyValuePair<string, object>(resource.Name, resourceValue ?? string.Empty));
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

        protected override void CloseTarget()
        {
            var logProvider = _loggerProvider;
            _loggerProvider = null;
            logProvider?.Dispose();
            _loggers.Clear();

            var processor = _processor;
            _processor = null;
            var result = processor?.Shutdown(1000) ?? true;
            if (!result)
                InternalLogger.Info("OtlpTarget(Name={0}) - Shutdown OpenTelemetry BatchProcessor unsuccessful", Name);

            _internalLoggerEventListener?.Dispose();

            base.CloseTarget();
        }

        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            Task.Run(() => _processor?.ForceFlush(15000)).ContinueWith(t => asyncContinuation(t.Exception));
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var data = new LogRecordData()
            {
                SeverityText = logEvent.Level.ToString(),
                Severity = ResolveSeverity(logEvent.Level),
                Timestamp = logEvent.TimeStamp,
            };

            if (IncludeFormattedMessage && (logEvent.Parameters?.Length > 0 || logEvent.HasProperties))
            {
                var formattedMessage = RenderLogEvent(Layout, logEvent);
                data.Body = formattedMessage;
            }

            var attributes = new LogRecordAttributeList();
            AppendAttributes(logEvent, ref attributes);
            GetLogger(logEvent.LoggerName).EmitLog(data, attributes);
        }

        private void AppendAttributes(LogEventInfo logEvent, ref LogRecordAttributeList attributes)
        {
            attributes.Add(OriginalFormatName, logEvent.Message ?? string.Empty);

            if (logEvent.Exception != null)
                attributes.RecordException(logEvent.Exception);

            if ((IncludeScopeProperties || IncludeGdc) && ShouldIncludeProperties(logEvent))
            {
                var properties = GetAllProperties(logEvent);
                foreach (var property in properties)
                {
                    attributes.Add(property.Key, property.Value);
                }
            }
            else
            {
                if (IncludeEventProperties && logEvent.HasProperties)
                {
                    foreach (var property in logEvent.Properties)
                    {
                        var key = property.Key.ToString();
                        if (OnlyIncludeProperties.Count > 0)
                        {
                            if (OnlyIncludeProperties.Contains(key))
                                attributes.Add(key, property.Value);
                            continue;
                        }

                        if (ExcludeProperties.Count > 0 && ExcludeProperties.Contains(key))
                            continue;

                        attributes.Add(key, property.Value);
                    }
                }
                else if (IncludeEventParameters && logEvent.Parameters?.Length > 0)
                {
                    for (int i = 0; i < logEvent.Parameters.Length; i++)
                    {
                        attributes.Add(ResolveParameterKey(i), logEvent.Parameters[i]);
                    }
                }

                if (ContextProperties?.Count > 0)
                {
                    for (int i = 0; i < ContextProperties.Count; i++)
                    {
                        var property = ContextProperties[i];
                        var value = RenderLogEvent(property.Layout, logEvent);
                        if (!property.IncludeEmptyValue && string.IsNullOrEmpty(value))
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

        private OpenTelemetry.Logs.Logger GetLogger(string name)
        {
            if (!_loggers.TryGetValue(name, out OpenTelemetry.Logs.Logger logger))
            {
                lock (_sync)
                {
                    if (!_loggers.TryGetValue(name, out logger))
                    {
                        logger = _loggerProvider.GetLogger(name);
                        _loggers[name] = logger;
                    }
                }
            }

            return logger;
        }
    }
}