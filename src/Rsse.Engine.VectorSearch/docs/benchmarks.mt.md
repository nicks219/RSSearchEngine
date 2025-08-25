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

* коммит: доработка поискового движка (добавлено хранение данных документа в одном массиве GinArrayDirect, удалены extended fast алгоритмы)
```
| Type                        | Method             | SearchType                   | Mean           | Error       | StdDev      | Min            | Allocated     |
|---------------------------- |------------------- |----------------------------- |---------------:|------------:|------------:|---------------:|--------------:|
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinDirectOffsetFilterBs |     18.0204 ms |   6.8063 ms |   4.0503 ms |     13.6137 ms |  144424.15 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinDirectOffsetFilterLs |     19.0219 ms |   2.7601 ms |   1.8256 ms |     16.4661 ms |  145025.88 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinFrozenDirectFilter   |     19.2190 ms |   3.6939 ms |   2.4433 ms |     16.0064 ms |  144420.83 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinFrozenDirectFilter   |     19.7183 ms |   1.9820 ms |   1.1795 ms |     17.9712 ms |  145015.68 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinOffsetFilter         |     21.5025 ms |   8.5211 ms |   5.0708 ms |     14.9695 ms |  145618.00 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinOffsetFilter         |     22.2032 ms |   1.8515 ms |   1.2246 ms |     20.2943 ms |  145218.52 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinDirectOffsetFilterLs |     23.1414 ms |  11.3120 ms |   7.4822 ms |     14.4747 ms |  144427.92 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinDirectOffsetFilterBs |     28.5267 ms |  12.8462 ms |   8.4970 ms |     14.5505 ms |  145032.22 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterLs  |     29.3508 ms |   4.0571 ms |   2.4143 ms |     25.6425 ms |  144436.82 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinOffset               |     30.7770 ms |   0.6312 ms |   0.4175 ms |     30.1847 ms |  144753.88 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinMergeFilter          |     31.3973 ms |   3.9002 ms |   2.5797 ms |     27.9013 ms |  145025.79 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterLs  |     31.4728 ms |   1.5113 ms |   0.9996 ms |     30.2748 ms |  145026.22 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterBs  |     32.3805 ms |   2.3180 ms |   1.5332 ms |     30.1738 ms |  145029.05 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterBs  |     32.8529 ms |   3.2374 ms |   2.1414 ms |     28.7123 ms |  144427.87 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinMergeFilter          |     34.0518 ms |   9.4243 ms |   5.6083 ms |     25.5373 ms |  144432.66 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinOffset               |     35.6157 ms |   7.8424 ms |   4.6669 ms |     32.5187 ms |  144374.71 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinFilter               |     53.2747 ms |   2.3480 ms |   1.5531 ms |     50.9459 ms |  144426.33 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinFilter               |     57.5265 ms |   9.4366 ms |   5.6156 ms |     52.3055 ms |  252038.15 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectLs        |    148.2876 ms |   4.1045 ms |   2.7149 ms |    145.5230 ms |  145181.90 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectBs        |    151.7707 ms |   2.2235 ms |   1.1629 ms |    149.4654 ms |  144406.29 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinMerge                |    153.1540 ms |   3.1320 ms |   1.8638 ms |    150.6400 ms |  145179.36 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectBs        |    153.5487 ms |   4.8790 ms |   2.9034 ms |    150.6776 ms |  145183.51 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectLs        |    153.7418 ms |   2.2609 ms |   1.4954 ms |    151.9591 ms |  144459.11 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinMerge                |    157.3447 ms |   5.5259 ms |   3.6551 ms |    153.0806 ms |  144463.18 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinDirectOffsetLs       |    157.6870 ms |   2.8302 ms |   1.8720 ms |    154.4631 ms |  145239.44 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinDirectOffsetLs       |    158.7568 ms |   1.9673 ms |   1.0289 ms |    157.0602 ms |  144460.20 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinFrozenDirect         |    168.8315 ms |   3.5583 ms |   2.3536 ms |    166.1261 ms |  145181.39 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinFrozenDirect         |    174.4695 ms |   4.5346 ms |   2.9994 ms |    171.4419 ms |  144442.46 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinDirectOffsetBs       |    182.9615 ms |   3.3677 ms |   2.0041 ms |    179.3245 ms |  145224.57 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinDirectOffsetBs       |    184.4180 ms |   4.0627 ms |   2.6872 ms |    179.7723 ms |  144459.32 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      Legacy                  |    712.3820 ms |  16.0581 ms |  10.6214 ms |    699.9176 ms |  144438.34 KB |
|                             |                    |                              |                |             |             |                |               |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinMergeFilter          |     84.5345 ms |   4.1006 ms |   2.7123 ms |     81.2304 ms |  566106.66 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinMerge                |     85.4822 ms |   5.7265 ms |   3.7877 ms |     79.1707 ms |  566139.34 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinMergeFilter          |     85.5833 ms |   5.8451 ms |   3.8662 ms |     79.7616 ms |  566386.62 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimized            |     87.4692 ms |   7.7007 ms |   5.0935 ms |     79.9337 ms |  566135.41 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinMerge                |     88.1474 ms |   1.9270 ms |   1.1467 ms |     85.8871 ms |  566370.18 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinFast                 |     91.6509 ms |   4.3988 ms |   2.6176 ms |     86.3044 ms |  566138.41 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimizedFilter      |    102.0294 ms |   4.9639 ms |   3.2833 ms |     97.7593 ms |  566105.18 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinFastFilter           |    108.5115 ms |   2.9200 ms |   1.9314 ms |    105.3935 ms |  566093.71 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinOptimizedFilter      |    132.1776 ms |   9.0776 ms |   6.0043 ms |    123.2498 ms | 1119586.24 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinFastFilter           |    137.5022 ms |   7.2771 ms |   4.8134 ms |    130.6065 ms | 1001101.60 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinFast                 |    147.0116 ms |  15.2060 ms |  10.0579 ms |    137.1884 ms | 1455009.82 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinFilter               |    175.5951 ms |   3.8425 ms |   2.2866 ms |    172.8324 ms |  566138.18 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinFilter               |    207.3040 ms |   7.6515 ms |   5.0610 ms |    198.6808 ms | 1095773.01 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinOptimized            |    224.3144 ms |  12.3213 ms |   8.1498 ms |    204.8093 ms | 2223475.27 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      Legacy                  |    668.7984 ms |  20.0605 ms |  13.2688 ms |    650.6919 ms |  566226.30 KB |
```

