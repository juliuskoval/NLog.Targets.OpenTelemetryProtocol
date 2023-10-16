namespace NLog.Targets.OpenTelemetryProtocol.Exceptions
{
    internal class FailedToParseAttributesException : NLogConfigurationException
    {
        internal FailedToParseAttributesException(string message) : base(message) { }
    }
}