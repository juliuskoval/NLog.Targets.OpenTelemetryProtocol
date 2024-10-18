using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace NLog.Targets.OpenTelemetryProtocol
{
    /// <summary>
    /// Redirects output from OpenTelemetry EventSource to NLog InternalLogger
    /// </summary>
    internal class OpenTelemetryEventListener : EventListener
    {
        private const string EventSourceNamePrefix = "OpenTelemetry-";
        private readonly OtlpTarget _ownerTarget;

        internal static EventLevel EventLevel { get; set; } = EventLevel.LogAlways;

        public OpenTelemetryEventListener(OtlpTarget ownerTarget)
        {
            if (EventLevel == EventLevel.LogAlways)
                throw new InvalidOperationException("Must initialize EventLevel before calling constructor");

            _ownerTarget = ownerTarget;
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData.EventSource.Name.StartsWith(EventSourceNamePrefix, StringComparison.OrdinalIgnoreCase))
            {
                WriteEvent(eventData.Level, eventData.Message, eventData.Payload);
            }
        }

        private void WriteEvent(EventLevel eventLevel, string? eventMessage, IReadOnlyList<object> payload)
        {
            switch (eventLevel)
            {
                case EventLevel.Informational:
                    if (NLog.Common.InternalLogger.IsInfoEnabled)
                    {
                        WriteInternalLogger(LogLevel.Info, eventMessage, payload);
                    } break;
                case EventLevel.Warning:
                    if (NLog.Common.InternalLogger.IsWarnEnabled)
                    {
                        WriteInternalLogger(LogLevel.Warn, eventMessage, payload);
                    }
                    break;
                case EventLevel.Error:
                    if (NLog.Common.InternalLogger.IsErrorEnabled)
                    {
                        WriteInternalLogger(LogLevel.Error, eventMessage, payload);
                    }
                    break;
                case EventLevel.Critical:
                    if (NLog.Common.InternalLogger.IsFatalEnabled)
                    {
                        WriteInternalLogger(LogLevel.Fatal, eventMessage, payload);
                    }
                    break;
                default:
                    if (NLog.Common.InternalLogger.IsDebugEnabled)
                    {
                        WriteInternalLogger(LogLevel.Debug, eventMessage, payload);
                    }
                    break;
            }
        }

        private void WriteInternalLogger(LogLevel logLevel, string eventMessage, IReadOnlyList<object> payload)
        {
            if (payload is null || payload.Count == 0)
            {
                NLog.Common.InternalLogger.Log(logLevel, "{0}: {1}", _ownerTarget, eventMessage);
            }
            else if (payload.Count == 1)
            {
                NLog.Common.InternalLogger.Log(logLevel, "{0}: {1} {2}", _ownerTarget, eventMessage, payload[0]);
            }
            else if (payload.Count == 2)
            {
                NLog.Common.InternalLogger.Log(logLevel, "{0}: {1} {2} {3}", _ownerTarget, eventMessage, payload[0], payload[1]);
            }
            else if (payload.Count == 3)
            {
                NLog.Common.InternalLogger.Log(logLevel, "{0}: {1} {2} {3} {4}", _ownerTarget, eventMessage, payload[0], payload[1], payload[2]);
            }
            else
            {
                NLog.Common.InternalLogger.Log(logLevel, "{0}: {1} {2} {3} {4} {5}", _ownerTarget, eventMessage, payload[0], payload[1], payload[2], payload[3]);
            }
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name.StartsWith(EventSourceNamePrefix, StringComparison.Ordinal))
            {
                this.EnableEvents(eventSource, EventLevel, EventKeywords.All);
            }

            base.OnEventSourceCreated(eventSource);
        }
    }
}
