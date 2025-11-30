using NLog;
using NLog.Targets;
using OpenTelemetry.Exporter.OpenTelemetryProtocol.Implementation;
using OpenTelemetry.Exporter.OpenTelemetryProtocol.Implementation.Serializer;
using OpenTelemetry.Logs;
using OtlpLogs = OpenTelemetry.Proto.Logs.V1;

namespace UnitTests;

public class TargetTests
{
    private const string OriginalFormat = "{OriginalFormat}";
    private const string CustomLayout = "${logger}: ${message}";

    private static readonly SdkLimitOptions DefaultSdkLimitOptions = new();

#if TEST

    #region IncludeFormattedMessage

    [Fact]
    public void IncludeFormattedMessageWithProperties()
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.IncludeFormattedMessage = true;
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {field}";
        var parameter = "testing";
        var expectedMessage = "message : \"testing\"";


        logger.Info(message, parameter);

        Assert.Single(target.LogRecords);

        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        Assert.Equal(expectedMessage, otlpLogRecord.Body.StringValue);
        Assert.True(otlpLogRecord.Attributes.Count() == 2);

        var index = 0;
        var attribute = otlpLogRecord.Attributes[index];
        Assert.Equal(OriginalFormat, attribute.Key);
        Assert.Equal(message, attribute.Value.StringValue);

