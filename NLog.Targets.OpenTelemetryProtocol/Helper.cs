using OpenTelemetry.Proto.Common.V1;

namespace NLog.Targets.OpenTelemetryProtocol
{
    internal static class Helper
    {
        private const long NanosecondsPerTicks = 100;
        private const long UnixEpochTicks = 621355968000000000; // = DateTimeOffset.FromUnixTimeMilliseconds(0).Ticks

        internal static long ToUnixTimeNanoseconds(this DateTime dt)
        {
            return (dt.Ticks - UnixEpochTicks) * NanosecondsPerTicks;
        }
        
        internal static KeyValue CreateKeyValue(string key, string value)
        {
            return new KeyValue
            {
                Key = key,
                Value = new AnyValue
                {
                    StringValue = value
                }
            };
        }
        
        internal static KeyValue CreateKeyValue(string key, int value)
        {
            return new KeyValue
            {
                Key = key,
                Value = new AnyValue
                {
                    IntValue = value
                }
            };
        }
    }
}
