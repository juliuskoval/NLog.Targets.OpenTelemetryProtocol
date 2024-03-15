using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using NLog;

namespace Benchmark;

[SimpleJob(RuntimeMoniker.Net48,baseline:true)]
[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser(false)]
public class Logging
{
    private readonly Logger _logger = LogManager.GetLogger("otlp");

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
}