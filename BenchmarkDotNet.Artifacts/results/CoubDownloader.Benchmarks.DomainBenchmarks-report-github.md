```

BenchmarkDotNet v0.14.0, Ubuntu 26.04 LTS (Resolute Raccoon)
AMD EPYC-Rome Processor, 1 CPU, 16 logical and 16 physical cores
.NET SDK 10.0.300
  [Host]     : .NET 10.0.8 (10.0.826.23019), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.8 (10.0.826.23019), X64 RyuJIT AVX2


```
| Method                | Mean        | Error     | StdDev    | Median      | Gen0   | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|-------:|----------:|
| GetFormattedViewCount |  33.2163 ns | 0.7020 ns | 0.8621 ns |  33.3863 ns | 0.0038 |      32 B |
| GetFormattedFileSize  | 107.3630 ns | 1.9146 ns | 1.7909 ns | 107.2712 ns | 0.0048 |      40 B |
| EstimateOutputSize    |   0.6638 ns | 0.0494 ns | 0.0754 ns |   0.6531 ns |      - |         - |
| GetProgressPercent    |   0.0200 ns | 0.0276 ns | 0.0307 ns |   0.0000 ns |      - |         - |
