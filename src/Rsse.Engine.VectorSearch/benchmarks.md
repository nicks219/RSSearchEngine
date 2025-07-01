## Разработка и оптимизация алгортимов поиска сервиса RSSE

### Идеи и варианты реализации: **[Viktor Loshmanov](https://github.com/ViktorLoshmanov)**

---
### Результаты бенчмарков различных алгоритмов

* коммит: доработка поискового движка (extended фильтр релевантности)
```
| Type                        | Method             | SearchType         | Mean          | Error      | StdDev     | Min           | Gen0      | Gen1      | Gen2   | Allocated    |
|---------------------------- |------------------- |------------------- |--------------:|-----------:|-----------:|--------------:|----------:|----------:|-------:|-------------:|
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | GinFast            |   113.7162 ms |  1.1770 ms |  0.7785 ms |   112.4263 ms | 1000.0000 |         - |      - |   70059.5 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | GinOptimized       |   173.2782 ms |  2.5735 ms |  1.5314 ms |   171.3618 ms | 1000.0000 |         - |      - |  70059.62 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | GinFastFilter      |   206.7772 ms |  2.0917 ms |  1.3835 ms |   204.8027 ms |  666.6667 |         - |      - |  44478.61 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | GinOptimizedFilter |   228.0318 ms |  2.3524 ms |  1.2303 ms |   225.9259 ms |  666.6667 |         - |      - |  44478.62 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | GinSimpleFilter    |   519.0966 ms |  3.6974 ms |  2.4456 ms |   515.2097 ms |         - |         - |      - |  33338.87 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Legacy             |   811.4064 ms | 12.2035 ms |  8.0719 ms |   792.4849 ms |         - |         - |      - |   4279.38 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | GinSimple          | 1,873.6174 ms |  3.3914 ms |  1.7738 ms | 1,870.3614 ms |         - |         - |      - |   4279.62 KB |
|                             |                    |                    |               |            |            |               |           |           |        |              |
| QueryBenchmarkReduced       | QueryReduced       | GinFastFilter      |     0.6830 ms |  0.0089 ms |  0.0053 ms |     0.6699 ms |    7.8125 |    2.9297 | 2.9297 |    578.15 KB |
| QueryBenchmarkReduced       | QueryReduced       | GinOptimizedFilter |     0.7108 ms |  0.0056 ms |  0.0033 ms |     0.7054 ms |    3.9063 |    2.9297 | 2.9297 |    578.15 KB |
| QueryBenchmarkReduced       | QueryReduced       | GinFast            |     0.8674 ms |  0.0154 ms |  0.0092 ms |     0.8546 ms |    7.8125 |    3.9063 | 3.9063 |    667.75 KB |
| QueryBenchmarkReduced       | QueryReduced       | GinOptimized       |     0.9139 ms |  0.0144 ms |  0.0095 ms |     0.9014 ms |    8.7891 |    4.8828 | 4.8828 |    667.76 KB |
| QueryBenchmarkReduced       | QueryReduced       | GinSimpleFilter    |     1.6575 ms |  0.0119 ms |  0.0071 ms |     1.6487 ms |    1.9531 |         - |      - |    263.13 KB |
| QueryBenchmarkReduced       | QueryReduced       | GinSimple          |     4.3933 ms |  0.1244 ms |  0.0740 ms |     4.2515 ms |         - |         - |      - |     10.52 KB |
| QueryBenchmarkReduced       | QueryReduced       | Legacy             |    12.6309 ms |  0.3499 ms |  0.2314 ms |    12.3180 ms |         - |         - |      - |     10.52 KB |
|                             |                    |                    |               |            |            |               |           |           |        |              |
| DuplicatesBenchmarkExtended | DuplicatesExtended | GinSimpleFilter    |    71.4985 ms |  1.1053 ms |  0.7311 ms |    70.4870 ms |  625.0000 |         - |      - |  43208.91 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | GinOptimizedFilter |   285.6757 ms |  3.6994 ms |  2.2014 ms |   282.5283 ms |  500.0000 |         - |      - |   44179.8 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Legacy             |   770.6592 ms |  3.9858 ms |  2.3719 ms |   766.9426 ms |         - |         - |      - |   2061.07 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | GinSimple          |   901.6697 ms | 17.2035 ms | 11.3791 ms |   886.5034 ms |         - |         - |      - |   2061.07 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | GinOptimized       |   956.2511 ms |  4.3860 ms |  2.6101 ms |   952.0236 ms |         - |         - |      - |  54485.88 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | GinFastFilter      | 1,372.7167 ms |  6.6229 ms |  4.3807 ms | 1,365.6579 ms | 9000.0000 | 2000.0000 |      - | 589826.16 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | GinFast            | 2,076.5755 ms |  6.0991 ms |  3.1899 ms | 2,070.5198 ms | 8000.0000 | 2000.0000 |      - | 548678.03 KB |
|                             |                    |                    |               |            |            |               |           |           |        |              |
| QueryBenchmarkExtended      | QueryExtended      | GinOptimizedFilter |     0.1087 ms |  0.0005 ms |  0.0003 ms |     0.1083 ms |    0.3662 |         - |      - |      25.9 KB |
| QueryBenchmarkExtended      | QueryExtended      | GinFastFilter      |     0.2620 ms |  0.0112 ms |  0.0074 ms |     0.2562 ms |    1.9531 |    1.9531 | 1.9531 |    283.69 KB |
| QueryBenchmarkExtended      | QueryExtended      | GinSimpleFilter    |     0.6934 ms |  0.0146 ms |  0.0096 ms |     0.6709 ms |         - |         - |      - |     13.31 KB |
| QueryBenchmarkExtended      | QueryExtended      | GinFast            |     1.2429 ms |  0.0390 ms |  0.0258 ms |     1.1998 ms |    1.9531 |    1.9531 | 1.9531 |     270.7 KB |
| QueryBenchmarkExtended      | QueryExtended      | GinOptimized       |     1.6833 ms |  0.0399 ms |  0.0264 ms |     1.6249 ms |    1.9531 |         - |      - |    252.45 KB |
| QueryBenchmarkExtended      | QueryExtended      | GinSimple          |     4.6114 ms |  0.1566 ms |  0.1036 ms |     4.4635 ms |         - |         - |      - |       0.3 KB |
| QueryBenchmarkExtended      | QueryExtended      | Legacy             |    12.1972 ms |  0.3300 ms |  0.2183 ms |    11.8357 ms |         - |         - |      - |      0.31 KB |
```

* коммит: ...