* коммит: доработка поискового движка (оптимизация GinArrayDirect)
```
| Type                        | Method             | SearchType                  | Mean           | Error       | StdDev      | Min            | Allocated     |
|---------------------------- |------------------- |---------------------------- |---------------:|------------:|------------:|---------------:|--------------:|
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterLs |     19.3285 ms |   2.1162 ms |   1.3997 ms |     16.2606 ms |  145022.62 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterHs |     19.3487 ms |   1.8628 ms |   1.1086 ms |     17.1410 ms |  145029.07 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterHs |     19.6481 ms |   2.8853 ms |   1.9084 ms |     15.9929 ms |  144423.53 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterBs |     19.7212 ms |   2.4737 ms |   1.6362 ms |     17.2457 ms |  145030.70 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterLs |     19.9025 ms |   1.9362 ms |   1.1522 ms |     17.7143 ms |  144423.64 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinOffsetFilter        |     22.5370 ms |   1.4896 ms |   0.9853 ms |     21.2734 ms |  145609.68 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinOffsetFilter        |     22.5600 ms |   1.9463 ms |   1.1582 ms |     20.3243 ms |  145225.41 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterBs |     26.8545 ms |   9.6746 ms |   6.3991 ms |     15.5062 ms |  144427.31 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinOffset              |     31.8049 ms |   2.9557 ms |   1.5459 ms |     30.4966 ms |  144754.82 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinOffset              |     38.9748 ms |   1.4335 ms |   0.9482 ms |     37.2434 ms |  144408.68 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectLs       |    120.6632 ms |   2.1527 ms |   1.4239 ms |    118.7225 ms |  145181.11 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectLs       |    125.6177 ms |   1.5106 ms |   0.9992 ms |    124.2683 ms |  144439.60 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectBs       |    128.8505 ms |   3.1387 ms |   1.8678 ms |    126.2824 ms |  144406.05 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectBs       |    131.7919 ms |   2.0265 ms |   1.2059 ms |    130.2342 ms |  145236.09 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectHs       |    166.8974 ms |   2.7067 ms |   1.7903 ms |    162.7561 ms |  145237.87 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectHs       |    167.5712 ms |   2.1314 ms |   1.4098 ms |    164.9142 ms |  144466.62 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      Legacy                 |    710.2712 ms |  15.3166 ms |  10.1310 ms |    696.0963 ms |  144437.82 KB |
|                             |                    |                             |                |             |             |                |               |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinMergeFilter         |     84.6575 ms |   3.2199 ms |   2.1298 ms |     80.8079 ms |  566401.67 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinMergeFilter         |     85.6639 ms |   5.1327 ms |   3.3950 ms |     80.1657 ms |  566131.95 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinMerge               |     89.1208 ms |   6.4919 ms |   4.2940 ms |     82.5366 ms |  566108.80 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinMerge               |     90.8135 ms |   4.6223 ms |   3.0574 ms |     85.7151 ms |  566388.58 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimized           |     98.8519 ms |  13.3734 ms |   8.8457 ms |     83.6186 ms |  566136.59 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinFast                |    100.0384 ms |   8.6138 ms |   5.6975 ms |     92.6240 ms |  566135.79 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimizedFilter     |    110.4083 ms |   3.2262 ms |   1.9199 ms |    106.8882 ms |  566131.48 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinFastFilter          |    115.4842 ms |   3.2566 ms |   2.1540 ms |    112.8323 ms |  566306.67 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinOptimizedFilter     |    134.7354 ms |  11.0650 ms |   7.3188 ms |    124.4279 ms | 1119633.54 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinFastFilter          |    134.7972 ms |   8.2305 ms |   5.4439 ms |    128.2972 ms | 1001131.29 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinFast                |    146.6033 ms |   9.9382 ms |   6.5735 ms |    136.8778 ms | 1455039.88 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinOptimized           |    185.0273 ms |  11.0774 ms |   7.3270 ms |    173.4036 ms | 2223527.70 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinFilter              |    185.0288 ms |   6.3390 ms |   3.7722 ms |    181.4096 ms |  566131.69 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinFilter              |    207.8770 ms |   7.6295 ms |   5.0464 ms |    199.9532 ms | 1095788.44 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      Legacy                 |    665.5148 ms |  14.6621 ms |   8.7252 ms |    647.1275 ms |  566281.48 KB |
```

