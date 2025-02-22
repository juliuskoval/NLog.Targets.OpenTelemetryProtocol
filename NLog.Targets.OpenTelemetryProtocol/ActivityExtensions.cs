using System.Diagnostics;

namespace NLog.Targets.OpenTelemetryProtocol
{
    /// <summary>
    /// Formats elements of <see cref="Activity.Current"/> for inclusion in log events. Non-W3C-format activities are
    /// ignored (Seq does not support the older Microsoft-proprietary hierarchical activity id format).
    /// </summary>
    internal static class ActivityExtensions
    {
        private static readonly System.Diagnostics.ActivitySpanId EmptySpanId = default(System.Diagnostics.ActivitySpanId);
        private static readonly System.Diagnostics.ActivityTraceId EmptyTraceId = default(System.Diagnostics.ActivityTraceId);

        public static string GetSpanId(this Activity activity)
        {
            return activity.IdFormat == ActivityIdFormat.W3C ?
                SpanIdToHexString(activity.SpanId) :
                string.Empty;
        }

        public static string GetTraceId(this Activity activity)
        {
            return activity.IdFormat == ActivityIdFormat.W3C ?
                TraceIdToHexString(activity.TraceId) :
                string.Empty;
        }

        private static string SpanIdToHexString(ActivitySpanId spanId)
        {
            if (EmptySpanId.Equals(spanId))
                return string.Empty;

            var spanHexString = spanId.ToHexString();
            if (ReferenceEquals(spanHexString, EmptySpanId.ToHexString()))
                return string.Empty;

            return spanHexString;
        }

        private static string TraceIdToHexString(ActivityTraceId traceId)
        {
            if (EmptyTraceId.Equals(traceId))
                return string.Empty;

            var traceHexString = traceId.ToHexString();
            if (ReferenceEquals(traceHexString, EmptyTraceId.ToHexString()))
                return string.Empty;

            return traceHexString;
        }
    }
}