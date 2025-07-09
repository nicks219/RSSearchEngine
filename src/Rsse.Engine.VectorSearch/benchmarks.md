## Разработка и оптимизация алгортимов поиска сервиса RSSE

### Идеи и варианты реализации: **[Viktor Loshmanov](https://github.com/ViktorLoshmanov)**

---
### Результаты бенчмарков различных алгоритмов

* коммит: доработка поискового движка (extended фильтр релевантности)
```
| Type                        | Method             | SearchType         | Mean          | Error      | StdDev     | Min           | Gen0      | Gen1      | Gen2   | Allocated    |
|---------------------------- |------------------- |------------------- |--------------:|-----------:|-----------:|--------------:|----------:|----------:|-------:|-------------:|
| DuplicatesBenchmarkExtended | DuplicatesExtended | GinSimpleFilter    |    71.4985 ms |  1.1053 ms |  0.7311 ms |    70.4870 ms |  625.0000 |         - |      - |  43208.91 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | GinOptimizedFilter |   285.6757 ms |  3.6994 ms |  2.2014 ms |   282.5283 ms |  500.0000 |         - |      - |  44179.80 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Legacy             |   770.6592 ms |  3.9858 ms |  2.3719 ms |   766.9426 ms |         - |         - |      - |   2061.07 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | GinSimple          |   901.6697 ms | 17.2035 ms | 11.3791 ms |   886.5034 ms |         - |         - |      - |   2061.07 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | GinOptimized       |   956.2511 ms |  4.3860 ms |  2.6101 ms |   952.0236 ms |         - |         - |      - |  54485.88 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | GinFastFilter      | 1,372.7167 ms |  6.6229 ms |  4.3807 ms | 1,365.6579 ms | 9000.0000 | 2000.0000 |      - | 589826.16 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | GinFast            | 2,076.5755 ms |  6.0991 ms |  3.1899 ms | 2,070.5198 ms | 8000.0000 | 2000.0000 |      - | 548678.03 KB |
|                             |                    |                    |               |            |            |               |           |           |        |              |
| QueryBenchmarkExtended      | QueryExtended      | GinOptimizedFilter |     0.1087 ms |  0.0005 ms |  0.0003 ms |     0.1083 ms |    0.3662 |         - |      - |     25.90 KB |
| QueryBenchmarkExtended      | QueryExtended      | GinFastFilter      |     0.2620 ms |  0.0112 ms |  0.0074 ms |     0.2562 ms |    1.9531 |    1.9531 | 1.9531 |    283.69 KB |
| QueryBenchmarkExtended      | QueryExtended      | GinSimpleFilter    |     0.6934 ms |  0.0146 ms |  0.0096 ms |     0.6709 ms |         - |         - |      - |     13.31 KB |
| QueryBenchmarkExtended      | QueryExtended      | GinFast            |     1.2429 ms |  0.0390 ms |  0.0258 ms |     1.1998 ms |    1.9531 |    1.9531 | 1.9531 |    270.70 KB |
| QueryBenchmarkExtended      | QueryExtended      | GinOptimized       |     1.6833 ms |  0.0399 ms |  0.0264 ms |     1.6249 ms |    1.9531 |         - |      - |    252.45 KB |
| QueryBenchmarkExtended      | QueryExtended      | GinSimple          |     4.6114 ms |  0.1566 ms |  0.1036 ms |     4.4635 ms |         - |         - |      - |      0.30 KB |
| QueryBenchmarkExtended      | QueryExtended      | Legacy             |    12.1972 ms |  0.3300 ms |  0.2183 ms |    11.8357 ms |         - |         - |      - |      0.31 KB |
|                             |                    |                    |               |            |            |               |           |           |        |              |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | GinFast            |   113.7162 ms |  1.1770 ms |  0.7785 ms |   112.4263 ms | 1000.0000 |         - |      - |  70059.50 KB |
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
```

