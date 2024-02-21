using NLog;
using NLog.Targets;
using OpenTelemetry.Exporter.OpenTelemetryProtocol.Implementation;

namespace UnitTests;

public class TargetTests
{
    private const string OriginalFormat = "{OriginalFormat}";

#if TEST
    [Fact]
    public void IncludeFormattedMessageWithProperties()
    {
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
    public void IncludeFormattedMessageWithoutProperties()
    {
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
    public void DontIncludeFormattedMessageWithProperties()
    {
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
        Assert.True(otlpLogRecord.Attributes.Count == 1);

        var index = 0;

        var attribute = otlpLogRecord.Attributes[index];
        Assert.Equal("field", attribute.Key);
        Assert.Equal(parameter, attribute.Value.StringValue);
    }

    [Fact]
    public void DontIncludeFormattedMessageWithoutProperties()
    {
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
        Assert.True(otlpLogRecord.Attributes.Count == 0);
    }

    [Fact]
    public void IncludeFormattedMessageWithParameters()
    {
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
    public void DontIncludeFormattedMessageWithParameters()
    {
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
        Assert.True(otlpLogRecord.Attributes.Count == 1);

        var index = 0;
        var attribute = otlpLogRecord.Attributes[index];
        Assert.Equal("0", attribute.Key);
        Assert.Equal(parameter, attribute.Value.StringValue);
    }

    [Fact]
    public void IncludeFormattedMessageWithPropertiesAndParameters()
    {
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

    //[Fact]
    //public void IncludeFormattedMessageWithParameters()
    //{
    //    var logger = LogManager.GetCurrentClassLogger();

    //    OtlpTarget target = (OtlpTarget)LogManager.Configuration.AllTargets.First(x => x is OtlpTarget);
    //    target.IncludeFormattedMessage = true;
    //    target.IncludeEventParameters = false;
    //    target.Dispose();
    //    LogManager.ReconfigExistingLoggers();

    //    var message = "message : {0}";
    //    var parameter = "testing";
    //    var expectedMessage = "message : testing";


    //    logger.Info(message, parameter);

    //    Assert.Single(target.LogRecords);


    //    var otlpLogRecordTransformer = new OtlpLogRecordTransformer(new(), new());

    //    var otlpLogRecord = otlpLogRecordTransformer.ToOtlpLog(target.LogRecords[0]);

    //    Assert.Equal(expectedMessage, otlpLogRecord.Body.StringValue);
    //    Assert.True(otlpLogRecord.Attributes.Count == 1);

    //    var index = 0;
    //    var attribute = otlpLogRecord.Attributes[index];
    //    Assert.Equal(OriginalFormat, attribute.Key);
    //    Assert.Equal(message, attribute.Value.StringValue);
    //}

    [Fact]
    public void DontIncludeFormattedMessageWithoutParameters()
    {
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
        Assert.True(otlpLogRecord.Attributes.Count == 0);
    }
#endif
}