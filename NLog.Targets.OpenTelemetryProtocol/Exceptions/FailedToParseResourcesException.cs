namespace NLog.Targets.OpenTelemetryProtocol.Exceptions
{
    internal class FailedToParseResourcesException : NLogConfigurationException
    {
        internal FailedToParseResourcesException(string message) : base(message) { }
    }
}