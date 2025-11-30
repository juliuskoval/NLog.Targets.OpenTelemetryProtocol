# Changelog

# 1.2.3

* Added the option to customize the layout for `SeverityText`. ([#41](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/pull/41))

# 1.2.2

* Bug fix - disposing the loggerProvider when closing the target ([commit](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/commit/c2af0185a5daecb953466e13a3c38dc2987f1e4a))

# 1.2.1

* Packing OpenTelemetry.Tests.dll together with the main package. ([commit](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/commit/1fdaaeef9c5cde3fc3e172882f2f1f62fd5610c7))

# 1.2.0

* **Breaking change**: The package now depends on stable versions of OpenTelemetry packages.
 This is made possible by a temporary workaround which means that the bin folder will contain OpenTelemetry.Tests.dll
 ([#36](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/pull/32))


# 1.1.10

* Avoiding unnecessary formatting ([#32](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/pull/32))
* Added the ability to rename the message template attribute ([commit](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/commit/d277eafcef507753f635ad124ec523b0442b34b1))
* Added a region in TargetTests.cs ([commit](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/commit/6002af5a8ed86a13d21cb99e10a4b85461112935))
* **Breaking change**:  Introduced ResolveLoggerProvider to use shared LoggerProvider dependency ([#31](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/pull/31))
* Faster check if SpanId / TraceId is empty ([#31](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/pull/31))

# 1.1.9

* Added netstandard2.1 to TargetFrameworks ([commit](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/commit/56308607102926d97fb84a9706e40b27c6873ebe))
* Avoid empty TraceId / SpanId ([commit](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/commit/0ac72895b2b9dc23b9ccba7dcf8550054ec34e0b))


# 1.1.8

* Fixed unit tests which were broken by the introduction of a custom protobuf serializer in the OTLP Exporter library.
([commit](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/commit/cc390a3343e81f69143b351a069c20ba21c2b062))
* Added a unit test to make sure that traceId and spanId are populated correctly.
([commit](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/commit/1bf62becff8abc81c880ed38e356a8c7d0e14d7b))
* Added a benchmark ([commit](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/commit/67c219726a9c1aa80471c4d361189d6c0520f00b))
* Fixed a bug which caused spanId and traceId to not be poopulated correctly when the target was wrapped in an async wrapper
and updated NLog to version 5.3.4. ([#27](https://github.com/juliuskoval/NLog.Targets.OpenTelemetryProtocol/pull/27))

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
