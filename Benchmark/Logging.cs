using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using NLog;

namespace Benchmark;

[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser(false)]
public class Logging
{
    private readonly Logger _logger = LogManager.GetLogger("otlp");
    private readonly Logger _loggerLightweight = LogManager.GetLogger("lightweight");

    [Benchmark]
    public void Log()
    {
        _logger.Info("message");
    }

    [Benchmark]
    public void Log1()
    {
        _logger.Info("{first}", "message");
    }

    [Benchmark]
    public void Log2()
    {
        _logger.Info("{first} {second}", "message", "message2");
    }

    [Benchmark]
    public void Log_Lightweight()
    {
        _loggerLightweight.Info("message");
    }

    [Benchmark]
    public void Log1_lightweight()
    {
        _loggerLightweight.Info("{first}", "message");
    }

    [Benchmark]
    public void Log2_Lightweight()
    {
        _loggerLightweight.Info("{first} {second}", "message", "message2");
    }
}