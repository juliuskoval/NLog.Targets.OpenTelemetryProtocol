using System;
using System.Collections.Generic;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets.OpenTelemetryProtocol.Exceptions;
using OpenTelemetry.Logs;

namespace NLog.Targets
{
    [Target("OtlpTargetLightweight")]
    public class OtlpTargetLightweight : OtlpTargetBase
    {
        private List<KeyValuePair<string, Layout>> _attributes = new();

        private static readonly LogEventInfo NullEvent = LogEventInfo.CreateNullEvent();

        protected override void InitializeTarget()
        {
            if (Attributes is not null)
                ParseAttributes();

            base.InitializeTarget();
        }

        private void ParseAttributes()
        {
            try
            {
                foreach (var attribute in Attributes)
                {
                    _attributes.Add(new KeyValuePair<string, Layout>(attribute.Name, attribute.Layout));
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "OtlpTarget(Name={0}) - An exception occured when parsing attributes.", Name);
                throw new FailedToParseAttributesException("Failed to parse attributes");
            }
        }

        protected override void AppendAttributes(LogEventInfo logEvent, ref LogRecordAttributeList attributes)
        {
            if (logEvent.Exception != null)
                attributes.RecordException(logEvent.Exception);

            if (logEvent.Properties?.Count > 0)
            {
                foreach (var property in logEvent.Properties)
                {
                    attributes.Add(property.Key.ToString(), property.Value);
                }
            }
            else
            {
                if (IncludeEventParameters && logEvent.Parameters?.Length > 0)
                {
                    for (int i = 0; i < logEvent.Parameters.Length; i++)
                    {
                        attributes.Add(ResolveParameterKey(i), logEvent.Parameters[i]);
                    }
                }
            }
            
            if (_attributes?.Count > 0)
            {
                foreach (var kvp in _attributes)
                {
                    attributes.Add(kvp.Key, RenderLogEvent(kvp.Value, NullEvent));
                }
            }
        }
    }
}