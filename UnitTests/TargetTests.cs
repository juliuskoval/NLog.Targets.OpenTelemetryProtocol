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

    private (NLog.Logger logger, OtlpTarget target) SetupTarget(string configFile = "nlog.config", Action<OtlpTarget>? configure = null)
    {
        LogManager.Setup().LoadConfigurationFromFile(configFile, optional: false);
        var logger = LogManager.GetCurrentClassLogger();
        var target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);

        if (configure != null)
        {
            configure(target);
            target.Dispose();
            LogManager.ReconfigExistingLoggers();
        }
        else
        {
            LogManager.ReconfigExistingLoggers();
        }

        return (logger, target);
    }

    private static OtlpLogs.LogRecord ToSingleOtlpLog(OtlpTarget target)
    {
        Assert.Single(target.LogRecords);
        return ToOtlpLogs(DefaultSdkLimitOptions, new ExperimentalOptions(), target.LogRecords[0])!;
    }

    #region IncludeFormattedMessage

    [Fact]
    public void IncludeFormattedMessageWithProperties()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.IncludeFormattedMessage = true;
        });

        var message = "message : {field}";
        var parameter = "testing";
        var expectedMessage = "message : \"testing\"";

        logger.Info(message, parameter);

        var otlpLogRecord = ToSingleOtlpLog(target);

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
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.IncludeFormattedMessage = true;
            t.IncludeEventParameters = true;
        });

        var message = "message : {field}";
        var parameter = "testing";
        var expectedMessage = "message : \"testing\"";

        logger.Info(message, parameter);

        var otlpLogRecord = ToSingleOtlpLog(target);

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
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.IncludeFormattedMessage = true;
        });

        var message = "message without parameters";
        logger.Info(message);

        var otlpLogRecord = ToSingleOtlpLog(target);

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.True(otlpLogRecord.Attributes.Count() == 0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(CustomLayout)]
    public void IncludeFormattedMessageAndIncludeParameters(string? layout)
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.IncludeFormattedMessage = true;
            t.IncludeEventParameters = true;
            t.Layout = layout ?? t.Layout;
        });

        var message = "message : {0}";
        var parameter = "testing";
        var expectedMessage = "message : testing";

        logger.Info(message, parameter);

        var otlpLogRecord = ToSingleOtlpLog(target);

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
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.IncludeFormattedMessage = true;
        });

        var message = "message : {0}";
        var parameter = "testing";
        var expectedMessage = "message : testing";

        logger.Info(message, parameter);

        var otlpLogRecord = ToSingleOtlpLog(target);

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

        var (logger, target) = SetupTarget(configure: t =>
        {
            t.MessageTemplateAttribute = new NLog.Layouts.Layout<string>(templateString);
            t.IncludeFormattedMessage = true;
        });

        var message = "message : {field}";
        var parameter = "testing";
        var expectedMessage = "message : \"testing\"";

        logger.Info(message, parameter);

        var otlpLogRecord = ToSingleOtlpLog(target);

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
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.IncludeFormattedMessage = false;
            t.Layout = layout ?? t.Layout;
        });

        var message = "message : {field}";
        var parameter = "testing";

        logger.Info(message, parameter);

        var otlpLogRecord = ToSingleOtlpLog(target);

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
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.IncludeFormattedMessage = false;
        });

        var message = "message without parameters";

        logger.Info(message);

        var otlpLogRecord = ToSingleOtlpLog(target);

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.Empty(otlpLogRecord.Attributes);
    }

    [Fact]
    public void DontIncludeFormattedMessageWithParameters()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.IncludeFormattedMessage = false;
            t.IncludeEventParameters = true;
        });

        var message = "message : {0}";
        var parameter = "testing";

        logger.Info(message, parameter);

        var otlpLogRecord = ToSingleOtlpLog(target);

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
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.IncludeFormattedMessage = false;
            t.IncludeEventParameters = false;
        });

        var message = "message : {0}";
        var parameter = "testing";

        logger.Info(message, parameter);

        var otlpLogRecord = ToSingleOtlpLog(target);

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.Empty(otlpLogRecord.Attributes);
    }

    [Fact]
    public void DontIncludeFormattedMessageWithCustomMessageTemplateAttribute()
    {
        var templateString = "templateString";

        var (logger, target) = SetupTarget(configure: t =>
        {
            t.MessageTemplateAttribute = new NLog.Layouts.Layout<string>(templateString);
            t.IncludeFormattedMessage = false;
        });

        var message = "message : {field}";
        var parameter = "testing";

        logger.Info(message, parameter);

        var otlpLogRecord = ToSingleOtlpLog(target);

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
    public void ExcludeProperties()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.ExcludeProperties = new HashSet<string>() { "message", "someProperty" };
        });

        var message = "message : {message}, id: {id}";
        var property1 = "testing";
        var property2 = 123;

        logger.Info(message, property1, property2);

        var otlpLogRecord = ToSingleOtlpLog(target);

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
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.ExcludeProperties = new HashSet<string>() { "someProperty" };
        });

        var message = "message : {message}, id: {id}";
        var property1 = "testing";
        var property2 = 123;

        logger.Info(message, property1, property2);

        var otlpLogRecord = ToSingleOtlpLog(target);

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
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.ExcludeProperties = new HashSet<string>() { "id", "message", "someProperty" };
        });

        var message = "message : {message}, id: {id}";
        var property1 = "testing";
        var property2 = 123;

        logger.Info(message, property1, property2);

        var otlpLogRecord = ToSingleOtlpLog(target);

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.Empty(otlpLogRecord.Attributes);
    }

    [Fact]
    public void OnlyIncludeProperties()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.OnlyIncludeProperties = new HashSet<string>() { "id", "someProperty" };
        });

        var message = "message : {message}, id: {id}";
        var property1 = "testing";
        var property2 = 123;

        logger.Info(message, property1, property2);

        var otlpLogRecord = ToSingleOtlpLog(target);

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
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.OnlyIncludeProperties = new HashSet<string>() { "message", "id", "someProperty" };
        });

        var message = "message : {message}, id: {id}";
        var property1 = "testing";
        var property2 = 123;

        logger.Info(message, property1, property2);

        var otlpLogRecord = ToSingleOtlpLog(target);

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
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.OnlyIncludeProperties = new HashSet<string>() { "someProperty" };
        });

        var message = "message : {message}, id: {id}";
        var property1 = "testing";
        var property2 = 123;

        logger.Info(message, property1, property2);

        var otlpLogRecord = ToSingleOtlpLog(target);

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.Empty(otlpLogRecord.Attributes);
    }

    #endregion

    #region ActivityContext

    [Fact]
    public void ActivityContextIsPopulated()
    {
        var (logger, target) = SetupTarget();

        var message = "message";

        using var currentActivity = new System.Diagnostics.Activity("Hello World").Start();

        logger.Info(message);

        var otlpLogRecord = ToSingleOtlpLog(target);

        Assert.Equal(currentActivity.TraceId.ToString(), ByteStringToHexString(otlpLogRecord.TraceId));
        Assert.Equal(currentActivity.SpanId.ToString(), ByteStringToHexString(otlpLogRecord.SpanId));
    }

    [Fact]
    public void ActivityContextIsPopulatedIfAsync()
    {
        var (logger, target) = SetupTarget(configFile: "nlog2.config");

        var message = "message";

        using var currentActivity = new System.Diagnostics.Activity("Hello World").Start();

        logger.Info(message);
        LogManager.Flush();

        var otlpLogRecord = ToSingleOtlpLog(target);

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
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.SeverityText = layout;
        });

        var message = "message : {field}";
        var parameter = "testing";

        logger.Warn(message, parameter);

        var otlpLogRecord = ToSingleOtlpLog(target);

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