* коммит: доработка поискового движка (оптимизация алгоритмов поиска)
```
| Type                        | Method             | SearchType              | Mean          | Error      | StdDev     | Min           | Gen0      | Gen1   | Gen2   | Allocated    |
|---------------------------- |------------------- |------------------------ |--------------:|-----------:|-----------:|--------------:|----------:|-------:|-------:|-------------:|
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFilter          |    58.0136 ms |  0.6641 ms |  0.3473 ms |    57.5496 ms |  555.5556 |      - |      - |  43150.90 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFastFilter      |   210.9734 ms |  2.2526 ms |  1.4899 ms |   208.9451 ms | 1333.3333 |      - |      - |  89389.42 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFastFilter      |   223.9624 ms |  3.5025 ms |  2.3167 ms |   220.9855 ms | 2000.0000 |      - |      - | 141872.45 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      Legacy             |   758.6360 ms |  4.1497 ms |  2.7448 ms |   753.6148 ms |         - |      - |      - |   2061.12 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinSimple          |   888.7724 ms |  4.0698 ms |  2.4219 ms |   885.5621 ms |         - |      - |      - |   2061.40 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOptimized       |   940.7147 ms |  5.1882 ms |  3.0874 ms |   934.9123 ms |         - |      - |      - |  54485.55 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFast            |   953.2402 ms |  8.7062 ms |  5.7586 ms |   946.3294 ms |         - |      - |      - |  48241.55 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFast            |   974.6816 ms |  4.5483 ms |  2.3789 ms |   969.2262 ms | 1000.0000 |      - |      - | 100724.18 KB |
|                             |                    |                         |               |            |            |               |           |        |        |              |
| QueryBenchmarkExtended      | QueryExtended      |      GinFilter          |     0.0291 ms |  0.0003 ms |  0.0001 ms |     0.0289 ms |    0.1831 |      - |      - |     13.25 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFastFilter      |     0.3662 ms |  0.0075 ms |  0.0050 ms |     0.3580 ms |    1.9531 | 0.9766 | 0.9766 |    398.87 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFastFilter      |     0.4574 ms |  0.0066 ms |  0.0044 ms |     0.4519 ms |    9.2773 | 3.4180 | 2.4414 |    651.08 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFast            |     1.3596 ms |  0.0294 ms |  0.0194 ms |     1.3224 ms |    3.9063 |      - |      - |    385.86 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFast            |     1.4354 ms |  0.0567 ms |  0.0337 ms |     1.3794 ms |    1.9531 | 1.9531 | 1.9531 |    638.07 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOptimized       |     1.6600 ms |  0.0126 ms |  0.0066 ms |     1.6460 ms |    1.9531 |      - |      - |    252.45 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinSimple          |     4.6497 ms |  0.0696 ms |  0.0460 ms |     4.5856 ms |         - |      - |      - |      0.30 KB |
| QueryBenchmarkExtended      | QueryExtended      |      Legacy             |    12.4887 ms |  0.4135 ms |  0.2460 ms |    12.1844 ms |         - |      - |      - |      0.31 KB |
|                             |                    |                         |               |            |            |               |           |        |        |              |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFast            |    98.1245 ms |  1.3516 ms |  0.8940 ms |    96.7701 ms |         - |      - |      - |   7589.35 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimized       |   106.4066 ms |  2.3346 ms |  1.5442 ms |   104.7496 ms | 1000.0000 |      - |      - |  70059.50 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFast            |   111.4918 ms |  1.9308 ms |  1.1490 ms |   110.2242 ms | 1000.0000 |      - |      - |  72674.81 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimizedFilter |   114.7251 ms |  1.2970 ms |  0.6783 ms |   113.5562 ms |  400.0000 |      - |      - |  35538.01 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFastFilter      |   126.7914 ms |  2.8487 ms |  1.6952 ms |   124.6739 ms |         - |      - |      - |  36648.05 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFastFilter      |   135.2468 ms |  2.3841 ms |  1.5769 ms |   133.4772 ms |  500.0000 |      - |      - |  47845.91 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFilter          |   238.2699 ms |  2.5054 ms |  1.6572 ms |   235.9145 ms |  333.3333 |      - |      - |  33280.12 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      Legacy             |   814.2701 ms |  8.4296 ms |  5.5757 ms |   805.6685 ms |         - |      - |      - |   4279.66 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinSimple          | 1,914.9344 ms | 19.3091 ms | 11.4905 ms | 1,886.8618 ms |         - |      - |      - |   4279.62 KB |
|                             |                    |                         |               |            |            |               |           |        |        |              |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFast            |     0.1450 ms |  0.0021 ms |  0.0014 ms |     0.1415 ms |         - |      - |      - |     10.96 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFast            |     0.2799 ms |  0.0087 ms |  0.0057 ms |     0.2699 ms |    1.9531 | 1.4648 | 1.4648 |    326.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimizedFilter |     0.2818 ms |  0.0059 ms |  0.0039 ms |     0.2772 ms |    4.3945 | 1.4648 | 1.4648 |    326.04 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFastFilter      |     0.3282 ms |  0.0100 ms |  0.0066 ms |     0.3203 ms |    3.4180 | 0.9766 | 0.9766 |    263.58 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFastFilter      |     0.4838 ms |  0.0203 ms |  0.0134 ms |     0.4618 ms |    3.9063 | 2.9297 | 2.9297 |    578.65 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimized       |     0.8817 ms |  0.0255 ms |  0.0169 ms |     0.8619 ms |    8.7891 | 4.8828 | 4.8828 |    667.75 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFilter          |     1.2452 ms |  0.0285 ms |  0.0188 ms |     1.2126 ms |    1.9531 |      - |      - |    263.07 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinSimple          |     4.6290 ms |  0.1139 ms |  0.0754 ms |     4.4503 ms |         - |      - |      - |     10.52 KB |
| QueryBenchmarkReduced       | QueryReduced       |      Legacy             |    12.4378 ms |  0.3743 ms |  0.2476 ms |    11.9985 ms |         - |      - |      - |     10.52 KB |
```

