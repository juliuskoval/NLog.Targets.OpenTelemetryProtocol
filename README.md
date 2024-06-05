# NLog Target for OpenTelemetry
[![Version](https://img.shields.io/nuget/v/NLog.Targets.OpenTelemetryProtocol.svg)](https://www.nuget.org/packages/NLog.Targets.OpenTelemetryProtocol ) 

This target can export logs in the format defined in the OpenTelemetry specification using the OpenTelemetry.Exporter.OpenTelemetryProtocol package.

For an explanation of the log data model, see https://opentelemetry.io/docs/specs/otel/logs/data-model/. <br>
For an example, see https://opentelemetry.io/docs/specs/otel/protocol/file-exporter/#examples.

**Note that the OpenTelemetry logging API is still unfinished, which means that it is internal in stable releases and public in prelease versions of the OpenTelemetry package.
This package has a reference to OpenTelemetry version 1.9.0-alpha.1. If your project has a reference to a stable version higher than that,
you will get a runtime error.**

## Configuration
Example XML config: 
```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    
    <extensions>
        <add assembly="NLog.Targets.OpenTelemetryProtocol"/>
    </extensions>

    <targets>
      <target xsi:type="OtlpTarget"
        name="otlp"
        usehttp="true"
        servicename="TestService"
        scheduledDelayMilliseconds="1000"
        useDefaultResources="false"
        includeFormattedMessage="true"
        onlyIncldueParameters="correlationId,messageId">
          <attribute name="thread.id" layout="${threadid}" />
          <resource name="process.name" layout="${processname}" />
          <resource name="process.id" layout="${processid}" />
          <resource name="deployment.environment" layout="DEV" />
      </target>
    </targets>
    <rules>
        <logger name="*" writeTo="otlp" />
    </rules>
</nlog>
```

### Configuration parameters

- **UseHttp** : Determines whether logs are exported using gRPC or HTTP. Optional, by default false, meaning the gRPC exporter will be used.
- **Endpoint** : Determines where the exporter should send logs. Used to set OtlpExporterOptions.Endpoint. 
If left empty, the default values will be used (optional)
- **Headers** : Used to set OtlpExporterOptions.Headers (optional)
- **ServiceName** : Used to set the service.name resource (optional)
- **Resource** : Additional resources to include in the ResourceBuilder (optional)
  - _Name_ : Name of Resource.
  - _Layout_ : Value of Resource (Will only resolve value at target initialize)
- **Attribute** : Attributes to be included with each LogEvent (optional)
  - _Name_ : Name of Attribute.
  - _Layout_ : Value of Attribute (If value is the same for all LogEvents, then add as resource instead)
- **MaxQueueSize** : The target uses a batch exporter, this defines the max queue size. By default 2048, optional.
- **MaxExportBatchSize** : The target uses a batch exporter, this defines the max batch size. By default 512, optional.
- **ScheduledDelayMilliseconds** : The target uses a batch exporter, this defines how often it is flushed in milliseconds. By default 5000, optional.
- **UseDefaultResources** : By default each exported batch will contain the resources telemetry.sdk.name, telemetry.sdk.language and telemetry.sdk.version. 
If you don't want them, set to false.
- **IncludeFormattedMessage** : If you're doing structured logging, this determines whether the body of the outputted log 
should be the formatted message or the message template. By default false, meaning the body of the outputted log will be the message template.
If you aren't doing structured logging, leave this as false.
- **IncludeEventProperties** : If you're doing structured logging, this determines whether to include NLog LogEvent properties as attributes. By default true, optional.
- **IncludeScopeProperties** : If you're doing structured logging, this determines whether to include NLog ScopeContext properties as attributes. By default false, optional.
- **IncludeEventParameters** : Whether to fallback to including NLog LogEvent parameters as attributes, when no NLog LogEvent properties. By default false, optional.
- **ResolveOptionsFromName** : Allows you to use the [Options pattern](https://learn.microsoft.com/en-my/dotnet/core/extensions/options) to initialize the target using 
 an instance of `OtlpExporterOptions` defined in appsettings.json. If the setting is not empty, NLog will try to resolve `OtlpExporterOptions` from the name given by this setting
 and `UseHttp`, `Endpoint` and `Headers` will be ignored. For an example, see the `TestWebApp` project.
- **ExcludeProperties** : A list of log event properties which won't be added to the final log as attributes. By default empty, meaning that no log event properties will be excluded.
- **OnlyIncludeProperties** : A list of log event properties which will be the only ones to be included in the final log. If both this and `ExcludeProperties` are defined,
 this setting will take precedence and `ExcludeProperties` will be ignored.



