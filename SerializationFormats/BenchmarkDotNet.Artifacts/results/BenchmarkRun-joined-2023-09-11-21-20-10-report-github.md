```

BenchmarkDotNet v0.13.8, macOS Ventura 13.5 (22G74) [Darwin 22.6.0]
Apple M1 Pro, 1 CPU, 8 logical and 8 physical cores
.NET SDK 8.0.100-preview.7.23376.3
  [Host]     : .NET 7.0.10 (7.0.1023.36312), Arm64 RyuJIT AdvSIMD
  Job-UITTJG : .NET 7.0.10 (7.0.1023.36312), Arm64 RyuJIT AdvSIMD

RunStrategy=ColdStart  

```

| Type            | Method      |        Mean |       Error |       StdDev |     Median | Ratio | RatioSD | Allocated | Alloc Ratio |
|-----------------|-------------|------------:|------------:|-------------:|-----------:|------:|--------:|----------:|------------:|
| Deserialization | JSON        |    57.57 μs |   131.33 μs |    387.23 μs |  16.688 μs |  0.82 |    0.40 |        NA |          NA |
| Full            | JSON        |   706.91 μs | 2,314.20 μs |  6,823.46 μs |  21.771 μs |  1.00 |    0.00 |        NA |          NA |
| Serialization   | JSON        |   685.97 μs | 2,286.92 μs |  6,743.03 μs |  10.271 μs |  0.51 |    0.22 |   3.88 KB |          NA |
| Deserialization | XML         |   144.33 μs |   255.68 μs |    753.87 μs |  49.875 μs |  2.92 |    5.47 |        NA |          NA |
| Full            | XML         |   627.60 μs | 1,888.58 μs |  5,568.53 μs |  65.562 μs |  3.04 |    0.76 |        NA |          NA |
| Serialization   | XML         |   499.56 μs | 1,605.13 μs |  4,732.78 μs |  23.479 μs |  1.11 |    0.27 |  26.57 KB |          NA |
| Deserialization | YAML        |   512.23 μs |   355.19 μs |  1,047.29 μs | 396.917 μs | 17.84 |    4.22 |        NA |          NA |
| Full            | YAML        | 1,453.65 μs | 2,376.99 μs |  7,008.60 μs | 743.604 μs | 33.14 |    7.87 |        NA |          NA |
| Serialization   | YAML        |   949.08 μs | 1,993.39 μs |  5,877.54 μs | 352.041 μs | 15.93 |    3.96 |  52.27 KB |          NA |
| Deserialization | MsgPack     |    55.48 μs |   154.48 μs |    455.48 μs |   9.021 μs |  0.43 |    0.13 |        NA |          NA |
| Full            | MessagePack | 1,108.72 μs | 3,708.61 μs | 10,934.92 μs |  13.584 μs |  0.67 |    0.22 |        NA |          NA |
| Serialization   | MessagePack | 1,076.00 μs | 3,629.62 μs | 10,702.02 μs |   5.146 μs |  0.26 |    0.15 |   2.03 KB |          NA |
| Deserialization | ProtoBuf    |    41.26 μs |   103.59 μs |    305.44 μs |   9.500 μs |  0.46 |    0.18 |        NA |          NA |
| Full            | ProtoBuf    |   514.83 μs | 1,705.76 μs |  5,029.47 μs |  10.375 μs |  0.52 |    0.16 |        NA |          NA |
| Serialization   | ProtoBuf    |   503.53 μs | 1,672.17 μs |  4,930.43 μs |   9.354 μs |  0.47 |    0.17 |   7.33 KB |          NA |
| Deserialization | Avro        | 1,027.16 μs | 1,617.36 μs |  4,768.81 μs | 529.792 μs | 24.31 |    6.89 |        NA |          NA |
| Full            | Avro        | 2,198.98 μs | 4,717.83 μs | 13,910.64 μs | 768.271 μs | 35.76 |   11.79 |        NA |          NA |
| Serialization   | Avro        | 1,226.62 μs | 3,242.65 μs |  9,561.01 μs | 258.042 μs | 11.90 |    3.05 |  94.88 KB |          NA |
