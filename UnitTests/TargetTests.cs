using NLog;
using NLog.Targets;
using OpenTelemetry.Exporter.OpenTelemetryProtocol.Implementation;

namespace UnitTests;

public class TargetTests
{
    private const string OriginalFormat = "{OriginalFormat}";

#if TEST

    #region IncludeFormattedMessage

    [Fact]
    public void IncludeFormattedMessageWithProperties()
    {
        LogManager.LoadConfiguration("nlog.config");
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


        var otlpLogRecordTransformer = new OtlpLogRecordTransformer(new (), new());

        var otlpLogRecord = otlpLogRecordTransformer.ToOtlpLog(target.LogRecords[0]);

        Assert.Equal(expectedMessage, otlpLogRecord.Body.StringValue);
        Assert.True(otlpLogRecord.Attributes.Count == 2);

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
        LogManager.LoadConfiguration("nlog.config");
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


        var otlpLogRecordTransformer = new OtlpLogRecordTransformer(new(), new());

        var otlpLogRecord = otlpLogRecordTransformer.ToOtlpLog(target.LogRecords[0]);

        Assert.Equal(expectedMessage, otlpLogRecord.Body.StringValue);
        Assert.True(otlpLogRecord.Attributes.Count == 2);

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
        LogManager.LoadConfiguration("nlog.config");
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.IncludeFormattedMessage = true;
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message without parameters";


        logger.Info(message);

        Assert.Single(target.LogRecords);


        var otlpLogRecordTransformer = new OtlpLogRecordTransformer(new(), new());

        var otlpLogRecord = otlpLogRecordTransformer.ToOtlpLog(target.LogRecords[0]);

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.True(otlpLogRecord.Attributes.Count == 0);
    }

    [Fact]
    public void IncludeFormattedMessageAndIncludeParameters()
    {
        LogManager.LoadConfiguration("nlog.config");
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.IncludeFormattedMessage = true;
        target.IncludeEventParameters = true;
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {0}";
        var parameter = "testing";
        var expectedMessage = "message : testing";


        logger.Info(message, parameter);

        Assert.Single(target.LogRecords);


        var otlpLogRecordTransformer = new OtlpLogRecordTransformer(new(), new());

        var otlpLogRecord = otlpLogRecordTransformer.ToOtlpLog(target.LogRecords[0]);

        Assert.Equal(expectedMessage, otlpLogRecord.Body.StringValue);
        Assert.True(otlpLogRecord.Attributes.Count == 2);

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
        LogManager.LoadConfiguration("nlog.config");
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.IncludeFormattedMessage = true;
        target.IncludeEventParameters = false;
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {0}";
        var parameter = "testing";
        var expectedMessage = "message : testing";


        logger.Info(message, parameter);

        Assert.Single(target.LogRecords);


        var otlpLogRecordTransformer = new OtlpLogRecordTransformer(new(), new());

        var otlpLogRecord = otlpLogRecordTransformer.ToOtlpLog(target.LogRecords[0]);

        Assert.Equal(expectedMessage, otlpLogRecord.Body.StringValue);
        Assert.Single(otlpLogRecord.Attributes);

        var index = 0;
        var attribute = otlpLogRecord.Attributes[index];
        Assert.Equal(OriginalFormat, attribute.Key);
        Assert.Equal(message, attribute.Value.StringValue);
    }
    #endregion

    #region DontIncldueFormattedMessage
    [Fact]
    public void DontIncludeFormattedMessageWithProperties()
    {
        LogManager.LoadConfiguration("nlog.config");
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.IncludeFormattedMessage = false;
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {field}";
        var parameter = "testing";


        logger.Info(message, parameter);

        Assert.Single(target.LogRecords);


        var otlpLogRecordTransformer = new OtlpLogRecordTransformer(new(), new());

        var otlpLogRecord = otlpLogRecordTransformer.ToOtlpLog(target.LogRecords[0]);

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
        LogManager.LoadConfiguration("nlog.config");
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.IncludeFormattedMessage = false;
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message without parameters";


        logger.Info(message);

        Assert.Single(target.LogRecords);


        var otlpLogRecordTransformer = new OtlpLogRecordTransformer(new(), new());

        var otlpLogRecord = otlpLogRecordTransformer.ToOtlpLog(target.LogRecords[0]);

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.Empty(otlpLogRecord.Attributes);
    }

    [Fact]
    public void DontIncludeFormattedMessageWithParameters()
    {
        LogManager.LoadConfiguration("nlog.config");
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


        var otlpLogRecordTransformer = new OtlpLogRecordTransformer(new(), new());

        var otlpLogRecord = otlpLogRecordTransformer.ToOtlpLog(target.LogRecords[0]);

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
        LogManager.LoadConfiguration("nlog.config");
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


        var otlpLogRecordTransformer = new OtlpLogRecordTransformer(new(), new());

        var otlpLogRecord = otlpLogRecordTransformer.ToOtlpLog(target.LogRecords[0]);

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.Empty(otlpLogRecord.Attributes);
    }
    #endregion

    [Fact]
    public void ExludeProperties()
    {
        LogManager.LoadConfiguration("nlog.config");
        var logger = LogManager.GetCurrentClassLogger();

        OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
        target.ExcludeProperties = new HashSet<string>() { "message", "someProperty"};
        target.Dispose();
        LogManager.ReconfigExistingLoggers();

        var message = "message : {message}, id: {id}";
        var property1 = "testing";
        var property2 = 123;


        logger.Info(message, property1, property2);

        Assert.Single(target.LogRecords);


        var otlpLogRecordTransformer = new OtlpLogRecordTransformer(new(), new());

        var otlpLogRecord = otlpLogRecordTransformer.ToOtlpLog(target.LogRecords[0]);

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
        LogManager.LoadConfiguration("nlog.config");
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


        var otlpLogRecordTransformer = new OtlpLogRecordTransformer(new(), new());

        var otlpLogRecord = otlpLogRecordTransformer.ToOtlpLog(target.LogRecords[0]);

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.True(otlpLogRecord.Attributes.Count == 2);

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
        LogManager.LoadConfiguration("nlog.config");
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


        var otlpLogRecordTransformer = new OtlpLogRecordTransformer(new(), new());

        var otlpLogRecord = otlpLogRecordTransformer.ToOtlpLog(target.LogRecords[0]);

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.Empty(otlpLogRecord.Attributes);
    }

    [Fact]
    public void OnlyIncludeProperties()
    {
        LogManager.LoadConfiguration("nlog.config");
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


        var otlpLogRecordTransformer = new OtlpLogRecordTransformer(new(), new());

        var otlpLogRecord = otlpLogRecordTransformer.ToOtlpLog(target.LogRecords[0]);

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
        LogManager.LoadConfiguration("nlog.config");
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


        var otlpLogRecordTransformer = new OtlpLogRecordTransformer(new(), new());

        var otlpLogRecord = otlpLogRecordTransformer.ToOtlpLog(target.LogRecords[0]);

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.True(otlpLogRecord.Attributes.Count == 2);

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
        LogManager.LoadConfiguration("nlog.config");
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


        var otlpLogRecordTransformer = new OtlpLogRecordTransformer(new(), new());

        var otlpLogRecord = otlpLogRecordTransformer.ToOtlpLog(target.LogRecords[0]);

        Assert.Equal(message, otlpLogRecord.Body.StringValue);
        Assert.Empty(otlpLogRecord.Attributes);
    }

#endif
}