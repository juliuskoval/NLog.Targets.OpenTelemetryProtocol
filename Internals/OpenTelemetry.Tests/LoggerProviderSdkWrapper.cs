//// Copyright The OpenTelemetry Authors
//// SPDX-License-Identifier: Apache-2.0

namespace OpenTelemetry.Logs.Custom;

internal sealed class LoggerProviderSdkWrapper : LoggerProvider
{
    internal readonly LoggerProviderSdk loggerProviderSdk;

    // Only a provider the wrapper owns is disposed along with it. A shared provider
    // resolved from the application belongs to the application.
    private readonly bool ownsProvider;

    public LoggerProviderSdkWrapper(LoggerProvider lp, bool ownsProvider)
    {
        if (lp is LoggerProviderSdk provider)
            loggerProviderSdk = provider;
        else
            throw new Exception("LoggerProviderSdkWrapper can only be created from LoggerProviderSdk.");

        this.ownsProvider = ownsProvider;
    }

    internal bool IsDisposed => loggerProviderSdk.Disposed;

    public void AddProcessor(BaseProcessor<LogRecord> processor)
    {
        loggerProviderSdk.AddProcessor(processor);
    }

    public OpenTelemetry.Resources.Resource Resource => loggerProviderSdk.Resource;

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
        if (disposing && ownsProvider)
            loggerProviderSdk.Dispose();

        base.Dispose(disposing);
    }

    internal new LoggerSdkWrapper GetLogger(string? name)
    {
        return new LoggerSdkWrapper(this, name);
    }
}