* коммит: доработка поискового движка (оптимизация выделения памяти для метрик)
```
| Type                        | Method             | SearchType                  | Mean           | Error       | StdDev      | Min            | Allocated     |
|---------------------------- |------------------- |---------------------------- |---------------:|------------:|------------:|---------------:|--------------:|
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterHs |       3.189 ms |   0.0056 ms |   0.0037 ms |       3.182 ms |     673.78 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterLs |       3.191 ms |   0.0077 ms |   0.0046 ms |       3.183 ms |    1273.69 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterHs |       3.198 ms |   0.0079 ms |   0.0053 ms |       3.191 ms |    1273.68 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterLs |       3.245 ms |   0.0490 ms |   0.0292 ms |       3.225 ms |     673.78 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterBs |       3.255 ms |   0.0178 ms |   0.0118 ms |       3.232 ms |     673.78 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterBs |       3.357 ms |   0.0086 ms |   0.0057 ms |       3.349 ms |    1273.68 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinOffsetFilter        |       7.410 ms |   0.0573 ms |   0.0341 ms |       7.322 ms |    1853.58 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinOffsetFilter        |       7.681 ms |   0.0223 ms |   0.0133 ms |       7.658 ms |    1474.34 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinOffset              |      27.524 ms |   0.7028 ms |   0.4182 ms |      26.991 ms |    1022.70 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinOffset              |      28.412 ms |   1.8676 ms |   1.1114 ms |      27.459 ms |     642.92 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectLs       |     115.304 ms |   1.9281 ms |   1.2753 ms |     113.678 ms |    1448.56 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectLs       |     117.398 ms |   2.1218 ms |   1.4034 ms |     115.815 ms |     673.86 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectBs       |     118.297 ms |   3.8990 ms |   2.5790 ms |     113.921 ms |     674.73 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectBs       |     123.013 ms |   3.9516 ms |   2.6138 ms |     118.880 ms |    1449.72 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectHs       |     162.242 ms |   3.5991 ms |   2.3806 ms |     158.726 ms |     673.94 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectHs       |     162.560 ms |   1.4592 ms |   0.9652 ms |     160.533 ms |    1448.22 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      Legacy                 |     661.094 ms |   2.1971 ms |   1.4533 ms |     658.930 ms |     705.95 KB |
|                             |                    |                             |                |             |             |                |               |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinMergeFilter         |      34.602 ms |   0.2736 ms |   0.1810 ms |      34.411 ms |    1329.22 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinMergeFilter         |      35.278 ms |   0.4405 ms |   0.2304 ms |      34.977 ms |    1039.83 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinMerge               |      38.674 ms |   0.3117 ms |   0.1855 ms |      38.450 ms |    1039.82 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinMerge               |      38.840 ms |   2.4672 ms |   1.4682 ms |      37.761 ms |    1302.54 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimized           |      46.157 ms |   2.5981 ms |   1.3589 ms |      45.351 ms |    1040.70 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinFast                |      48.010 ms |   0.6841 ms |   0.3578 ms |      47.379 ms |    1039.88 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimizedFilter     |      56.349 ms |   0.6592 ms |   0.4360 ms |      55.681 ms |    1039.72 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinFastFilter          |      67.494 ms |   1.2583 ms |   0.6581 ms |      66.675 ms |    1040.04 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinOptimizedFilter     |      88.643 ms |   7.9821 ms |   5.2796 ms |      81.529 ms |  554606.92 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinFastFilter          |      91.991 ms |   4.2082 ms |   2.5042 ms |      89.155 ms |  436103.86 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinFast                |     105.336 ms |   5.3784 ms |   3.2006 ms |     100.159 ms |  890044.85 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinFilter              |     132.600 ms |   1.0676 ms |   0.7061 ms |     131.494 ms |    1039.93 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinOptimized           |     140.208 ms |  10.5195 ms |   6.9580 ms |     133.675 ms | 1658459.70 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinFilter              |     160.529 ms |   6.2127 ms |   4.1093 ms |     154.391 ms |  530663.86 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      Legacy                 |     582.490 ms |   3.3673 ms |   2.0038 ms |     577.950 ms |    1102.88 KB |
```

