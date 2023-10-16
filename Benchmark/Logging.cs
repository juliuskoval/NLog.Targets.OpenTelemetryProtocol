using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using NLog;

namespace Benchmark;

[MemoryDiagnoser(false)]
public class Logging
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    [Benchmark]
    public void Log()
    {
        _logger.Info("message");
    }

    [Benchmark]
    public void Log1()
    {
        _logger.Info("{0}", "message");
    }


    [Benchmark]
    public void Log2()
    {
        _logger.Info("{0} {1}", "message", "message2");
    }

    [Benchmark]
    public void Log3()
    {
        _logger.Info("{0} {1} {2}", "message", "message2", "message3");
    }
}