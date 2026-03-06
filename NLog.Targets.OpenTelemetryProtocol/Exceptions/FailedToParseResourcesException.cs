using System;

namespace NLog.Targets.OpenTelemetryProtocol.Exceptions
{
    internal class FailedToParseResourcesException : NLogConfigurationException
    {
        internal FailedToParseResourcesException(string message) : base(message) { }

        internal FailedToParseResourcesException(string message, Exception innerException) : base(message, innerException) { }
    }
}