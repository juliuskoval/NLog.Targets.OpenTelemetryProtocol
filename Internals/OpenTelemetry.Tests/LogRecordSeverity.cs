// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

namespace OpenTelemetry.Logs.Custom;

/// <summary>
/// Describes the severity level of a log record.
/// </summary>
internal enum LogRecordSeverity
{
    /// <summary>Unspecified severity (0).</summary>
    Unspecified = 0,

    /// <summary>Trace severity (1).</summary>
    Trace = 1,

    /// <summary>Trace1 severity (2).</summary>
    Trace2 = Trace + 1,

    /// <summary>Trace3 severity (3).</summary>
    Trace3 = Trace2 + 1,

    /// <summary>Trace4 severity (4).</summary>
    Trace4 = Trace3 + 1,

    /// <summary>Debug severity (5).</summary>
    Debug = 5,

    /// <summary>Debug2 severity (6).</summary>
    Debug2 = Debug + 1,

    /// <summary>Debug3 severity (7).</summary>
    Debug3 = Debug2 + 1,

    /// <summary>Debug4 severity (8).</summary>
    Debug4 = Debug3 + 1,

    /// <summary>Info severity (9).</summary>
    Info = 9,

    /// <summary>Info2 severity (11).</summary>
    Info2 = Info + 1,

    /// <summary>Info3 severity (12).</summary>
    Info3 = Info2 + 1,

    /// <summary>Info4 severity (13).</summary>
    Info4 = Info3 + 1,

    /// <summary>Warn severity (13).</summary>
    Warn = 13,

    /// <summary>Warn2 severity (14).</summary>
    Warn2 = Warn + 1,

    /// <summary>Warn3 severity (15).</summary>
    Warn3 = Warn2 + 1,

    /// <summary>Warn severity (16).</summary>
    Warn4 = Warn3 + 1,

    /// <summary>Error severity (17).</summary>
    Error = 17,

    /// <summary>Error2 severity (18).</summary>
    Error2 = Error + 1,

    /// <summary>Error3 severity (19).</summary>
    Error3 = Error2 + 1,

    /// <summary>Error4 severity (20).</summary>
    Error4 = Error3 + 1,

    /// <summary>Fatal severity (21).</summary>
    Fatal = 21,

    /// <summary>Fatal2 severity (22).</summary>
    Fatal2 = Fatal + 1,

    /// <summary>Fatal3 severity (23).</summary>
    Fatal3 = Fatal2 + 1,

    /// <summary>Fatal4 severity (24).</summary>
    Fatal4 = Fatal3 + 1,
}

internal static class SeverityMapper
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
