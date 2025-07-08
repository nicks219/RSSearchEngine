## Разработка и оптимизация алгортимов поиска сервиса RSSE

### Идеи и варианты реализации: **[Viktor Loshmanov](https://github.com/ViktorLoshmanov)**

---
### Результаты бенчмарков различных алгоритмов

* коммит: доработка поискового движка (extended фильтр релевантности)
```
| Type                        | Method             | SearchType         | Mean          | Error      | StdDev     | Min           | Gen0      | Gen1      | Gen2   | Allocated    |
|---------------------------- |------------------- |------------------- |--------------:|-----------:|-----------:|--------------:|----------:|----------:|-------:|-------------:|
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
|                             |                    |                    |               |            |            |               |           |           |        |              |
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
```

* коммит: доработка поискового движка (оптимизация алгоритмов поиска)
```
| Type                        | Method             | SearchType              | Mean          | Error      | StdDev     | Min           | Gen0      | Gen1   | Gen2   | Allocated    |
|---------------------------- |------------------- |------------------------ |--------------:|-----------:|-----------:|--------------:|----------:|-------:|-------:|-------------:|
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFilter          |    58.0136 ms |  0.6641 ms |  0.3473 ms |    57.5496 ms |  555.5556 |      - |      - |   43150.9 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFastFilter      |   210.9734 ms |  2.2526 ms |  1.4899 ms |   208.9451 ms | 1333.3333 |      - |      - |  89389.42 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFastFilter      |   223.9624 ms |  3.5025 ms |  2.3167 ms |   220.9855 ms | 2000.0000 |      - |      - | 141872.45 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      Legacy             |   758.6360 ms |  4.1497 ms |  2.7448 ms |   753.6148 ms |         - |      - |      - |   2061.12 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinSimple          |   888.7724 ms |  4.0698 ms |  2.4219 ms |   885.5621 ms |         - |      - |      - |    2061.4 KB |
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
| QueryBenchmarkExtended      | QueryExtended      |      GinSimple          |     4.6497 ms |  0.0696 ms |  0.0460 ms |     4.5856 ms |         - |      - |      - |       0.3 KB |
| QueryBenchmarkExtended      | QueryExtended      |      Legacy             |    12.4887 ms |  0.4135 ms |  0.2460 ms |    12.1844 ms |         - |      - |      - |      0.31 KB |
|                             |                    |                         |               |            |            |               |           |        |        |              |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFast            |    98.1245 ms |  1.3516 ms |  0.8940 ms |    96.7701 ms |         - |      - |      - |   7589.35 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimized       |   106.4066 ms |  2.3346 ms |  1.5442 ms |   104.7496 ms | 1000.0000 |      - |      - |   70059.5 KB |
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
| Type                        | Method             | SearchType              | Mean          | Error      | StdDev     | Min           | Gen0      | Gen1   | Gen2   | Allocated   |
|---------------------------- |------------------- |------------------------ |--------------:|-----------:|-----------:|--------------:|----------:|-------:|-------:|------------:|
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFilter          |    52.7754 ms |  0.2320 ms |  0.1535 ms |    52.5581 ms |  300.0000 |      - |      - | 24321.99 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFilter          |    53.5384 ms |  0.5183 ms |  0.3084 ms |    53.0434 ms |  400.0000 |      - |      - | 28199.91 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFastFilter      |    56.4211 ms |  1.0236 ms |  0.6770 ms |    55.6410 ms |  444.4444 |      - |      - | 29353.07 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFastFilter      |    57.8190 ms |  1.9987 ms |  1.3220 ms |    55.9187 ms |  333.3333 |      - |      - | 24321.95 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      Legacy             |   764.8586 ms |  8.5827 ms |  5.6769 ms |   757.8254 ms |         - |      - |      - |  2061.07 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFast            |   828.4584 ms | 16.7621 ms | 11.0871 ms |   812.1539 ms |         - |      - |      - |  2003.01 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFast            |   836.8824 ms |  6.3996 ms |  4.2329 ms |   831.9741 ms |         - |      - |      - | 57834.18 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOptimized       |   841.9473 ms | 18.6941 ms | 11.1246 ms |   825.1419 ms |         - |      - |      - |  2002.77 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOptimized       |   855.2005 ms |  5.7098 ms |  3.3978 ms |   851.6851 ms |         - |      - |      - | 57834.18 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinSimple          |   914.7369 ms |  4.3771 ms |  2.8952 ms |   909.9346 ms |         - |      - |      - |  2061.12 KB |
|                             |                    |                         |               |            |            |               |           |        |        |             |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFilter          |     0.0270 ms |  0.0001 ms |  0.0001 ms |     0.0268 ms |         - |      - |      - |     0.77 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFilter          |     0.0293 ms |  0.0004 ms |  0.0003 ms |     0.0290 ms |    0.1831 |      - |      - |     13.5 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFastFilter      |     0.0314 ms |  0.0004 ms |  0.0003 ms |     0.0309 ms |         - |      - |      - |     0.77 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFastFilter      |     0.0348 ms |  0.0004 ms |  0.0003 ms |     0.0344 ms |    0.0610 |      - |      - |    26.31 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFast            |     1.2550 ms |  0.0494 ms |  0.0327 ms |     1.1782 ms |         - |      - |      - |     0.24 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFast            |     1.3419 ms |  0.0259 ms |  0.0171 ms |     1.3178 ms |    1.9531 |      - |      - |   252.62 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOptimized       |     1.5765 ms |  0.0606 ms |  0.0401 ms |     1.5091 ms |         - |      - |      - |     0.24 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOptimized       |     1.6576 ms |  0.0631 ms |  0.0418 ms |     1.6027 ms |    1.9531 |      - |      - |   252.62 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinSimple          |     4.6963 ms |  0.1770 ms |  0.1053 ms |     4.5261 ms |         - |      - |      - |     0.31 KB |
| QueryBenchmarkExtended      | QueryExtended      |      Legacy             |    12.3177 ms |  0.2907 ms |  0.1923 ms |    11.9973 ms |         - |      - |      - |     0.31 KB |
|                             |                    |                         |               |            |            |               |           |        |        |             |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFast            |    97.0654 ms |  1.0136 ms |  0.6704 ms |    95.5678 ms |         - |      - |      - |  7589.35 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimized       |   107.5713 ms |  1.6747 ms |  0.9966 ms |   105.7552 ms | 1000.0000 |      - |      - |  70059.5 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFast            |   112.3430 ms |  1.7528 ms |  1.0431 ms |   110.8577 ms | 1000.0000 |      - |      - | 72674.32 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimizedFilter |   114.5791 ms |  1.4205 ms |  0.8453 ms |   113.7504 ms |  400.0000 |      - |      - | 35537.95 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFastFilter      |   128.3048 ms |  1.4167 ms |  0.9371 ms |   126.9792 ms |  500.0000 |      - |      - | 36647.73 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFastFilter      |   136.0427 ms |  2.2077 ms |  1.4602 ms |   134.3064 ms |  500.0000 |      - |      - | 47845.91 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFilter          |   240.1106 ms |  2.6364 ms |  1.5689 ms |   237.5109 ms |  333.3333 |      - |      - | 33280.22 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      Legacy             |   821.1654 ms |  6.2733 ms |  3.7331 ms |   814.6477 ms |         - |      - |      - |  4279.66 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinSimple          | 1,923.4519 ms | 16.9863 ms | 11.2354 ms | 1,901.9284 ms |         - |      - |      - |  4279.66 KB |
|                             |                    |                         |               |            |            |               |           |        |        |             |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFast            |     0.1482 ms |  0.0022 ms |  0.0014 ms |     0.1449 ms |         - |      - |      - |    10.96 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFast            |     0.2883 ms |  0.0077 ms |  0.0051 ms |     0.2804 ms |    1.9531 | 1.4648 | 1.4648 |   326.04 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimizedFilter |     0.2924 ms |  0.0091 ms |  0.0060 ms |     0.2823 ms |    4.3945 | 1.4648 | 1.4648 |   326.05 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFastFilter      |     0.3406 ms |  0.0061 ms |  0.0040 ms |     0.3362 ms |    1.4648 | 0.9766 | 0.9766 |   263.58 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFastFilter      |     0.4582 ms |  0.0118 ms |  0.0078 ms |     0.4457 ms |    7.8125 | 2.4414 | 2.4414 |   578.65 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimized       |     0.8907 ms |  0.0157 ms |  0.0104 ms |     0.8704 ms |    7.8125 | 3.9063 | 3.9063 |   667.76 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFilter          |     1.2576 ms |  0.0409 ms |  0.0271 ms |     1.2132 ms |    1.9531 |      - |      - |   263.07 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinSimple          |     5.2908 ms |  0.1218 ms |  0.0805 ms |     5.1949 ms |         - |      - |      - |    10.52 KB |
| QueryBenchmarkReduced       | QueryReduced       |      Legacy             |    12.8378 ms |  0.2906 ms |  0.1922 ms |    12.5355 ms |         - |      - |      - |    10.52 KB |
```

* коммит: ...
