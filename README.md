This target can export logs in the format defined in the OpenTelemetry specification using the OpenTelemetry.Exporter.OpenTelemetryProtocol package.

For an explanation of the log data model, see https://opentelemetry.io/docs/specs/otel/logs/data-model/. <br>
For an example, see https://opentelemetry.io/docs/specs/otel/protocol/file-exporter/#examples.

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
        resources="process.name=${processname};process.id=${processid};deployment.environment=DEV"
		attributes="thread.id=${thread.id}"
	    servicename="TestService"
		scheduledDelayMilliseconds="1000"
		useDefaultResource="false"
		includeFormattedMessage="true"/>
    </targets>
    <rules>
		
        <logger name="*" writeTo="otlp" />
    </rules>
</nlog>
```

### Configuration parameters

- **UseHttp** : Determines whether logs are exported using gRPC or HTTP. Optional, by default false
- **Endpoint** : Determines where the exporter should send logs. Used to set OtlpExporterOptions.Endpoint. 
If left empty, the default values will be used (optional)
- **Headers** : Used to set OtlpExporterOptions.Headers (optional)
- **Resources** : Used to set resources in the ResourceBuilder. It must be in the format like in the example above (optional)
- **ServiceName** : Used to set the service.name resource (optional)
- **Attributes** : Attributes which each log should contain. It must be in the same format as resources. Attributes are rendered 
everytime a log is written, so if an attribute would have the same value in all logs, set it in resources instead.
- **ScheduledDelayMilliseconds** : The target uses a batch exporter, this defines how often it is flushed in milliseconds. 
By default 5000, optional
- **UseDefaultResources** : By default each exported batch will contain the resources telemetry.sdk.name, telemetry.sdk.language and telemetry.sdk.version. 
If you don't want them, set to false.
- **IncludeFormattedMessage** : If you're doing structured logging, this determines whether the body of the log 
should be the formatted message or the message template. By default false, meaning the body will be the message template.

