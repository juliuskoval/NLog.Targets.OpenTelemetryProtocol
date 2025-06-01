namespace OpenTelemetry.Logs.Custom;

internal static class LogRecordSeverityMapper
{
    internal static Logs.LogRecordSeverity Map(LogRecordSeverity? severity) => severity switch
    {
        LogRecordSeverity.Unspecified => Logs.LogRecordSeverity.Unspecified,
        LogRecordSeverity.Trace => Logs.LogRecordSeverity.Trace,
        LogRecordSeverity.Trace2 => Logs.LogRecordSeverity.Trace2,
        LogRecordSeverity.Trace3 => Logs.LogRecordSeverity.Trace3,
        LogRecordSeverity.Trace4 => Logs.LogRecordSeverity.Trace4,
        LogRecordSeverity.Debug => Logs.LogRecordSeverity.Debug,
        LogRecordSeverity.Debug2 => Logs.LogRecordSeverity.Debug2,
        LogRecordSeverity.Debug3 => Logs.LogRecordSeverity.Debug3,
        LogRecordSeverity.Debug4 => Logs.LogRecordSeverity.Debug4,
        LogRecordSeverity.Info => Logs.LogRecordSeverity.Info,
        LogRecordSeverity.Info2 => Logs.LogRecordSeverity.Info2,
        LogRecordSeverity.Info3 => Logs.LogRecordSeverity.Info3,
        LogRecordSeverity.Info4 => Logs.LogRecordSeverity.Info4,
        LogRecordSeverity.Warn => Logs.LogRecordSeverity.Warn,
        LogRecordSeverity.Warn2 => Logs.LogRecordSeverity.Warn2,
        LogRecordSeverity.Warn3 => Logs.LogRecordSeverity.Warn3,
        LogRecordSeverity.Warn4 => Logs.LogRecordSeverity.Warn4,
        LogRecordSeverity.Error => Logs.LogRecordSeverity.Error,
        LogRecordSeverity.Error2 => Logs.LogRecordSeverity.Error2,
        LogRecordSeverity.Error3 => Logs.LogRecordSeverity.Error3,
        LogRecordSeverity.Error4 => Logs.LogRecordSeverity.Error4,
        LogRecordSeverity.Fatal => Logs.LogRecordSeverity.Fatal,
        LogRecordSeverity.Fatal2 => Logs.LogRecordSeverity.Fatal2,
        LogRecordSeverity.Fatal3 => Logs.LogRecordSeverity.Fatal3,
        LogRecordSeverity.Fatal4 => Logs.LogRecordSeverity.Fatal4,
        _ => Logs.LogRecordSeverity.Unspecified
    };
}
