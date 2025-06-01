#region Assembly OpenTelemetry, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7bd6737fe5b67e3c
// location unknown
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using OpenTelemetry.Internal;

namespace OpenTelemetry.Logs.Custom;

//
// Summary:
//     Contains extension methods for the OpenTelemetry.Logs.LoggerProviderBuilder class.
internal static class LoggerProviderBuilderExtensions
{
    public static LoggerProvider BuildCustom(this LoggerProviderBuilder loggerProviderBuilder)
    {
        if (loggerProviderBuilder is LoggerProviderBuilderBase loggerProviderBuilderBase)
        {
            return loggerProviderBuilderBase.Build();
        }

        throw new NotSupportedException("Build is not supported on '" + (loggerProviderBuilder?.GetType().FullName ?? "null") + "' instances.");
    }
}