* коммит: доработка поискового движка (оптимизация extended алгоритмов поиска)
```
| Type                        | Method             | SearchType              | Mean          | Error      | StdDev    | Min           | Gen0      | Gen1   | Gen2   | Allocated   |
|---------------------------- |------------------- |------------------------ |--------------:|-----------:|----------:|--------------:|----------:|-------:|-------:|------------:|
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFilter          |    52.3181 ms |  1.0682 ms | 0.7065 ms |    51.4560 ms |  300.0000 |      - |      - | 24321.98 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFilter          |    53.0141 ms |  0.8931 ms | 0.5907 ms |    52.1514 ms |  400.0000 |      - |      - | 28199.95 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFastFilter      |    54.0969 ms |  1.2129 ms | 0.7218 ms |    53.4739 ms |  300.0000 |      - |      - | 24322.02 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFastFilter      |    55.5764 ms |  0.7482 ms | 0.4452 ms |    54.7822 ms |  400.0000 |      - |      - | 29353.09 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      Legacy             |   743.4035 ms |  5.8398 ms | 3.8627 ms |   736.9648 ms |         - |      - |      - |  2061.07 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFast            |   790.5811 ms |  8.4609 ms | 5.0350 ms |   778.1670 ms |         - |      - |      - |  2003.01 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOptimized       |   814.6289 ms |  5.8258 ms | 3.4668 ms |   806.7913 ms |         - |      - |      - |  2003.05 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFast            |   831.7705 ms | 10.7510 ms | 7.1111 ms |   824.8536 ms |         - |      - |      - | 57833.90 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOptimized       |   840.9647 ms |  3.5371 ms | 1.8500 ms |   836.7889 ms |         - |      - |      - | 57834.13 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinSimple          |   891.8549 ms | 13.5810 ms | 7.1031 ms |   875.1972 ms |         - |      - |      - |  2060.84 KB |
|                             |                    |                         |               |            |           |               |           |        |        |             |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFilter          |     0.0264 ms |  0.0002 ms | 0.0001 ms |     0.0262 ms |         - |      - |      - |     0.77 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFastFilter      |     0.0284 ms |  0.0003 ms | 0.0002 ms |     0.0281 ms |         - |      - |      - |     0.77 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFilter          |     0.0287 ms |  0.0004 ms | 0.0002 ms |     0.0283 ms |    0.1831 |      - |      - |    13.50 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFastFilter      |     0.0342 ms |  0.0005 ms | 0.0003 ms |     0.0338 ms |    0.0610 |      - |      - |    26.31 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFast            |     1.2423 ms |  0.0583 ms | 0.0386 ms |     1.1712 ms |         - |      - |      - |     0.24 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFast            |     1.3509 ms |  0.0561 ms | 0.0371 ms |     1.2807 ms |    1.9531 |      - |      - |   252.62 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOptimized       |     1.5984 ms |  0.0441 ms | 0.0292 ms |     1.5455 ms |         - |      - |      - |     0.24 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOptimized       |     1.6579 ms |  0.0580 ms | 0.0383 ms |     1.5984 ms |    1.9531 |      - |      - |   252.62 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinSimple          |     4.7379 ms |  0.0710 ms | 0.0422 ms |     4.6867 ms |         - |      - |      - |     0.30 KB |
| QueryBenchmarkExtended      | QueryExtended      |      Legacy             |    12.3266 ms |  0.2384 ms | 0.1418 ms |    11.9979 ms |         - |      - |      - |     0.31 KB |
|                             |                    |                         |               |            |           |               |           |        |        |             |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFast            |    93.8236 ms |  1.0518 ms | 0.6259 ms |    93.1909 ms |         - |      - |      - |  7589.39 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimized       |   106.8661 ms |  3.0085 ms | 1.9899 ms |   104.6667 ms | 1000.0000 |      - |      - | 70059.60 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFast            |   111.7446 ms |  3.1447 ms | 2.0800 ms |   109.3015 ms | 1000.0000 |      - |      - | 72674.32 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimizedFilter |   113.0141 ms |  0.9451 ms | 0.5624 ms |   112.1732 ms |  400.0000 |      - |      - | 35537.95 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFastFilter      |   126.2593 ms |  1.6596 ms | 1.0978 ms |   124.8837 ms |  500.0000 |      - |      - | 36647.80 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFastFilter      |   136.2970 ms |  6.7367 ms | 4.4559 ms |   132.7280 ms |         - |      - |      - | 47846.29 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFilter          |   234.7708 ms |  1.5049 ms | 0.8956 ms |   233.3047 ms |  333.3333 |      - |      - | 33280.22 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      Legacy             |   788.8439 ms | 11.6977 ms | 6.1181 ms |   773.7772 ms |         - |      - |      - |  4279.66 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinSimple          | 1,891.7832 ms |  5.5725 ms | 3.6859 ms | 1,886.3156 ms |         - |      - |      - |  4279.66 KB |
|                             |                    |                         |               |            |           |               |           |        |        |             |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFast            |     0.1460 ms |  0.0025 ms | 0.0015 ms |     0.1444 ms |         - |      - |      - |    10.96 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFast            |     0.2696 ms |  0.0058 ms | 0.0039 ms |     0.2628 ms |    4.3945 | 1.4648 | 1.4648 |   326.04 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimizedFilter |     0.2919 ms |  0.0059 ms | 0.0039 ms |     0.2839 ms |    1.9531 | 1.4648 | 1.4648 |   326.04 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFastFilter      |     0.3267 ms |  0.0082 ms | 0.0054 ms |     0.3189 ms |    3.4180 | 0.9766 | 0.9766 |   263.58 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFastFilter      |     0.4892 ms |  0.0126 ms | 0.0083 ms |     0.4746 ms |    3.9063 | 2.9297 | 2.9297 |   578.66 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimized       |     0.9022 ms |  0.0273 ms | 0.0180 ms |     0.8763 ms |    8.7891 | 4.8828 | 4.8828 |   667.76 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFilter          |     1.2620 ms |  0.0362 ms | 0.0216 ms |     1.2151 ms |         - |      - |      - |   263.07 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinSimple          |     4.8064 ms |  0.1377 ms | 0.0911 ms |     4.5985 ms |         - |      - |      - |    10.52 KB |
| QueryBenchmarkReduced       | QueryReduced       |      Legacy             |    12.4212 ms |  0.2265 ms | 0.1185 ms |    12.2485 ms |         - |      - |      - |    10.52 KB |
```

