using BenchmarkDotNet.Running;

namespace CoubDownloader.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<DomainBenchmarks>();
    }
}