        attribute = otlpLogRecord.Attributes[++index];
        Assert.Equal("field", attribute.Key);
        Assert.Equal(parameter, attribute.Value.StringValue);
    }

    [Fact]
    public void IncludeFormattedMessageWithPropertiesAndParameters()
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.IncludeFormattedMessage = true;
        target.IncludeEventParameters = true;
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {field}";
        var parameter = "testing";
        var expectedMessage = "message : \"testing\"";


        logger.Info(message, parameter);

        Assert.Single(target.LogRecords);

        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        Assert.Equal(expectedMessage, otlpLogRecord.Body.StringValue);
        Assert.True(otlpLogRecord.Attributes.Count() == 2);

        var index = 0;
        var attribute = otlpLogRecord.Attributes[index];
        Assert.Equal(OriginalFormat, attribute.Key);
        Assert.Equal(message, attribute.Value.StringValue);

        attribute = otlpLogRecord.Attributes[++index];
        Assert.Equal("field", attribute.Key);
        Assert.Equal(parameter, attribute.Value.StringValue);
    }

    [Fact]
    public void IncludeFormattedMessageWithoutProperties()
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.IncludeFormattedMessage = true;
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message without parameters";
        logger.Info(message);

        Assert.Single(target.LogRecords);

        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.True(otlpLogRecord.Attributes.Count() == 0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(CustomLayout)]
    public void IncludeFormattedMessageAndIncludeParameters(string? layout)
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.IncludeFormattedMessage = true;
        target.IncludeEventParameters = true;
        target.Layout = layout ?? target.Layout;
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {0}";
        var parameter = "testing";
        var expectedMessage = "message : testing";

        logger.Info(message, parameter);

        Assert.Single(target.LogRecords);

        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        if (layout is null)
            Assert.Equal(expectedMessage, otlpLogRecord.Body.StringValue);
        else
            Assert.Equal($"{logger.Name}: {expectedMessage}", otlpLogRecord.Body.StringValue);

        Assert.True(otlpLogRecord.Attributes.Count() == 2);

        var index = 0;
        var attribute = otlpLogRecord.Attributes[index];
        Assert.Equal(OriginalFormat, attribute.Key);
        Assert.Equal(message, attribute.Value.StringValue);

        attribute = otlpLogRecord.Attributes[++index];
        Assert.Equal("0", attribute.Key);
        Assert.Equal(parameter, attribute.Value.StringValue);
    }

    [Fact]
    public void IncludeFormattedMessageAndDontIncludeParameters()
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.IncludeFormattedMessage = true;
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {0}";
        var parameter = "testing";
        var expectedMessage = "message : testing";


        logger.Info(message, parameter);

        Assert.Single(target.LogRecords);

        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        Assert.Equal(expectedMessage, otlpLogRecord.Body.StringValue);
        Assert.Single(otlpLogRecord.Attributes);

        var index = 0;
        var attribute = otlpLogRecord.Attributes[index];
        Assert.Equal(OriginalFormat, attribute.Key);
        Assert.Equal(message, attribute.Value.StringValue);
    }

    [Fact]
    public void IncludeFormattedMessageWithCustomMessageTemplateAttribute()
    {
        var templateString = "templateString";

        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.MessageTemplateAttribute = new NLog.Layouts.Layout<string>(templateString);
        target.IncludeFormattedMessage = true;
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {field}";
        var parameter = "testing";
        var expectedMessage = "message : \"testing\"";


        logger.Info(message, parameter);

        Assert.Single(target.LogRecords);

        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        Assert.Equal(expectedMessage, otlpLogRecord.Body.StringValue);
        Assert.True(otlpLogRecord.Attributes.Count() == 2);

        var index = 0;
        var attribute = otlpLogRecord.Attributes[index];
        Assert.Equal(templateString, attribute.Key);
        Assert.Equal(message, attribute.Value.StringValue);

        attribute = otlpLogRecord.Attributes[++index];
        Assert.Equal("field", attribute.Key);
        Assert.Equal(parameter, attribute.Value.StringValue);
    }
    #endregion

    #region DontIncludeFormattedMessage
    [Theory]
    [InlineData(null)]
    [InlineData(CustomLayout)]
    public void DontIncludeFormattedMessageWithProperties(string? layout)
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.IncludeFormattedMessage = false;
        target.Layout = layout ?? target.Layout;
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {field}";
        var parameter = "testing";


        logger.Info(message, parameter);

        Assert.Single(target.LogRecords);

        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.Single(otlpLogRecord.Attributes);

        var index = 0;

        var attribute = otlpLogRecord.Attributes[index];
        Assert.Equal("field", attribute.Key);
        Assert.Equal(parameter, attribute.Value.StringValue);
    }

    [Fact]
    public void DontIncludeFormattedMessageWithoutProperties()
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.IncludeFormattedMessage = false;
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message without parameters";


        logger.Info(message);

        Assert.Single(target.LogRecords);

        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.Empty(otlpLogRecord.Attributes);
    }

    [Fact]
    public void DontIncludeFormattedMessageWithParameters()
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.IncludeFormattedMessage = false;
        target.IncludeEventParameters = true;
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {0}";
        var parameter = "testing";


        logger.Info(message, parameter);

        Assert.Single(target.LogRecords);

        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.Single(otlpLogRecord.Attributes);

        var index = 0;
        var attribute = otlpLogRecord.Attributes[index];
        Assert.Equal("0", attribute.Key);
        Assert.Equal(parameter, attribute.Value.StringValue);
    }

    [Fact]
    public void DontIncludeFormattedMessageWithoutParameters()
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.IncludeFormattedMessage = false;
        target.IncludeEventParameters = false;
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {0}";
        var parameter = "testing";


        logger.Info(message, parameter);

        Assert.Single(target.LogRecords);

        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.Empty(otlpLogRecord.Attributes);
    }

    [Fact]
    public void DontIncludeFormattedMessageWithCustomMessageTemplateAttribute()
    {
        var templateString = "templateString";

        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.MessageTemplateAttribute = new NLog.Layouts.Layout<string>(templateString);
        target.IncludeFormattedMessage = false;
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {field}";
        var parameter = "testing";


        logger.Info(message, parameter);

        Assert.Single(target.LogRecords);

        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        Assert.Single(otlpLogRecord.Attributes);
        Assert.Equal(message, otlpLogRecord.Body.StringValue);

        var index = 0;
        var attribute = otlpLogRecord.Attributes[index];
        Assert.Equal("field", attribute.Key);
        Assert.Equal(parameter, attribute.Value.StringValue);
    }
    #endregion

    #region PropertyExclusion

    [Fact]
    public void ExludeProperties()
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.ExcludeProperties = new HashSet<string>() { "message", "someProperty" };
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {message}, id: {id}";
        var property1 = "testing";
        var property2 = 123;


        logger.Info(message, property1, property2);

        Assert.Single(target.LogRecords);

        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.Single(otlpLogRecord.Attributes);

        var index = 0;
        var attribute = otlpLogRecord.Attributes[index];
        Assert.Equal("id", attribute.Key);
        Assert.Equal(property2, attribute.Value.IntValue);
    }

    [Fact]
    public void ExcludeNonExistentProperties()
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.ExcludeProperties = new HashSet<string>() { "someProperty" };
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {message}, id: {id}";
        var property1 = "testing";
        var property2 = 123;


        logger.Info(message, property1, property2);

        Assert.Single(target.LogRecords);

        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.True(otlpLogRecord.Attributes.Count() == 2);

        var index = 0;
        var attribute = otlpLogRecord.Attributes[index];
        Assert.Equal("message", attribute.Key);
        Assert.Equal(property1, attribute.Value.StringValue);

        attribute = otlpLogRecord.Attributes[++index];
        Assert.Equal("id", attribute.Key);
        Assert.Equal(property2, attribute.Value.IntValue);
    }

    [Fact]
    public void ExcludeAllProperties()
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.ExcludeProperties = new HashSet<string>() { "id", "message", "someProperty" };
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {message}, id: {id}";
        var property1 = "testing";
        var property2 = 123;


        logger.Info(message, property1, property2);

        Assert.Single(target.LogRecords);

        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.Empty(otlpLogRecord.Attributes);
    }

    [Fact]
    public void OnlyIncludeProperties()
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.OnlyIncludeProperties = new HashSet<string>() { "id", "someProperty" };
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {message}, id: {id}";
        var property1 = "testing";
        var property2 = 123;


        logger.Info(message, property1, property2);

        Assert.Single(target.LogRecords);

        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.Single(otlpLogRecord.Attributes);

        var index = 0;
        var attribute = otlpLogRecord.Attributes[index];
        Assert.Equal("id", attribute.Key);
        Assert.Equal(property2, attribute.Value.IntValue);
    }

    [Fact]
    public void OnlyIncludeAllProperties()
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.OnlyIncludeProperties = new HashSet<string>() { "message", "id", "someProperty" };
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {message}, id: {id}";
        var property1 = "testing";
        var property2 = 123;


        logger.Info(message, property1, property2);

        Assert.Single(target.LogRecords);


        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.True(otlpLogRecord.Attributes.Count() == 2);

        var index = 0;
        var attribute = otlpLogRecord.Attributes[index];
        Assert.Equal("message", attribute.Key);
        Assert.Equal(property1, attribute.Value.StringValue);

        attribute = otlpLogRecord.Attributes[++index];
        Assert.Equal("id", attribute.Key);
        Assert.Equal(property2, attribute.Value.IntValue);
    }

    [Fact]
    public void OnlyIncludeNonExistentProperties()
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.OnlyIncludeProperties = new HashSet<string>() { "someProperty" };
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {message}, id: {id}";
        var property1 = "testing";
        var property2 = 123;


        logger.Info(message, property1, property2);

        Assert.Single(target.LogRecords);

        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.Empty(otlpLogRecord.Attributes);
    }

    #endregion

    #region ActivityContext

    [Fact]
    public void ActivityContextIsPopulated()
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        LogManager.ReconfigExistingLoggers();

        var message = "message";

        using var currentActivity = new System.Diagnostics.Activity("Hello World").Start();

        logger.Info(message);

        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        Assert.Equal(currentActivity.TraceId.ToString(), ByteStringToHexString(otlpLogRecord.TraceId));
        Assert.Equal(currentActivity.SpanId.ToString(), ByteStringToHexString(otlpLogRecord.SpanId));
    }

    [Fact]
    public void ActivityContextIsPopulatedIfAsync()
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog2.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        LogManager.ReconfigExistingLoggers();

        var message = "message";

        using var currentActivity = new System.Diagnostics.Activity("Hello World").Start();

        logger.Info(message);
        LogManager.Flush();

        Assert.Single(target.LogRecords);
        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;

        Assert.Equal(currentActivity.TraceId.ToString(), ByteStringToHexString(otlpLogRecord.TraceId));
        Assert.Equal(currentActivity.SpanId.ToString(), ByteStringToHexString(otlpLogRecord.SpanId));
    }

    #endregion

    #region SeverityText
    [Theory]
    [InlineData(null)]
    [InlineData("${level}")]
    [InlineData("${level:uppercase=true}")]
    [InlineData("${level:format=FullName}")]
    public void CustomizeSeverityText(string? layout)
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: false);
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.SeverityText = layout;
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {field}";
        var parameter = "testing";

        logger.Warn(message, parameter);

        Assert.Single(target.LogRecords);

        OtlpLogs.LogRecord otlpLogRecord = ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;
        switch (layout)
        {
            case "${level:uppercase=true}":
                Assert.Equal("WARN", otlpLogRecord.SeverityText);
                break;
            case "${level:format=FullName}":
                Assert.Equal("Warning", otlpLogRecord.SeverityText);
                break;
            case null:
            default:
                Assert.Equal("Warn", otlpLogRecord.SeverityText);
                break;
        }
    }
    #endregion

    private string ByteStringToHexString(Google.Protobuf.ByteString str)
    {
        return BitConverter.ToString(str.ToByteArray()).Replace("-", "").ToLower();
    }

    private static OtlpLogs.LogRecord? ToOtlpLogs(SdkLimitOptions sdkOptions, ExperimentalOptions experimentalOptions, LogRecord logRecord)
    {
        var buffer = new byte[4096];
        var writePosition = ProtobufOtlpLogSerializer.WriteLogRecord(buffer, 0, sdkOptions, experimentalOptions, logRecord);
        using var stream = new MemoryStream(buffer, 0, writePosition);
        var scopeLogs = OtlpLogs.ScopeLogs.Parser.ParseFrom(stream);
        return scopeLogs.LogRecords.FirstOrDefault();
    }

#endif
}