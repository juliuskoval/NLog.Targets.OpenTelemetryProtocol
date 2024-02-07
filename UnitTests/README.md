The logic of creating export requests in the OtlpLogExporter is pretty complicated, 
so I included some unit tests to make sure that changes to the target don't break it.

The project has to be called OpenTelemetry.Exporter.OpenTelemetryProtocol.Tests so that the internals of 
OpenTelemetry.Exporter.OpenTelemetryProtocol are visible to it. 

If you want to run the tests, select the Test build configuration.