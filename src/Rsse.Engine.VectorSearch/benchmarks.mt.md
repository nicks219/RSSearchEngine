## Разработка и оптимизация алгортимов поиска сервиса RSSE

### Идеи и варианты реализации: **[Viktor Loshmanov](https://github.com/ViktorLoshmanov)**

---
### Результаты бенчмарков различных алгоритмов в многопоточном запуске на наборе запросов
### Выполняется 1000 запросов в одном бенчмарке, результаты делить на 1000

* коммит: доработка поискового движка (добавлен direct index спользующий FrozenDictionary)
```
| Type                     | Method        | SearchType                 | Mean      | Error     | StdDev    | Min       | Allocated     |
|------------------------- |-------------- |--------------------------- |----------:|----------:|----------:|----------:|--------------:|
| MtQueryBenchmarkExtended | QueryExtended | Pool GinMergeFilter        |  32.74 ms |  0.948 ms |  0.564 ms |  32.02 ms |  144090.65 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinMergeFilter        |  33.27 ms |  1.999 ms |  1.322 ms |  31.27 ms |  144688.91 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinFrozenDirectFilter |  34.25 ms |  1.411 ms |  0.738 ms |  33.33 ms |  144694.73 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinDirectOffsetFilter |  34.40 ms |  4.047 ms |  2.677 ms |  30.99 ms |  144692.78 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinFrozenDirectFilter |  34.55 ms |  1.726 ms |  1.142 ms |  33.50 ms |  144094.21 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinDirectOffsetFilter |  35.21 ms |  2.887 ms |  1.910 ms |  32.63 ms |  144087.17 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinOffsetFilter       |  36.54 ms |  8.809 ms |  5.827 ms |  26.68 ms |  145283.91 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinOffsetFilter       |  36.61 ms | 10.802 ms |  7.145 ms |  25.94 ms |  144898.04 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinOffset             |  43.93 ms |  2.037 ms |  1.065 ms |  43.16 ms |  144035.10 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinOffset             |  44.22 ms |  1.470 ms |  0.875 ms |  43.46 ms |  144418.45 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinFilter             |  50.98 ms |  6.450 ms |  3.838 ms |  47.47 ms |  144096.68 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinFastFilter         |  59.26 ms |  1.832 ms |  1.212 ms |  57.31 ms |  144086.70 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinFilter             |  60.03 ms |  6.038 ms |  3.994 ms |  54.24 ms |  251701.02 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinFastFilter         |  63.02 ms |  1.754 ms |  1.044 ms |  61.12 ms |  264724.06 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinMerge              | 154.17 ms |  6.137 ms |  4.059 ms | 149.21 ms |  144898.82 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinMerge              | 154.48 ms |  3.659 ms |  2.177 ms | 151.69 ms |  144158.30 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinDirectOffset       | 180.75 ms |  3.795 ms |  2.510 ms | 175.68 ms |  144121.70 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinDirectOffset       | 185.08 ms |  6.754 ms |  4.467 ms | 180.69 ms |  144890.89 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinFrozenDirect       | 204.46 ms |  1.933 ms |  1.278 ms | 202.87 ms |  144102.84 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinFrozenDirect       | 208.65 ms |  5.730 ms |  3.790 ms | 204.11 ms |  144903.09 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinFast               | 352.07 ms |  7.587 ms |  5.019 ms | 344.92 ms |  144037.59 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinOptimized          | 374.39 ms |  7.440 ms |  4.921 ms | 365.48 ms |  144036.32 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinFast               | 417.54 ms | 17.278 ms | 11.429 ms | 397.16 ms |  923714.80 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinOptimized          | 426.83 ms | 15.371 ms |  9.147 ms | 414.53 ms |  923692.52 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinSimple             | 649.44 ms |  3.088 ms |  2.043 ms | 646.47 ms |  144097.93 KB |
| MtQueryBenchmarkExtended | QueryExtended |      Legacy                | 715.33 ms | 13.984 ms |  9.249 ms | 698.99 ms |  144107.53 KB |
|                          |               |                            |           |           |           |           |               |
| MtQueryBenchmarkReduced  | QueryReduced  | Pool GinMergeFilter        |  82.76 ms | 10.015 ms |  6.624 ms |  72.91 ms |  565704.07 KB |
| MtQueryBenchmarkReduced  | QueryReduced  |      GinMergeFilter        |  87.44 ms |  4.548 ms |  2.707 ms |  83.33 ms |  565990.55 KB |
| MtQueryBenchmarkReduced  | QueryReduced  |      GinMerge              |  89.52 ms |  5.541 ms |  3.665 ms |  86.10 ms |  565979.59 KB |
| MtQueryBenchmarkReduced  | QueryReduced  | Pool GinMerge              |  93.58 ms |  9.908 ms |  6.554 ms |  86.99 ms |  565735.24 KB |
| MtQueryBenchmarkReduced  | QueryReduced  | Pool GinFast               |  94.56 ms |  7.623 ms |  5.042 ms |  86.74 ms |  565706.91 KB |
| MtQueryBenchmarkReduced  | QueryReduced  | Pool GinOptimized          | 101.18 ms |  5.700 ms |  3.770 ms |  95.32 ms |  565711.43 KB |
| MtQueryBenchmarkReduced  | QueryReduced  | Pool GinOptimizedFilter    | 105.70 ms |  5.426 ms |  2.838 ms | 101.30 ms |  565715.71 KB |
| MtQueryBenchmarkReduced  | QueryReduced  | Pool GinFastFilter         | 116.59 ms |  4.707 ms |  2.801 ms | 112.94 ms |  565729.82 KB |
| MtQueryBenchmarkReduced  | QueryReduced  |      GinOptimizedFilter    | 137.44 ms | 13.115 ms |  8.674 ms | 125.13 ms | 1119215.92 KB |
| MtQueryBenchmarkReduced  | QueryReduced  |      GinFastFilter         | 147.20 ms |  5.968 ms |  3.948 ms | 140.46 ms | 1160457.61 KB |
| MtQueryBenchmarkReduced  | QueryReduced  |      GinFast               | 152.89 ms |  7.902 ms |  5.227 ms | 144.75 ms | 1454603.59 KB |
| MtQueryBenchmarkReduced  | QueryReduced  | Pool GinFilter             | 188.25 ms |  8.082 ms |  5.346 ms | 178.60 ms |  565741.65 KB |
| MtQueryBenchmarkReduced  | QueryReduced  |      GinFilter             | 215.64 ms | 23.431 ms | 15.498 ms | 192.65 ms | 1095369.88 KB |
| MtQueryBenchmarkReduced  | QueryReduced  |      GinOptimized          | 218.26 ms |  9.340 ms |  6.178 ms | 206.68 ms | 2223138.40 KB |
| MtQueryBenchmarkReduced  | QueryReduced  |      GinSimple             | 571.41 ms | 15.823 ms | 10.466 ms | 552.93 ms |  565879.69 KB |
| MtQueryBenchmarkReduced  | QueryReduced  |      Legacy                | 689.90 ms | 23.159 ms | 15.318 ms | 661.65 ms |  565987.03 KB |
```

* коммит: ...
