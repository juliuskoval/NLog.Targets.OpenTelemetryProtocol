namespace NLog.Targets.OpenTelemetryProtocol.Exceptions
{
    internal class FailedToCreateProcessorException : NLogConfigurationException
    {
        internal FailedToCreateProcessorException(string message) : base(message){}
    }
}