using System;

namespace NLog.Targets.OpenTelemetryProtocol.Exceptions
{
    internal class FailedToResolveResourceDetectorException : NLogConfigurationException
    {
        internal FailedToResolveResourceDetectorException(string message) : base(message) { }

        internal FailedToResolveResourceDetectorException(string message, Exception innerException) : base(message, innerException) { }
    }
}
