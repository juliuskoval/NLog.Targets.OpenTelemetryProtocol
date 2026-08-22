using NLog;
using NLog.Targets;
using NLog.Targets.OpenTelemetryProtocol;
using OpenTelemetry.Exporter.OpenTelemetryProtocol.Implementation;
using OpenTelemetry.Exporter.OpenTelemetryProtocol.Implementation.Serializer;
using OpenTelemetry.Logs;
using OtlpLogs = OpenTelemetry.Proto.Logs.V1;
using OtlpResource = OpenTelemetry.Proto.Resource.V1;

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

    private static OtlpResource.Resource ToOtlpResource(OtlpTarget target)
    {
        // WriteLogsData serializes the full LogsData payload, which carries the resource
        // (unlike WriteLogRecord, which only emits a single LogRecord). An empty batch is
        // enough because we only care about the resource attributes here.
        var buffer = new byte[4096];
        var writePosition = ProtobufOtlpLogSerializer.WriteLogsData(ref buffer, 0, DefaultSdkLimitOptions, new ExperimentalOptions(), target.Resource, default);
        using var stream = new MemoryStream(buffer, 0, writePosition);
        var logsData = OtlpLogs.LogsData.Parser.ParseFrom(stream);
        return logsData.ResourceLogs.Single().Resource;
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

    #region Resources

    [Fact]
    public void CustomServiceNameIsAddedToResource()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.ServiceName = "MyService";
        });

        logger.Info("message");

        var resource = ToOtlpResource(target);

        Assert.Contains(resource.Attributes, a => a.Key == "service.name" && a.Value.StringValue == "MyService");
    }

    [Fact]
    public void CustomResourcesAreAdded()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.Resources.Add(new TargetPropertyWithContext("deployment.environment", "production"));
            t.Resources.Add(new TargetPropertyWithContext("service.version", "1.2.3"));
        });

        logger.Info("message");

        var resource = ToOtlpResource(target);

        Assert.Contains(resource.Attributes, a => a.Key == "deployment.environment" && a.Value.StringValue == "production");
        Assert.Contains(resource.Attributes, a => a.Key == "service.version" && a.Value.StringValue == "1.2.3");
    }

    [Fact]
    public void ResourceValueSupportsLayoutRendering()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.Resources.Add(new TargetPropertyWithContext("host.name", "${machinename}"));
        });

        logger.Info("message");

        var resource = ToOtlpResource(target);

        Assert.Contains(resource.Attributes, a => a.Key == "host.name" && a.Value.StringValue == Environment.MachineName);
    }

    [Fact]
    public void EmptyResourceValueIsStillAdded()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.Resources.Add(new TargetPropertyWithContext("custom.empty", ""));
        });

        logger.Info("message");

        var resource = ToOtlpResource(target);

        Assert.Contains(resource.Attributes, a => a.Key == "custom.empty" && a.Value.StringValue == "");
    }

    [Fact]
    public void DefaultResourcesAreNotIncludedWhenDisabled()
    {
        var (logger, target) = SetupTarget();

        logger.Info("message");

        var resource = ToOtlpResource(target);

        Assert.DoesNotContain(resource.Attributes, a => a.Key == "telemetry.sdk.name");
        Assert.DoesNotContain(resource.Attributes, a => a.Key == "telemetry.sdk.language");
    }

    [Fact]
    public void DefaultResourcesAreIncludedWhenEnabled()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.UseDefaultResources = true;
        });

        logger.Info("message");

        var resource = ToOtlpResource(target);

        Assert.Contains(resource.Attributes, a => a.Key == "telemetry.sdk.name" && a.Value.StringValue == "opentelemetry");
        Assert.Contains(resource.Attributes, a => a.Key == "telemetry.sdk.language" && a.Value.StringValue == "dotnet");
        Assert.Contains(resource.Attributes, a => a.Key == "service.instance.id");
        Assert.Contains(resource.Attributes, a => a.Key == "telemetry.sdk.version");
    }

    [Fact]
    public void CustomResourcesAreCombinedWithServiceName()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.ServiceName = "MyService";
            t.Resources.Add(new TargetPropertyWithContext("deployment.environment", "staging"));
        });

        logger.Info("message");

        var resource = ToOtlpResource(target);

        Assert.Contains(resource.Attributes, a => a.Key == "service.name" && a.Value.StringValue == "MyService");
        Assert.Contains(resource.Attributes, a => a.Key == "deployment.environment" && a.Value.StringValue == "staging");
    }

    #endregion

    #region ResourceDetectors

    private const string StubAssembly = "OpenTelemetry.Exporter.OpenTelemetryProtocol.Tests";
    private const string StubExtensions = "UnitTests.StubResourceBuilderExtensions";

    [Fact]
    public void ResourceDetectorIsResolvedFromAssemblyAndMethod()
    {
        // The real OpenTelemetry.Resources.Host package, whose HostDetector is an internal type
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.ResourceDetectors.Add(new ResourceDetector("OpenTelemetry.Resources.Host", "AddHostDetector"));
        });

        logger.Info("message");

        var resource = ToOtlpResource(target);

        Assert.Contains(resource.Attributes, a => a.Key == "host.name" && a.Value.StringValue == Environment.MachineName);
    }

    [Fact]
    public void ResourceDetectorsAreReadFromXmlConfiguration()
    {
        var (logger, target) = SetupTarget("nlog-resourcedetector.config");

        logger.Info("message");

        var resource = ToOtlpResource(target);

        Assert.Equal(3, target.ResourceDetectors.Count);
        Assert.Contains(resource.Attributes, a => a.Key == "host.name" && a.Value.StringValue == Environment.MachineName);
        Assert.Contains(resource.Attributes, a => a.Key == "stub.detector" && a.Value.StringValue == "detected");
        Assert.Contains(resource.Attributes, a => a.Key == "public.detector" && a.Value.StringValue == "detected");
        Assert.Contains(resource.Attributes, a => a.Key == "deployment.environment" && a.Value.StringValue == "DEV");
    }

    [Fact]
    public void ResourceDetectorIsResolvedFromExplicitType()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.ResourceDetectors.Add(new ResourceDetector
            {
                Assembly = "OpenTelemetry.Resources.Host",
                TypeName = "OpenTelemetry.Resources.HostResourceBuilderExtensions",
                Method = "AddHostDetector",
            });
        });

        logger.Info("message");

        var resource = ToOtlpResource(target);

        Assert.Contains(resource.Attributes, a => a.Key == "host.name" && a.Value.StringValue == Environment.MachineName);
    }

    [Fact]
    public void ResourceDetectorWithOptionalParameterIsInvoked()
    {
        // Mirrors AddAWSEC2Detector(builder, Action<AWSResourceBuilderOptions> configure = null)
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.ResourceDetectors.Add(new ResourceDetector(StubAssembly, "AddStubDetector"));
        });

        logger.Info("message");

        var resource = ToOtlpResource(target);

        Assert.Contains(resource.Attributes, a => a.Key == "stub.detector" && a.Value.StringValue == "detected");
    }

    [Fact]
    public void ResourceDetectorIsFoundAmongLoadedAssembliesWhenAssemblyIsOmitted()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.ResourceDetectors.Add(new ResourceDetector { Method = "AddSimpleStubDetector" });
        });

        logger.Info("message");

        var resource = ToOtlpResource(target);

        // NotAStaticClass declares the same method but is not a static class, so it must be skipped
        Assert.Contains(resource.Attributes, a => a.Key == "simple.stub.detector" && a.Value.StringValue == "detected");
    }

    [Fact]
    public void ResourceDetectorPrefersTheOverloadWithFewestParameters()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.ResourceDetectors.Add(new ResourceDetector(StubAssembly, "AddOverloadedStubDetector"));
        });

        logger.Info("message");

        var resource = ToOtlpResource(target);

        Assert.Contains(resource.Attributes, a => a.Key == "overload.parameters" && a.Value.StringValue == "0");
    }

    [Fact]
    public void ResourceDetectorTypeIsInstantiatedWhenMethodIsOmitted()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.ResourceDetectors.Add(new ResourceDetector { Assembly = StubAssembly, TypeName = "UnitTests.PublicStubDetector" });
        });

        logger.Info("message");

        var resource = ToOtlpResource(target);

        Assert.Contains(resource.Attributes, a => a.Key == "public.detector" && a.Value.StringValue == "detected");
    }

    [Fact]
    public void MultipleResourceDetectorsAreApplied()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.ResourceDetectors.Add(new ResourceDetector(StubAssembly, "AddStubDetector"));
            t.ResourceDetectors.Add(new ResourceDetector(StubAssembly, "AddSimpleStubDetector"));
            t.ResourceDetectors.Add(new ResourceDetector("OpenTelemetry.Resources.Host", "AddHostDetector"));
        });

        logger.Info("message");

        var resource = ToOtlpResource(target);

        Assert.Contains(resource.Attributes, a => a.Key == "stub.detector");
        Assert.Contains(resource.Attributes, a => a.Key == "simple.stub.detector");
        Assert.Contains(resource.Attributes, a => a.Key == "host.name");
    }

    [Fact]
    public void ConfiguredResourcesWinOverDetectedResources()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.Resources.Add(new TargetPropertyWithContext("host.name", "configured-host"));
            t.ResourceDetectors.Add(new ResourceDetector("OpenTelemetry.Resources.Host", "AddHostDetector"));
        });

        logger.Info("message");

        var resource = ToOtlpResource(target);

        Assert.Contains(resource.Attributes, a => a.Key == "host.name" && a.Value.StringValue == "configured-host");
        Assert.Single(resource.Attributes, a => a.Key == "host.name");
        // The detector still contributes the attributes that were not configured explicitly
        Assert.Contains(resource.Attributes, a => a.Key == "host.id");
    }

    [Fact]
    public void ResourceDetectorsAreCombinedWithDefaultResources()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.UseDefaultResources = true;
            t.ResourceDetectors.Add(new ResourceDetector(StubAssembly, "AddStubDetector"));
        });

        logger.Info("message");

        var resource = ToOtlpResource(target);

        Assert.Contains(resource.Attributes, a => a.Key == "telemetry.sdk.name" && a.Value.StringValue == "opentelemetry");
        Assert.Contains(resource.Attributes, a => a.Key == "stub.detector" && a.Value.StringValue == "detected");
    }

    #endregion

    #region ResourceDetectorErrors

    [Theory]
    // The assembly is not referenced by the application
    [InlineData("OpenTelemetry.Resources.NotInstalled", null, "AddNotInstalledDetector")]
    // The assembly is there, but the method is not
    [InlineData("OpenTelemetry.Resources.Host", null, "AddMisspelledDetector")]
    [InlineData("OpenTelemetry.Resources.Host", "OpenTelemetry.Resources.HostResourceBuilderExtensions", "AddMisspelledDetector")]
    // The type does not exist
    [InlineData("OpenTelemetry.Resources.Host", "OpenTelemetry.Resources.NoSuchType", "AddHostDetector")]
    // Not resolvable among the loaded assemblies either
    [InlineData(null, null, "AddMisspelledDetector")]
    // A required second parameter means there is no callable overload to find
    [InlineData(StubAssembly, null, "AddUncallableStubDetector")]
    // [Optional] without a default value leaves nothing to pass, so it is not callable either
    [InlineData(StubAssembly, null, "AddOptionalWithoutDefaultStubDetector")]
    // Neither Method nor TypeName was given
    [InlineData(null, null, null)]
    // TypeName is not an IResourceDetector and no Method was given
    [InlineData(StubAssembly, StubExtensions, null)]
    // The detector cannot be constructed
    [InlineData(StubAssembly, "UnitTests.NoParameterlessConstructorDetector", null)]
    // The registration method itself throws
    [InlineData(StubAssembly, null, "AddThrowingStubDetector")]
    // The detector registers fine, but throws while detecting - OpenTelemetry does not guard that itself
    [InlineData(StubAssembly, "UnitTests.ThrowingStubDetector", null)]
    [InlineData(StubAssembly, null, "AddDetectorThatThrowsWhenDetecting")]
    // The detector is found, but is not a detector at all
    [InlineData(StubAssembly, "UnitTests.NotAStaticClass", null)]
    public void ResourceDetectorThatCannotBeAddedIsSkipped(string? assembly, string? type, string? method)
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.ResourceDetectors.Add(new ResourceDetector { Assembly = assembly, TypeName = type, Method = method });
        });

        logger.Info("message");

        // The target still works, it just exports without the resources the detector would have added
        var otlpLogRecord = ToSingleOtlpLog(target);
        Assert.Equal("message", otlpLogRecord.Body.StringValue);
    }

    [Fact]
    public void ResourceDetectorThatIsNotFoundIsLoggedAsAWarning()
    {
        var output = CaptureInternalLog(t =>
            t.ResourceDetectors.Add(new ResourceDetector("OpenTelemetry.Resources.NotInstalled", "AddNotInstalledDetector")));

        Assert.Contains("Warn", output);
        Assert.Contains("OpenTelemetry.Resources.NotInstalled", output);
    }

    /// <summary>
    /// Captures NLog's InternalLogger output while the target is re-initialized with the given configuration.
    /// </summary>
    private string CaptureInternalLog(Action<OtlpTarget> configure)
    {
        var internalLog = new StringWriter();
        var previousLogWriter = NLog.Common.InternalLogger.LogWriter;
        var previousLogLevel = NLog.Common.InternalLogger.LogLevel;

        try
        {
            SetupTarget(configure: target =>
            {
                // Assigned from inside the configure callback, so that it is in place for the re-initialization
                // that SetupTarget performs afterwards - that is when the detectors are applied.
                NLog.Common.InternalLogger.LogWriter = internalLog;
                NLog.Common.InternalLogger.LogLevel = LogLevel.Warn;
                configure(target);
            });
        }
        finally
        {
            NLog.Common.InternalLogger.LogWriter = previousLogWriter;
            NLog.Common.InternalLogger.LogLevel = previousLogLevel;
        }

        return internalLog.ToString();
    }

    [Fact]
    public void ResourceDetectorsAreStillAppliedWhenAnotherOneIsNotFound()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.ResourceDetectors.Add(new ResourceDetector("OpenTelemetry.Resources.NotInstalled", "AddNotInstalledDetector"));
            t.ResourceDetectors.Add(new ResourceDetector(StubAssembly, "AddStubDetector"));
        });

        logger.Info("message");

        var resource = ToOtlpResource(target);

        Assert.Contains(resource.Attributes, a => a.Key == "stub.detector" && a.Value.StringValue == "detected");
    }

    [Fact]
    public void ResourceDetectorThatThrowsDuringRegistrationIsLoggedAsAWarning()
    {
        var output = CaptureInternalLog(t =>
            t.ResourceDetectors.Add(new ResourceDetector(StubAssembly, "AddThrowingStubDetector")));

        Assert.Contains("Warn", output);
        // The reason the registration failed has to survive into the warning
        Assert.Contains("registration failed", output);
    }

    [Fact]
    public void ResourceDetectorThatThrowsWhileDetectingIsLoggedAsAWarning()
    {
        var output = CaptureInternalLog(t =>
            t.ResourceDetectors.Add(new ResourceDetector { Assembly = StubAssembly, TypeName = "UnitTests.ThrowingStubDetector" }));

        Assert.Contains("Warn", output);
        Assert.Contains("detector failed", output);
    }

    [Fact]
    public void ResourceDetectorsAreStillAppliedWhenAnotherOneThrowsWhileDetecting()
    {
        var (logger, target) = SetupTarget(configure: t =>
        {
            t.ResourceDetectors.Add(new ResourceDetector { Assembly = StubAssembly, TypeName = "UnitTests.ThrowingStubDetector" });
            t.ResourceDetectors.Add(new ResourceDetector(StubAssembly, "AddStubDetector"));
        });

        logger.Info("message");

        var resource = ToOtlpResource(target);

        Assert.Contains(resource.Attributes, a => a.Key == "stub.detector" && a.Value.StringValue == "detected");
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