* коммит: доработка поискового движка (оптимизация reduced алгоритмов поиска)
```
| Type                        | Method             | SearchType              | Mean          | Error      | StdDev    | Min           | Gen0      | Gen1   | Gen2   | Allocated   |
|---------------------------- |------------------- |------------------------ |--------------:|-----------:|----------:|--------------:|----------:|-------:|-------:|------------:|
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFilter          |    52.7658 ms |  1.1992 ms | 0.7932 ms |    51.8071 ms |         - |      - |      - |  2060.72 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFilter          |    53.9538 ms |  1.4292 ms | 0.9453 ms |    52.7247 ms |  100.0000 |      - |      - |  8915.78 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFastFilter      |    55.9458 ms |  1.7184 ms | 1.1366 ms |    54.9254 ms |         - |      - |      - |  2060.69 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFastFilter      |    57.5734 ms |  1.5186 ms | 1.0045 ms |    56.4048 ms |  111.1111 |      - |      - | 10181.20 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      Legacy             |   762.2071 ms |  4.5250 ms | 2.9930 ms |   757.2818 ms |         - |      - |      - |  2060.79 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFast            |   795.9835 ms |  9.9638 ms | 6.5904 ms |   781.0214 ms |         - |      - |      - |  2002.73 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFast            |   812.1129 ms | 10.5349 ms | 6.9682 ms |   804.2781 ms |         - |      - |      - | 57834.46 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOptimized       |   827.9916 ms |  8.4182 ms | 5.5681 ms |   820.5004 ms |         - |      - |      - |  2003.05 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOptimized       |   847.0659 ms | 10.1877 ms | 6.7385 ms |   839.0349 ms |         - |      - |      - | 57834.13 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinSimple          |   906.4436 ms |  6.2644 ms | 4.1435 ms |   899.6006 ms |         - |      - |      - |  2061.12 KB |
|                             |                    |                         |               |            |           |               |           |        |        |             |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFilter          |     0.0272 ms |  0.0002 ms | 0.0001 ms |     0.0270 ms |         - |      - |      - |     0.30 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFilter          |     0.0295 ms |  0.0003 ms | 0.0002 ms |     0.0291 ms |    0.1831 |      - |      - |    13.13 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFastFilter      |     0.0305 ms |  0.0005 ms | 0.0003 ms |     0.0301 ms |         - |      - |      - |     0.30 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFastFilter      |     0.0359 ms |  0.0003 ms | 0.0002 ms |     0.0356 ms |    0.0610 |      - |      - |    25.94 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFast            |     1.2477 ms |  0.0574 ms | 0.0379 ms |     1.1951 ms |         - |      - |      - |     0.24 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFast            |     1.3495 ms |  0.0632 ms | 0.0418 ms |     1.2761 ms |    1.9531 |      - |      - |   252.62 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOptimized       |     1.6246 ms |  0.0441 ms | 0.0231 ms |     1.5862 ms |         - |      - |      - |     0.24 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOptimized       |     1.7001 ms |  0.0783 ms | 0.0466 ms |     1.6110 ms |         - |      - |      - |   252.62 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinSimple          |     4.8155 ms |  0.0582 ms | 0.0347 ms |     4.7629 ms |         - |      - |      - |     0.31 KB |
| QueryBenchmarkExtended      | QueryExtended      |      Legacy             |    12.6456 ms |  0.4127 ms | 0.2729 ms |    12.3170 ms |         - |      - |      - |     0.31 KB |
|                             |                    |                         |               |            |           |               |           |        |        |             |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimizedFilter |    96.2716 ms |  0.8991 ms | 0.5351 ms |    95.6707 ms |         - |      - |      - |  4221.23 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimized       |    97.3839 ms |  1.1365 ms | 0.5944 ms |    96.2177 ms |         - |      - |      - |  4221.17 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFastFilter      |    97.6516 ms |  1.4970 ms | 0.9902 ms |    96.2555 ms |         - |      - |      - |  4221.23 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFast            |   103.5288 ms |  2.0892 ms | 1.3819 ms |   102.1559 ms |         - |      - |      - |  4221.24 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimized       |   112.5586 ms |  3.1867 ms | 1.8963 ms |   110.0847 ms | 1000.0000 |      - |      - | 70059.81 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimizedFilter |   117.8411 ms |  1.3967 ms | 0.8312 ms |   116.6529 ms |  200.0000 |      - |      - | 17172.22 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFast            |   121.5838 ms |  1.6906 ms | 1.0061 ms |   119.9293 ms | 1000.0000 |      - |      - | 70864.50 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFastFilter      |   122.7481 ms |  1.8336 ms | 1.0911 ms |   121.5755 ms |  400.0000 |      - |      - | 26163.05 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFilter          |   240.5367 ms |  1.9746 ms | 1.3061 ms |   238.8860 ms |         - |      - |      - | 17172.21 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFilter          |   241.6356 ms |  1.8434 ms | 1.2193 ms |   239.3170 ms |         - |      - |      - |  4221.29 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      Legacy             |   815.9010 ms |  7.4357 ms | 4.9183 ms |   808.3336 ms |         - |      - |      - |  4279.66 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinSimple          | 1,933.4320 ms |  8.7708 ms | 5.8014 ms | 1,922.4587 ms |         - |      - |      - |  4279.34 KB |
|                             |                    |                         |               |            |           |               |           |        |        |             |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFast            |     0.1874 ms |  0.0021 ms | 0.0014 ms |     0.1860 ms |         - |      - |      - |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimizedFilter |     0.1901 ms |  0.0018 ms | 0.0011 ms |     0.1882 ms |         - |      - |      - |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFastFilter      |     0.2131 ms |  0.0018 ms | 0.0012 ms |     0.2110 ms |         - |      - |      - |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimized       |     0.2691 ms |  0.0053 ms | 0.0035 ms |     0.2620 ms |         - |      - |      - |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFast            |     0.3071 ms |  0.0064 ms | 0.0042 ms |     0.2991 ms |    1.9531 | 1.4648 | 1.4648 |   325.61 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimizedFilter |     0.3201 ms |  0.0048 ms | 0.0032 ms |     0.3158 ms |    1.9531 | 1.4648 | 1.4648 |   325.61 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFastFilter      |     0.3780 ms |  0.0042 ms | 0.0025 ms |     0.3735 ms |    2.4414 | 1.4648 | 1.4648 |   446.23 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimized       |     0.5486 ms |  0.0108 ms | 0.0071 ms |     0.5374 ms |    7.8125 | 3.9063 | 3.9063 |   667.75 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFilter          |     1.1900 ms |  0.0348 ms | 0.0230 ms |     1.1617 ms |         - |      - |      - |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFilter          |     1.2975 ms |  0.0593 ms | 0.0392 ms |     1.2190 ms |    3.9063 | 1.9531 | 1.9531 |   325.62 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinSimple          |     4.9112 ms |  0.2320 ms | 0.1534 ms |     4.7553 ms |         - |      - |      - |    10.52 KB |
| QueryBenchmarkReduced       | QueryReduced       |      Legacy             |    12.8079 ms |  0.4164 ms | 0.2754 ms |    12.3161 ms |         - |      - |      - |    10.52 KB |
```

* коммит: ...