* коммит: доработка поискового движка (оптимизация памяти выделяемой токенайзером)
```
| Type                        | Method             | SearchType                  | Mean           | Error       | StdDev      | Min            | Allocated     |
|---------------------------- |------------------- |---------------------------- |---------------:|------------:|------------:|---------------:|--------------:|
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterLs |      3.2130 ms |   0.0219 ms |   0.0145 ms |      3.1957 ms |    1166.55 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterHs |      3.2185 ms |   0.0089 ms |   0.0047 ms |      3.2118 ms |     566.65 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterHs |      3.2497 ms |   0.0047 ms |   0.0031 ms |      3.2422 ms |    1166.54 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterLs |      3.2557 ms |   0.0077 ms |   0.0051 ms |      3.2473 ms |     566.65 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterBs |      3.2853 ms |   0.0047 ms |   0.0031 ms |      3.2780 ms |     566.65 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterBs |      3.4151 ms |   0.0068 ms |   0.0041 ms |      3.4085 ms |    1166.54 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinOffsetFilter        |      7.2540 ms |   0.0156 ms |   0.0093 ms |      7.2395 ms |    1746.45 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinOffsetFilter        |      7.4855 ms |   0.0197 ms |   0.0130 ms |      7.4664 ms |    1367.20 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinOffset              |     28.0422 ms |   1.6692 ms |   0.8730 ms |     27.1984 ms |     536.12 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinOffset              |     28.9564 ms |   1.0301 ms |   0.6130 ms |     27.6045 ms |     914.91 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectLs       |    115.9256 ms |   3.3103 ms |   1.7314 ms |    112.0490 ms |    1342.36 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectLs       |    117.3488 ms |   1.4106 ms |   0.8394 ms |    116.2434 ms |     566.90 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectBs       |    118.4203 ms |   0.9173 ms |   0.6068 ms |    117.8525 ms |     566.78 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectBs       |    121.9980 ms |   1.8102 ms |   1.0772 ms |    120.7502 ms |    1341.34 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectHs       |    161.7408 ms |   4.0035 ms |   2.6480 ms |    157.6951 ms |    1341.14 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectHs       |    162.4082 ms |   2.3378 ms |   1.5463 ms |    159.9098 ms |     566.75 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      Legacy                 |    667.4117 ms |   1.5931 ms |   1.0537 ms |    666.0072 ms |     600.39 KB |
|                             |                    |                             |                |             |             |                |               |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinMergeFilter         |     34.4818 ms |   0.3667 ms |   0.2425 ms |     34.1401 ms |     886.88 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinMergeFilter         |     35.0756 ms |   0.2609 ms |   0.1726 ms |     34.6970 ms |     597.07 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinMerge               |     37.9431 ms |   1.6736 ms |   1.1070 ms |     37.0530 ms |     859.74 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinMerge               |     38.6484 ms |   0.2891 ms |   0.1912 ms |     38.3951 ms |     597.03 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimized           |     46.6805 ms |   2.0536 ms |   1.2221 ms |     45.5676 ms |     597.70 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinFast                |     48.2951 ms |   0.5349 ms |   0.2798 ms |     47.7070 ms |     597.20 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimizedFilter     |     56.6633 ms |   0.4726 ms |   0.2812 ms |     56.3601 ms |     597.25 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinFastFilter          |     65.6320 ms |   0.8317 ms |   0.5501 ms |     64.9043 ms |     597.15 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinOptimizedFilter     |     89.0048 ms |   7.3561 ms |   4.8656 ms |     80.3762 ms |  554140.85 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinFastFilter          |     98.8125 ms |   9.5203 ms |   6.2971 ms |     90.2752 ms |  435704.35 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinFast                |    110.7752 ms |  11.2224 ms |   7.4230 ms |    100.1082 ms |  889619.40 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinFilter              |    132.2054 ms |   0.8615 ms |   0.5127 ms |    131.5000 ms |     597.36 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinOptimized           |    141.3895 ms |  10.3032 ms |   6.8149 ms |    133.1064 ms | 1658069.20 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinFilter              |    158.9878 ms |   5.0541 ms |   3.0076 ms |    153.7750 ms |  530267.39 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      Legacy                 |    584.9109 ms |   4.7687 ms |   3.1542 ms |    578.5789 ms |     662.55 KB |
```

