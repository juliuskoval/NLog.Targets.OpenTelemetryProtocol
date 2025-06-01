using OpenTelemetry.Logs;

namespace OpenTelemetry.Logs.Custom;

internal class LoggerSdkWrapper : Logger
{
    private readonly LoggerSdk _loggerSdk;

    public LoggerSdkWrapper(
           LoggerProviderSdkWrapper lp,
           string? name) 
           : base(name)
    {
        _loggerSdk = new LoggerSdk(lp.loggerProviderSdk, name);
    }

    public void EmitLog(in OpenTelemetry.Logs.Custom.LogRecordData data, in OpenTelemetry.Logs.Custom.LogRecordAttributeList attributes)
    {
        _loggerSdk.EmitLog(data.ToDefaultLogRecordData(), attributes.ToDefaultLogRecordAttributeList());

    }

    public override void EmitLog(in Logs.LogRecordData data, in Logs.LogRecordAttributeList attributes)
    {
        throw new NotImplementedException();
    }
}
