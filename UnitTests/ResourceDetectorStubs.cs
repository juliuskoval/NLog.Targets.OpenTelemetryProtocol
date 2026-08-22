using System.Runtime.InteropServices;
using OpenTelemetry.Resources;

namespace UnitTests;

/// <summary>
/// Stands in for a detector from a package like OpenTelemetry.Resources.AWS, whose detector types are internal
/// and only reachable through an extension method.
/// </summary>
internal sealed class StubDetector : IResourceDetector
{
    private readonly string _key;
    private readonly string _value;

    internal StubDetector(string key, string value)
    {
        _key = key;
        _value = value;
    }

    public Resource Detect() => new Resource(new[] { new KeyValuePair<string, object>(_key, _value) });
}

/// <summary>
/// A detector that can be registered directly by type name, the way a user's own detector would be.
/// </summary>
public sealed class PublicStubDetector : IResourceDetector
{
    public Resource Detect() => new Resource(new[] { new KeyValuePair<string, object>("public.detector", "detected") });
}

public sealed class ThrowingStubDetector : IResourceDetector
{
    public Resource Detect() => throw new InvalidOperationException("detector failed");
}

public sealed class NoParameterlessConstructorDetector : IResourceDetector
{
    public NoParameterlessConstructorDetector(string _) { }

    public Resource Detect() => Resource.Empty;
}

/// <summary>
/// Mirrors the shape of the official packages: a public static class whose methods take a ResourceBuilder.
/// </summary>
public static class StubResourceBuilderExtensions
{
    /// <summary>Mirrors AddAWSEC2Detector - a trailing optional parameter.</summary>
    public static ResourceBuilder AddStubDetector(this ResourceBuilder builder, Action<object>? configure = null)
        => builder.AddDetector(new StubDetector("stub.detector", "detected"));

    /// <summary>Mirrors AddHostDetector - no optional parameter.</summary>
    public static ResourceBuilder AddSimpleStubDetector(this ResourceBuilder builder)
        => builder.AddDetector(new StubDetector("simple.stub.detector", "detected"));

    /// <summary>Two overloads, so the resolver has to choose one.</summary>
    public static ResourceBuilder AddOverloadedStubDetector(this ResourceBuilder builder)
        => builder.AddDetector(new StubDetector("overload.parameters", "0"));

    public static ResourceBuilder AddOverloadedStubDetector(this ResourceBuilder builder, Action<object>? configure)
        => builder.AddDetector(new StubDetector("overload.parameters", "1"));

    /// <summary>A required second parameter means this cannot be called from configuration.</summary>
    public static ResourceBuilder AddUncallableStubDetector(this ResourceBuilder builder, string required)
        => builder.AddDetector(new StubDetector("uncallable.detector", required));

    /// <summary>Optional, but with no default value to pass along, so it cannot be called either.</summary>
    public static ResourceBuilder AddOptionalWithoutDefaultStubDetector(this ResourceBuilder builder, [Optional] int flag)
        => builder.AddDetector(new StubDetector("optional.without.default", "detected"));

    public static ResourceBuilder AddThrowingStubDetector(this ResourceBuilder builder)
        => throw new InvalidOperationException("registration failed");

    /// <summary>Registers fine, but the detector it registers fails once it actually detects.</summary>
    public static ResourceBuilder AddDetectorThatThrowsWhenDetecting(this ResourceBuilder builder)
        => builder.AddDetector(new ThrowingStubDetector());
}

/// <summary>
/// Not a static class, so it must not be picked up when searching for an extension method.
/// </summary>
public sealed class NotAStaticClass
{
    public static ResourceBuilder AddSimpleStubDetector(ResourceBuilder builder)
        => builder.AddDetector(new StubDetector("simple.stub.detector", "wrong"));
}