* коммит: доработка поискового движка (добавлено хранение данных документа в одном массиве GinArrayDirect ддя reduced)
```
| Type                        | Method             | SearchType                  | Mean           | Error       | StdDev      | Median         | Min            | Allocated     |
|---------------------------- |------------------- |---------------------------- |---------------:|------------:|------------:|---------------:|---------------:|--------------:|
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterHs |      3.1822 ms |   0.0045 ms |   0.0030 ms |      3.1825 ms |      3.1773 ms |    1135.30 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterLs |      3.2358 ms |   0.0049 ms |   0.0032 ms |      3.2357 ms |      3.2315 ms |    1135.29 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterHs |      3.2419 ms |   0.0085 ms |   0.0056 ms |      3.2439 ms |      3.2321 ms |     535.40 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterLs |      3.3064 ms |   0.0113 ms |   0.0075 ms |      3.3068 ms |      3.2925 ms |     535.40 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterBs |      3.3312 ms |   0.0080 ms |   0.0053 ms |      3.3329 ms |      3.3224 ms |     535.40 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterBs |      3.3948 ms |   0.0132 ms |   0.0069 ms |      3.3937 ms |      3.3868 ms |    1135.29 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinOffsetFilter        |      7.2546 ms |   0.0079 ms |   0.0047 ms |      7.2555 ms |      7.2442 ms |    1715.20 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinOffsetFilter        |      7.7108 ms |   0.3853 ms |   0.2015 ms |      7.6372 ms |      7.4861 ms |    1336.17 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinOffset              |     27.1416 ms |   0.4454 ms |   0.2330 ms |     27.0526 ms |     26.8744 ms |     885.53 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinOffset              |     27.6813 ms |   1.6318 ms |   0.8535 ms |     27.4941 ms |     26.9996 ms |     504.26 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectLs       |    118.7632 ms |   3.3814 ms |   1.7685 ms |    118.1356 ms |    117.0133 ms |    1311.36 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectLs       |    119.4824 ms |   0.9265 ms |   0.6128 ms |    119.4477 ms |    118.3478 ms |     536.28 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectBs       |    123.0637 ms |   1.8317 ms |   1.2116 ms |    122.9129 ms |    121.2797 ms |     535.49 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectBs       |    125.9911 ms |   1.5176 ms |   1.0038 ms |    125.8980 ms |    124.2712 ms |    1310.01 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectHs       |    168.9878 ms |   2.5021 ms |   1.4889 ms |    169.3538 ms |    167.0332 ms |    1310.89 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectHs       |    169.5356 ms |   1.9607 ms |   1.2969 ms |    169.6885 ms |    167.6743 ms |     535.41 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      Legacy                 |    698.0740 ms |   1.0832 ms |   0.7165 ms |    698.1173 ms |    697.0425 ms |     567.04 KB |
|                             |                    |                             |                |             |             |                |                |               |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinArrayMergeFilter    |     19.5180 ms |   0.1895 ms |   0.1254 ms |     19.5602 ms |     19.2731 ms |     565.78 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinArrayMergeFilter    |     20.3329 ms |   0.2613 ms |   0.1555 ms |     20.2769 ms |     20.0924 ms |     911.10 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinArrayDirectFilterLs |     24.1599 ms |   0.1991 ms |   0.1317 ms |     24.1451 ms |     23.9656 ms |     826.47 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinArrayDirect         |     25.7987 ms |   0.0726 ms |   0.0480 ms |     25.7855 ms |     25.7513 ms |     565.79 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinArrayDirect         |     26.7320 ms |   0.1086 ms |   0.0718 ms |     26.7262 ms |     26.6174 ms |     884.03 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinArrayDirectFilterBs |     30.5363 ms |   0.4132 ms |   0.2459 ms |     30.4788 ms |     30.1477 ms |     826.25 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinArrayDirectFilterBs |     30.6855 ms |   0.2963 ms |   0.1550 ms |     30.7151 ms |     30.4314 ms |     566.09 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinArrayDirectFilterLs |     32.5766 ms |  28.7724 ms |  17.1220 ms |     24.1429 ms |     23.8302 ms |     566.18 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinMergeFilter         |     35.7310 ms |   0.2056 ms |   0.1075 ms |     35.6872 ms |     35.6028 ms |     855.29 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinMergeFilter         |     37.1804 ms |   0.3295 ms |   0.2179 ms |     37.1310 ms |     36.9207 ms |     565.82 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinArrayDirectFilterHs |     37.8850 ms |   0.4749 ms |   0.3141 ms |     37.8572 ms |     37.4559 ms |     826.00 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinArrayDirectFilterHs |     38.3541 ms |   0.6532 ms |   0.4320 ms |     38.2685 ms |     37.8699 ms |     565.80 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinMerge               |     39.7844 ms |   0.3544 ms |   0.2109 ms |     39.7514 ms |     39.5754 ms |     565.80 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinMerge               |     41.1361 ms |   1.3935 ms |   0.9217 ms |     41.3176 ms |     39.6032 ms |     828.38 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimized           |     49.4308 ms |   4.0441 ms |   2.6749 ms |     47.9253 ms |     47.6093 ms |     566.16 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinFast                |     49.8705 ms |   0.4387 ms |   0.2611 ms |     49.9811 ms |     49.3812 ms |     566.11 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimizedFilter     |     54.2917 ms |   0.3692 ms |   0.2197 ms |     54.3255 ms |     53.8865 ms |     566.00 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinFastFilter          |     65.8236 ms |   0.6062 ms |   0.3608 ms |     65.7348 ms |     65.3676 ms |     565.82 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinOptimizedFilter     |     77.5046 ms |   5.1685 ms |   3.0757 ms |     76.6502 ms |     74.3638 ms |  394381.54 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinFastFilter          |     90.1880 ms |   4.1210 ms |   2.1554 ms |     89.4801 ms |     87.8441 ms |  435610.11 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinFast                |    109.5906 ms |   3.7172 ms |   2.4587 ms |    108.4731 ms |    106.1198 ms |  889547.16 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinOptimized           |    156.0301 ms |  10.0713 ms |   6.6615 ms |    155.7808 ms |    147.1707 ms | 1658025.95 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      Legacy                 |    609.5756 ms |   5.2257 ms |   3.4565 ms |    608.9131 ms |    605.2619 ms |     628.66 KB |
```

