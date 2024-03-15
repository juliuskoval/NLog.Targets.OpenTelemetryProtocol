# Changelog

## 1.1.0
* The target will now have a private logger provider, which will get a logger with the name of the NLog logger whenever a log is written, 
so that the `scopeLogs` field in export requests will be populated by the name of the NLog logger. ([#11](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/pull/11))

* Upgraded OpenTelemetry packages to version 1.8.0-beta.1 ([#10](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/pull/10))

* Added a unit test ([#9](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/pull/9))

## 1.0.8
* Making sure that the value of the \{OriginalFormat\} attribute is never null ([#8](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/pull/8))

## 1.0.7
* Merged the targets ([#7](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/pull/7))

## 1.0.6
* Added a lightweight version of the target. ([#6](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/pull/6))
