//// Copyright The OpenTelemetry Authors
//// SPDX-License-Identifier: Apache-2.0

namespace OpenTelemetry.Logs.Custom;

internal sealed class LoggerProviderSdkWrapper : LoggerProvider
{
    internal readonly LoggerProviderSdk loggerProviderSdk;

    public LoggerProviderSdkWrapper(
        IServiceProvider serviceProvider,
        bool ownsServiceProvider)
    {
        loggerProviderSdk = new LoggerProviderSdk(serviceProvider, ownsServiceProvider);
    }

    public LoggerProviderSdkWrapper(LoggerProvider lp)
    {
        if (lp is LoggerProviderSdk provider)
            loggerProviderSdk = provider;
        else
            throw new Exception("LoggerProviderSdkWrapper can only be created from LoggerProviderSdk.");
    }

    public void AddProcessor(BaseProcessor<LogRecord> processor)
    {
        loggerProviderSdk.AddProcessor(processor);
    }

    public bool ForceFlush(int timeoutMilliseconds = Timeout.Infinite)
    {
        return loggerProviderSdk.ForceFlush(timeoutMilliseconds);
    }

    public bool Shutdown(int timeoutMilliseconds)
    {
        return loggerProviderSdk.Shutdown(timeoutMilliseconds);
    }

    public bool ContainsBatchProcessor(BaseProcessor<LogRecord> processor)
    {
        return LoggerProviderSdk.ContainsBatchProcessor(processor);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    internal new LoggerSdkWrapper GetLogger(string? name)
    {
        return new LoggerSdkWrapper(this, name);
    }
}