* коммит: доработка поискового движка (оптимизация индексов, добавлено партицирование индексов)
```
| Type                        | Method             | SearchType                  | Mean           | Error       | StdDev      | Median         | Min            | Allocated    |
|---------------------------- |------------------- |---------------------------- |---------------:|------------:|------------:|---------------:|---------------:|-------------:|
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterHs |      3.1818 ms |   0.0058 ms |   0.0034 ms |      3.1811 ms |      3.1769 ms |   1135.30 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterLs |      3.1973 ms |   0.0057 ms |   0.0038 ms |      3.1985 ms |      3.1884 ms |   1135.30 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterHs |      3.2208 ms |   0.0069 ms |   0.0036 ms |      3.2205 ms |      3.2150 ms |    535.40 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterBs |      3.3104 ms |   0.0080 ms |   0.0053 ms |      3.3098 ms |      3.3035 ms |    535.40 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterBs |      3.3183 ms |   0.0063 ms |   0.0042 ms |      3.3184 ms |      3.3115 ms |   1135.29 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterLs |      3.3395 ms |   0.0232 ms |   0.0153 ms |      3.3371 ms |      3.3229 ms |    535.40 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinOffsetFilter        |      5.7210 ms |   0.0088 ms |   0.0058 ms |      5.7205 ms |      5.7117 ms |   1715.21 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinOffsetFilter        |      5.8148 ms |   0.0160 ms |   0.0106 ms |      5.8143 ms |      5.7943 ms |   1335.95 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinOffset              |     29.9815 ms |   0.7341 ms |   0.3840 ms |     29.8976 ms |     29.5980 ms |    504.47 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinOffset              |     35.2033 ms |   0.1127 ms |   0.0745 ms |     35.2204 ms |     35.0585 ms |    883.41 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectLs       |    116.4053 ms |   1.2765 ms |   0.8444 ms |    116.3371 ms |    115.2819 ms |   1309.93 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectLs       |    118.0314 ms |   3.3387 ms |   1.9868 ms |    118.6077 ms |    113.6138 ms |    535.79 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectBs       |    122.4518 ms |   2.7382 ms |   1.6294 ms |    122.8481 ms |    120.2931 ms |    535.79 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectBs       |    123.5495 ms |   1.2823 ms |   0.8482 ms |    123.5528 ms |    122.1431 ms |   1309.83 KB |
| MtQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectHs       |    168.4055 ms |   1.7131 ms |   1.0195 ms |    168.6413 ms |    166.1041 ms |    535.53 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectHs       |    169.6583 ms |   1.5720 ms |   1.0397 ms |    169.2698 ms |    168.4119 ms |   1309.98 KB |
| MtQueryBenchmarkExtended    | QueryExtended      |      Legacy                 |    696.8534 ms |   2.5940 ms |   1.7158 ms |    696.6464 ms |    694.0079 ms |    566.76 KB |
|                             |                    |                             |                |             |             |                |                |              |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinArrayMergeFilter    |     17.0724 ms |   0.3112 ms |   0.2059 ms |     17.1019 ms |     16.7921 ms |    565.79 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinArrayDirectFilterLs |     23.5179 ms |   0.1069 ms |   0.0636 ms |     23.5445 ms |     23.3826 ms |    565.78 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinArrayDirect         |     24.9226 ms |   0.1499 ms |   0.0784 ms |     24.9328 ms |     24.8132 ms |    884.02 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinArrayDirect         |     25.8574 ms |   0.0802 ms |   0.0477 ms |     25.8520 ms |     25.7771 ms |    565.78 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinArrayDirectFilterLs |     26.3546 ms |   5.2055 ms |   3.0977 ms |     25.0271 ms |     24.3000 ms |    827.27 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinArrayDirectFilterBs |     29.8892 ms |   0.1114 ms |   0.0663 ms |     29.9051 ms |     29.7562 ms |    826.00 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinArrayDirectFilterBs |     30.0114 ms |   0.0736 ms |   0.0438 ms |     30.0240 ms |     29.9343 ms |    565.78 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinArrayMergeFilter    |     31.7504 ms |  35.4008 ms |  23.4155 ms |     17.2571 ms |     17.1013 ms |    911.30 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      GinArrayDirectFilterHs |     37.5106 ms |   0.6275 ms |   0.4150 ms |     37.4004 ms |     37.0398 ms |    826.01 KB |
| MtQueryBenchmarkReduced     | QueryReduced       | Pool GinArrayDirectFilterHs |     37.5564 ms |   0.7724 ms |   0.5109 ms |     37.6287 ms |     36.9134 ms |    565.80 KB |
| MtQueryBenchmarkReduced     | QueryReduced       |      Legacy                 |    610.3681 ms |   3.8206 ms |   2.2736 ms |    611.3470 ms |    606.6573 ms |    629.87 KB |
```

* коммит: ...
