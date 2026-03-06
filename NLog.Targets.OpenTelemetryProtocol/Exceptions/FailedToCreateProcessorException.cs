using System;

namespace NLog.Targets.OpenTelemetryProtocol.Exceptions
{
    internal class FailedToCreateProcessorException : NLogConfigurationException
    {
        internal FailedToCreateProcessorException(string message) : base(message) { }
        internal FailedToCreateProcessorException(string message, Exception innerException) : base(message, innerException){}
    }
}