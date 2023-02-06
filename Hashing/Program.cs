using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Hashing.Benchmark;

namespace Hashing;

public class Program
{
    public static void Main(string[] args)
    {
        var _ = BenchmarkRunner.Run<HashAlgorithmsBenchmark>(ManualConfig.Create(DefaultConfig.Instance)
            .WithOptions(ConfigOptions.DisableOptimizationsValidator));

        // uncomment to run hash with serialisation benchmarks
        // var __ = BenchmarkRunner.Run<SerialisationHashBenchmarks>(ManualConfig.Create(DefaultConfig.Instance)
        //     .WithOptions(ConfigOptions.DisableOptimizationsValidator));
    }
}