using NLog.Common;
using NLog.Targets.OpenTelemetryProtocol.Exceptions;
using NLog.Targets.OpenTelemetryProtocol;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using NLog.Config;
using NLog.Layouts;
using System.Globalization;
namespace NLog.Targets;

public abstract class OtlpTargetBase : TargetWithContext
{

    private OpenTelemetry.Logs.Logger _logger;

    private BatchLogRecordExportProcessor _processor;

    private const string OriginalFormatName = "{OriginalFormat}";

#if TEST
        public List<LogRecord> LogRecords;
#endif
    public bool IncludeEventParameters { get; set; }

    public bool IncludeFormattedMessage { get; set; }

    public Layout<bool> UseHttp { get; set; } = false;

    public Layout Endpoint { get; set; }

    public Layout Headers { get; set; }

    public Layout ServiceName { get; set; }

    public Layout<int> ScheduledDelayMilliseconds { get; set; } = 5000;

    public Layout<int> MaxQueueSize { get; set; } = 2048;

    public Layout<int> MaxExportBatchSize { get; set; } = 512;

    public Layout<bool> UseDefaultResources { get; set; } = true;


    [ArrayParameter(typeof(TargetPropertyWithContext), "resource")]
    public IList<TargetPropertyWithContext> Resources { get; } = new List<TargetPropertyWithContext>();

    [ArrayParameter(typeof(TargetPropertyWithContext), "attribute")]
    public IList<TargetPropertyWithContext> Attributes => ContextProperties;

    public OtlpTargetBase()
    {
        Layout = "${message}";
        IncludeEventProperties = true;
    }

    protected override void InitializeTarget()
    {
#if TEST
            LogRecords = new List<LogRecord>();
#endif
        var endpoint = RenderLogEvent(Endpoint, LogEventInfo.CreateNullEvent());
        var useHttp = RenderLogEvent(UseHttp, LogEventInfo.CreateNullEvent());
        var headers = RenderLogEvent(Headers, LogEventInfo.CreateNullEvent());
        var maxQueueSize = RenderLogEvent(MaxQueueSize, LogEventInfo.CreateNullEvent(), 2048);
        var maxExportBatchSize = RenderLogEvent(MaxExportBatchSize, LogEventInfo.CreateNullEvent(), 512);
        var scheduledDelayMilliseconds = RenderLogEvent(ScheduledDelayMilliseconds, LogEventInfo.CreateNullEvent(), 5000);

        _processor = CreateProcessor(endpoint, useHttp, headers, maxQueueSize, maxExportBatchSize, scheduledDelayMilliseconds);
        var resourceBuilder = CreateResourceBuilder();

        _logger = Sdk
            .CreateLoggerProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddProcessor(new LogRecordProcessor(IncludeFormattedMessage))
            .AddProcessor(_processor)
#if TEST
                .AddInMemoryExporter(LogRecords)
#endif
            .Build()
            .GetLogger();

        base.InitializeTarget();
    }

    protected override void CloseTarget()
    {
        var processor = _processor;
        _processor = null;
        var result = processor?.Shutdown(1000) ?? true;
        if (!result)
            InternalLogger.Info("OtlpTarget(Name={0}) - Shutdown OpenTelemetry BatchProcessor unsuccesful");
        base.CloseTarget();
    }

    protected override void FlushAsync(AsyncContinuation asyncContinuation)
    {
        Task.Run(() => _processor?.ForceFlush(15000)).ContinueWith(t => asyncContinuation(t.Exception));
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
        attributes.Add(OriginalFormatName, logEvent.Message);

        AppendAttributes(logEvent, ref attributes);
        _logger.EmitLog(data, attributes);
    }

    protected abstract void AppendAttributes(LogEventInfo logEvent, ref LogRecordAttributeList attributes);

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

    protected static string ResolveParameterKey(int index)
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
}