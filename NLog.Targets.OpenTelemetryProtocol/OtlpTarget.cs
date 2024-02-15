using OpenTelemetry.Logs;

namespace NLog.Targets
{
    [Target("OtlpTarget")]
    public class OtlpTarget : OtlpTargetBase
    {
        protected override void AppendAttributes(LogEventInfo logEvent, ref LogRecordAttributeList attributes)
        {
            if (logEvent.Exception != null)
                attributes.RecordException(logEvent.Exception);

            if (ShouldIncludeProperties(logEvent))
            {
                var properties = GetAllProperties(logEvent);
                foreach (var property in properties)
                {
                    attributes.Add(property.Key, property.Value);
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

                if (ContextProperties?.Count > 0)
                {
                    for (int i = 0; i< ContextProperties.Count; i++)
                    {
                        var property = ContextProperties[i];
                        var value = property.RenderValue(logEvent);
                        if (!property.IncludeEmptyValue && (value is null || string.Empty.Equals(value)))
                            continue;

                        attributes.Add(property.Name, value);
                    }
                }
            }
        }
    }
}