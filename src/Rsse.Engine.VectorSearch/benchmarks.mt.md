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

* коммит: доработка поискового движка (оптимизация extended алгоритмов - добавлена проверка по дополнительному индексу, удалены simple лгоритмы)
```
| Type                     | Method        | SearchType                 | Mean      | Error     | StdDev    | Min       | Allocated     |
|------------------------- |-------------- |--------------------------- |----------:|----------:|----------:|----------:|--------------:|
| MtQueryBenchmarkExtended | QueryExtended |      GinFrozenDirectFilter |  19.43 ms |  2.762 ms |  1.826 ms |  17.92 ms |  145020.92 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinDirectOffsetFilter |  19.66 ms |  3.025 ms |  2.001 ms |  16.16 ms |  144423.77 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinDirectOffsetFilter |  20.43 ms |  2.067 ms |  1.367 ms |  18.91 ms |  145022.51 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinFrozenDirectFilter |  21.01 ms |  3.670 ms |  2.427 ms |  18.47 ms |  144424.03 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinOffsetFilter       |  22.65 ms |  1.824 ms |  1.206 ms |  20.80 ms |  145599.67 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinOffsetFilter       |  25.06 ms |  5.925 ms |  3.919 ms |  15.45 ms |  145246.65 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinOffset             |  31.06 ms |  0.811 ms |  0.424 ms |  30.47 ms |  144754.83 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinMergeFilter        |  31.36 ms |  2.258 ms |  1.493 ms |  29.23 ms |  144431.58 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinMergeFilter        |  32.36 ms |  2.187 ms |  1.446 ms |  30.09 ms |  145029.68 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinOffset             |  39.73 ms | 14.562 ms |  9.632 ms |  30.88 ms |  144376.23 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinFilter             |  49.17 ms |  3.713 ms |  2.209 ms |  47.16 ms |  144433.08 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinFastFilter         |  53.94 ms |  3.199 ms |  2.116 ms |  51.45 ms |  144443.64 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinFilter             |  60.17 ms |  4.457 ms |  2.948 ms |  56.14 ms |  252073.61 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinFastFilter         |  62.56 ms |  2.300 ms |  1.368 ms |  60.69 ms |  265073.60 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinMerge              | 152.02 ms |  4.182 ms |  2.766 ms | 148.97 ms |  145234.35 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinDirectOffset       | 155.46 ms |  2.073 ms |  1.084 ms | 153.45 ms |  145235.38 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinMerge              | 155.70 ms |  4.842 ms |  3.202 ms | 151.43 ms |  144455.39 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinDirectOffset       | 156.05 ms |  3.089 ms |  2.043 ms | 153.14 ms |  144439.68 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinFrozenDirect       | 172.38 ms |  2.948 ms |  1.950 ms | 169.36 ms |  144441.41 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinFrozenDirect       | 173.90 ms |  2.147 ms |  1.420 ms | 171.16 ms |  145228.71 KB |
| MtQueryBenchmarkExtended | QueryExtended | Pool GinFast               | 353.99 ms |  4.450 ms |  2.943 ms | 350.58 ms |  144374.48 KB |
| MtQueryBenchmarkExtended | QueryExtended |      GinFast               | 402.84 ms | 10.224 ms |  6.084 ms | 392.12 ms |  924028.35 KB |
| MtQueryBenchmarkExtended | QueryExtended |      Legacy                | 710.08 ms | 12.397 ms |  8.200 ms | 696.05 ms |  144437.26 KB |
|                          |               |                            |           |           |           |           |               |
| MtQueryBenchmarkReduced  | QueryReduced  |      GinMergeFilter        |  86.04 ms |  7.612 ms |  5.035 ms |  79.40 ms |  566388.65 KB |
| MtQueryBenchmarkReduced  | QueryReduced  | Pool GinMergeFilter        |  87.42 ms |  4.354 ms |  2.591 ms |  82.94 ms |  566102.04 KB |
| MtQueryBenchmarkReduced  | QueryReduced  |      GinMerge              |  87.53 ms |  5.493 ms |  3.633 ms |  82.15 ms |  566405.82 KB |
| MtQueryBenchmarkReduced  | QueryReduced  | Pool GinOptimized          |  87.68 ms |  6.857 ms |  4.535 ms |  81.58 ms |  566148.16 KB |
| MtQueryBenchmarkReduced  | QueryReduced  | Pool GinMerge              |  90.09 ms |  6.366 ms |  4.210 ms |  84.15 ms |  566117.01 KB |
| MtQueryBenchmarkReduced  | QueryReduced  | Pool GinFast               |  96.22 ms |  7.708 ms |  5.098 ms |  87.86 ms |  566147.46 KB |
| MtQueryBenchmarkReduced  | QueryReduced  | Pool GinOptimizedFilter    | 106.04 ms |  4.347 ms |  2.875 ms | 103.08 ms |  566107.79 KB |
| MtQueryBenchmarkReduced  | QueryReduced  | Pool GinFastFilter         | 112.76 ms |  1.929 ms |  1.276 ms | 110.82 ms |  566125.68 KB |
| MtQueryBenchmarkReduced  | QueryReduced  |      GinOptimizedFilter    | 132.04 ms | 12.050 ms |  7.970 ms | 121.15 ms | 1119587.46 KB |
| MtQueryBenchmarkReduced  | QueryReduced  |      GinFastFilter         | 149.42 ms | 10.741 ms |  7.104 ms | 142.74 ms | 1160773.17 KB |
| MtQueryBenchmarkReduced  | QueryReduced  |      GinFast               | 154.88 ms |  6.834 ms |  4.066 ms | 148.67 ms | 1455055.54 KB |
| MtQueryBenchmarkReduced  | QueryReduced  | Pool GinFilter             | 187.09 ms |  4.932 ms |  3.262 ms | 181.11 ms |  569910.68 KB |
| MtQueryBenchmarkReduced  | QueryReduced  |      GinFilter             | 208.69 ms |  8.524 ms |  5.638 ms | 200.87 ms | 1095786.65 KB |
| MtQueryBenchmarkReduced  | QueryReduced  |      GinOptimized          | 213.05 ms |  7.491 ms |  4.955 ms | 203.16 ms | 2223517.59 KB |
| MtQueryBenchmarkReduced  | QueryReduced  |      Legacy                | 668.23 ms | 18.827 ms | 12.453 ms | 647.95 ms |  566355.30 KB |
```

* коммит: ...
