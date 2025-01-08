# Changelog

## 1.1.7
* Updated dependencies. ([#22](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/pull/22))
* Added logging. ([#21](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/pull/21))

## 1.1.6
* The OpenTelemetry event source will log messages using NLog's internal logger. ([#19](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/pull/19))

## 1.1.5
* Upgraded OpenTelemetry packages to version 1.10.0-beta.1 ([commit](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/commit/03d9ca170a5c5e3b691497ed9458c7ea89004c91))

## 1.1.4
* Added a warning to README ([commit](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/commit/f2ed0e0721e9b6575d9754232e7554e285cc2a5c))
* Upgraded OpenTelemetry packages ([commit](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/commit/8bde762ae1077dc9e5e998366097a2ae6a8a9f3b))

## 1.1.3
* Added a new property `OnlyIncludeProperties` and added support for `ExcludeProperties` in all use cases. ([#15](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/pull/15))

## 1.1.2
* Added support for the [Options pattern](https://learn.microsoft.com/en-my/dotnet/core/extensions/options)
([#14](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/pull/14))


## 1.1.1
* When trying to add a resource with a null value, the value will instead default to an empty string and the target will be initialised 
([commit](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/commit/26edf215d44ada89886a55b7ef9c5defef596d18))

* Modifying nlog.config in the Test project ([commit](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/commit/c776ff519c08d8b43efd549936fa8af51e6282f8))

* Dispose LoggerProvider and clear Loggers when closing target ([#12](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/pull/12))

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
