## Разработка и оптимизация алгортимов поиска сервиса RSSE

### Идеи и варианты реализации: **[Viktor Loshmanov](https://github.com/ViktorLoshmanov)**

---
### Результаты бенчмарков различных алгоритмов

* коммит: доработка поискового движка (добавил алгоритм поиска extended merge)
```
| Type                        | Method             | SearchType              | Mean          | Error      | StdDev    | Min           | Gen0      | Gen1   | Gen2   | Allocated   |
|---------------------------- |------------------- |------------------------ |--------------:|-----------:|----------:|--------------:|----------:|-------:|-------:|------------:|
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFilter          |    50.1939 ms |  0.3212 ms | 0.2124 ms |    49.9135 ms |         - |      - |      - |  2031.65 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFilter          |    51.2786 ms |  0.2520 ms | 0.1500 ms |    50.9940 ms |  100.0000 |      - |      - |  8886.75 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMergeFilter     |    52.5651 ms |  0.3651 ms | 0.2415 ms |    52.1803 ms |         - |      - |      - |  4212.19 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMergeFilter     |    52.8014 ms |  0.4414 ms | 0.2308 ms |    52.3866 ms |  100.0000 |      - |      - | 10636.63 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFastFilter      |    54.3699 ms |  0.4282 ms | 0.2548 ms |    53.8194 ms |         - |      - |      - |  2031.69 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFastFilter      |    55.5851 ms |  0.2407 ms | 0.1259 ms |    55.2975 ms |  111.1111 |      - |      - | 10152.17 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      Legacy             |   751.5548 ms |  7.7985 ms | 4.0788 ms |   747.3944 ms |         - |      - |      - |  2060.84 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFast            |   771.5232 ms |  3.6433 ms | 2.4098 ms |   767.0588 ms |         - |      - |      - |  2003.05 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOptimized       |   805.4549 ms |  2.0856 ms | 1.3795 ms |   802.8828 ms |         - |      - |      - |  2003.05 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFast            |   811.6568 ms |  7.3090 ms | 4.8345 ms |   807.3620 ms |         - |      - |      - | 57833.85 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOptimized       |   831.0106 ms |  2.5608 ms | 1.5239 ms |   828.7428 ms |         - |      - |      - | 57833.90 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinSimple          |   956.8133 ms |  2.8564 ms | 1.8893 ms |   954.3153 ms |         - |      - |      - |  2060.84 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMerge           | 1,103.2116 ms |  3.8053 ms | 2.2645 ms | 1,100.1770 ms |         - |      - |      - | 10779.70 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMerge           | 1,141.4329 ms |  6.5611 ms | 4.3398 ms | 1,135.3190 ms |         - |      - |      - | 13621.54 KB |
|                             |                    |                         |               |            |           |               |           |        |        |             |
| QueryBenchmarkExtended      | QueryExtended      |      GinMergeFilter     |     0.0014 ms |  0.0000 ms | 0.0000 ms |     0.0014 ms |    0.0076 |      - |      - |     0.53 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMergeFilter     |     0.0016 ms |  0.0001 ms | 0.0000 ms |     0.0016 ms |    0.0038 |      - |      - |     0.27 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFilter          |     0.0261 ms |  0.0002 ms | 0.0002 ms |     0.0258 ms |         - |      - |      - |     0.27 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFilter          |     0.0286 ms |  0.0000 ms | 0.0000 ms |     0.0285 ms |    0.1831 |      - |      - |    13.09 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFastFilter      |     0.0589 ms |  0.0001 ms | 0.0001 ms |     0.0589 ms |         - |      - |      - |     0.27 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFastFilter      |     0.0640 ms |  0.0002 ms | 0.0001 ms |     0.0638 ms |    0.3662 |      - |      - |    25.91 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinMerge           |     0.0645 ms |  0.0003 ms | 0.0001 ms |     0.0642 ms |         - |      - |      - |     0.62 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMerge           |     0.0649 ms |  0.0009 ms | 0.0005 ms |     0.0641 ms |         - |      - |      - |     0.53 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFast            |     1.1764 ms |  0.0521 ms | 0.0310 ms |     1.0942 ms |         - |      - |      - |     0.24 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFast            |     1.2934 ms |  0.0467 ms | 0.0309 ms |     1.2488 ms |    3.9063 | 1.9531 | 1.9531 |   252.62 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOptimized       |     1.4691 ms |  0.0681 ms | 0.0450 ms |     1.3979 ms |         - |      - |      - |     0.24 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOptimized       |     1.6168 ms |  0.0478 ms | 0.0316 ms |     1.5590 ms |    3.9063 | 1.9531 | 1.9531 |   252.64 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinSimple          |     6.2747 ms |  0.0466 ms | 0.0308 ms |     6.2248 ms |         - |      - |      - |     0.31 KB |
| QueryBenchmarkExtended      | QueryExtended      |      Legacy             |    11.0469 ms |  0.2727 ms | 0.1804 ms |    10.8132 ms |         - |      - |      - |     0.31 KB |
|                             |                    |                         |               |            |           |               |           |        |        |             |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimized       |    84.0787 ms |  0.3207 ms | 0.1677 ms |    83.8011 ms |         - |      - |      - |  4221.23 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFast            |    88.8736 ms |  0.5475 ms | 0.3622 ms |    88.4086 ms |         - |      - |      - |  4221.18 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimized       |    98.7104 ms |  0.4954 ms | 0.2591 ms |    98.2948 ms | 1000.0000 |      - |      - | 70059.80 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFast            |   104.1059 ms |  0.5747 ms | 0.3801 ms |   103.4504 ms | 1000.0000 |      - |      - | 70864.50 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMerge2          |   196.1923 ms |  0.8091 ms | 0.5352 ms |   195.5013 ms |         - |      - |      - |  8597.31 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMerge2          |   197.6389 ms |  0.5357 ms | 0.3543 ms |   197.0882 ms |         - |      - |      - | 10155.45 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimizedFilter |   206.1447 ms |  0.7180 ms | 0.4749 ms |   205.3767 ms |         - |      - |      - |  4221.18 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFastFilter      |   219.5541 ms |  1.1813 ms | 0.7030 ms |   218.6329 ms |         - |      - |      - |  4221.29 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFilter          |   226.1179 ms |  1.6003 ms | 0.8370 ms |   225.0684 ms |         - |      - |      - |  4221.29 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFilter          |   230.3115 ms |  0.7237 ms | 0.4307 ms |   229.7503 ms |         - |      - |      - | 17172.19 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimizedFilter |   233.8998 ms |  1.0971 ms | 0.6529 ms |   233.1729 ms |         - |      - |      - | 17172.84 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMerge1          |   236.1325 ms |  1.2743 ms | 0.8429 ms |   235.0711 ms |         - |      - |      - |  8597.20 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMerge1          |   236.8707 ms |  1.2559 ms | 0.7474 ms |   236.0835 ms |         - |      - |      - | 10155.57 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFastFilter      |   239.4889 ms |  1.8046 ms | 1.1936 ms |   237.8421 ms |         - |      - |      - | 26163.90 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMergeFilter2    |   244.2119 ms |  1.0243 ms | 0.6775 ms |   243.0860 ms |         - |      - |      - |  6036.29 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMergeFilter1    |   244.3944 ms |  0.8432 ms | 0.5018 ms |   243.5359 ms |         - |      - |      - |  6036.40 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMergeFilter2    |   245.4699 ms |  0.8593 ms | 0.5113 ms |   244.8630 ms |         - |      - |      - | 19673.59 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMergeFilter1    |   250.2838 ms |  1.0159 ms | 0.6720 ms |   249.2310 ms |         - |      - |      - | 19673.56 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      Legacy             |   782.7063 ms |  3.4169 ms | 2.2601 ms |   779.4612 ms |         - |      - |      - |  4279.62 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinSimple          | 2,372.4010 ms | 11.9780 ms | 7.1279 ms | 2,354.0926 ms |         - |      - |      - |  4279.38 KB |
|                             |                    |                         |               |            |           |               |           |        |        |             |
| QueryBenchmarkReduced       | QueryReduced       |      GinMerge2          |     0.0766 ms |  0.0007 ms | 0.0004 ms |     0.0755 ms |    0.1221 |      - |      - |    10.69 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMerge2          |     0.0768 ms |  0.0005 ms | 0.0003 ms |     0.0764 ms |    0.1221 |      - |      - |    10.60 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFast            |     0.1444 ms |  0.0014 ms | 0.0009 ms |     0.1420 ms |         - |      - |      - |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMerge1          |     0.1688 ms |  0.0004 ms | 0.0002 ms |     0.1685 ms |         - |      - |      - |    10.60 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMerge1          |     0.1696 ms |  0.0008 ms | 0.0005 ms |     0.1690 ms |         - |      - |      - |    10.69 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimized       |     0.1951 ms |  0.0017 ms | 0.0010 ms |     0.1925 ms |         - |      - |      - |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFast            |     0.2675 ms |  0.0052 ms | 0.0031 ms |     0.2646 ms |    5.3711 | 2.4414 | 2.4414 |   325.62 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimizedFilter |     0.5145 ms |  0.0014 ms | 0.0009 ms |     0.5136 ms |         - |      - |      - |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFastFilter      |     0.5342 ms |  0.0018 ms | 0.0011 ms |     0.5325 ms |         - |      - |      - |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimized       |     0.5503 ms |  0.0197 ms | 0.0130 ms |     0.5330 ms |   10.7422 | 6.8359 | 6.8359 |   667.76 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimizedFilter |     0.6345 ms |  0.0103 ms | 0.0068 ms |     0.6259 ms |    4.8828 | 1.9531 | 1.9531 |   325.62 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFastFilter      |     0.6863 ms |  0.0097 ms | 0.0058 ms |     0.6738 ms |    6.8359 | 1.9531 | 1.9531 |   446.23 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMergeFilter1    |     1.1050 ms |  0.0365 ms | 0.0242 ms |     1.0468 ms |         - |      - |      - |    10.60 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMergeFilter2    |     1.1127 ms |  0.0253 ms | 0.0167 ms |     1.0689 ms |         - |      - |      - |    10.60 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFilter          |     1.1314 ms |  0.0038 ms | 0.0025 ms |     1.1283 ms |         - |      - |      - |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMergeFilter2    |     1.2050 ms |  0.0401 ms | 0.0265 ms |     1.1556 ms |    3.9063 | 1.9531 | 1.9531 |   325.85 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFilter          |     1.2211 ms |  0.0268 ms | 0.0177 ms |     1.1929 ms |    3.9063 | 1.9531 | 1.9531 |   325.62 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMergeFilter1    |     1.2378 ms |  0.0166 ms | 0.0110 ms |     1.2246 ms |    3.9063 | 1.9531 | 1.9531 |   325.85 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinSimple          |     7.5302 ms |  0.0648 ms | 0.0339 ms |     7.4799 ms |         - |      - |      - |    10.52 KB |
| QueryBenchmarkReduced       | QueryReduced       |      Legacy             |    11.3963 ms |  0.2806 ms | 0.1856 ms |    11.0638 ms |         - |      - |      - |    10.52 KB |
```

* коммит: доработка поискового движка (оптимизация фильтра reduced алгоритмов поиска)
```
| Type                        | Method             | SearchType              | Mean          | Error      | StdDev     | Min           | Gen0      | Gen1   | Gen2   | Allocated   |
|---------------------------- |------------------- |------------------------ |--------------:|-----------:|-----------:|--------------:|----------:|-------:|-------:|------------:|
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFilter          |    53.9560 ms |  1.4969 ms |  0.8908 ms |    52.8030 ms |  111.1111 |      - |      - |  8886.73 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFilter          |    54.7122 ms |  4.3265 ms |  2.8617 ms |    51.5447 ms |         - |      - |      - |  2031.69 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMergeFilter     |    55.4351 ms |  1.4693 ms |  0.8744 ms |    54.7208 ms |         - |      - |      - |  3594.34 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMergeFilter     |    56.9556 ms |  1.7582 ms |  1.1629 ms |    55.2549 ms |  100.0000 |      - |      - | 10018.76 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFastFilter      |    57.4798 ms |  1.9576 ms |  1.2948 ms |    55.2109 ms |         - |      - |      - |  2031.69 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFastFilter      |    60.1209 ms |  1.6454 ms |  1.0883 ms |    57.8985 ms |  111.1111 |      - |      - | 10152.17 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      Legacy             |   762.0704 ms |  4.9299 ms |  3.2608 ms |   756.6511 ms |         - |      - |      - |  2061.12 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFast            |   791.3295 ms |  8.0365 ms |  5.3156 ms |   783.5618 ms |         - |      - |      - |  2003.01 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOptimized       |   823.3200 ms |  3.2705 ms |  1.9462 ms |   819.4882 ms |         - |      - |      - |  2003.34 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFast            |   836.7595 ms | 12.4342 ms |  8.2244 ms |   820.7797 ms |         - |      - |      - | 57833.85 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOptimized       |   875.4625 ms | 17.3019 ms | 11.4442 ms |   862.1049 ms |         - |      - |      - | 57834.13 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinSimple          |   975.5982 ms |  8.5607 ms |  5.6624 ms |   967.0466 ms |         - |      - |      - |  2061.12 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMerge           | 1,141.4907 ms |  6.9964 ms |  4.1634 ms | 1,136.5512 ms |         - |      - |      - |  8107.20 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMerge           | 1,162.8409 ms | 20.3285 ms | 13.4461 ms | 1,146.9443 ms |         - |      - |      - | 10948.71 KB |
|                             |                    |                         |               |            |            |               |           |        |        |             |
| QueryBenchmarkExtended      | QueryExtended      |      GinMergeFilter     |     0.0014 ms |  0.0001 ms |  0.0000 ms |     0.0014 ms |    0.0019 |      - |      - |     0.53 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMergeFilter     |     0.0016 ms |  0.0001 ms |  0.0000 ms |     0.0016 ms |    0.0038 |      - |      - |     0.27 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFilter          |     0.0265 ms |  0.0001 ms |  0.0001 ms |     0.0264 ms |         - |      - |      - |     0.27 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFilter          |     0.0292 ms |  0.0003 ms |  0.0002 ms |     0.0290 ms |    0.0610 |      - |      - |    13.09 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFastFilter      |     0.0596 ms |  0.0004 ms |  0.0002 ms |     0.0593 ms |         - |      - |      - |     0.27 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFastFilter      |     0.0672 ms |  0.0007 ms |  0.0005 ms |     0.0665 ms |    0.1221 |      - |      - |    25.91 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMerge           |     0.0797 ms |  0.0007 ms |  0.0005 ms |     0.0791 ms |         - |      - |      - |     0.50 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinMerge           |     0.0812 ms |  0.0019 ms |  0.0013 ms |     0.0796 ms |         - |      - |      - |     0.59 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFast            |     1.2204 ms |  0.0348 ms |  0.0207 ms |     1.1848 ms |         - |      - |      - |     0.24 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFast            |     1.3656 ms |  0.0505 ms |  0.0334 ms |     1.2917 ms |    3.9063 | 1.9531 | 1.9531 |   252.62 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOptimized       |     1.5626 ms |  0.0198 ms |  0.0118 ms |     1.5426 ms |         - |      - |      - |     0.24 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOptimized       |     1.6890 ms |  0.0489 ms |  0.0291 ms |     1.6299 ms |    3.9063 | 1.9531 | 1.9531 |   252.62 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinSimple          |     6.6667 ms |  0.1293 ms |  0.0856 ms |     6.5541 ms |         - |      - |      - |     0.31 KB |
| QueryBenchmarkExtended      | QueryExtended      |      Legacy             |    11.9285 ms |  0.3431 ms |  0.2042 ms |    11.5677 ms |         - |      - |      - |     0.31 KB |
|                             |                    |                         |               |            |            |               |           |        |        |             |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFastFilter      |    43.5986 ms |  2.7919 ms |  1.8467 ms |    41.5647 ms |         - |      - |      - |  4221.20 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimizedFilter |    44.2094 ms |  2.0811 ms |  1.3765 ms |    42.4335 ms |         - |      - |      - |  4221.17 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMergeFilter     |    49.4845 ms |  2.1184 ms |  1.4012 ms |    47.5551 ms |  100.0000 |      - |      - |  8763.18 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMergeFilter     |    50.3592 ms |  3.2401 ms |  2.1431 ms |    47.4298 ms |  100.0000 |      - |      - |  7205.08 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimizedFilter |    50.4179 ms |  2.1390 ms |  1.4148 ms |    49.0331 ms |  200.0000 |      - |      - | 18043.92 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFastFilter      |    51.3638 ms |  1.4769 ms |  0.8789 ms |    49.9492 ms |  200.0000 |      - |      - | 18044.15 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimized       |    92.6763 ms |  5.9766 ms |  3.9531 ms |    88.2824 ms |         - |      - |      - |  4221.22 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFast            |    93.7737 ms |  1.8520 ms |  1.2250 ms |    91.8368 ms |         - |      - |      - |  4221.18 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimized       |   103.2977 ms |  1.4594 ms |  0.9653 ms |   101.5832 ms | 1000.0000 |      - |      - | 70059.82 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFast            |   111.5162 ms |  1.9068 ms |  1.2612 ms |   110.0219 ms | 1000.0000 |      - |      - | 70864.50 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMerge           |   209.4256 ms |  3.8929 ms |  2.5749 ms |   205.0923 ms |         - |      - |      - |  8746.45 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMerge           |   211.8614 ms |  4.5806 ms |  3.0298 ms |   207.3218 ms |         - |      - |      - |  7188.84 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFilter          |   231.3982 ms |  2.8217 ms |  1.6791 ms |   228.3870 ms |         - |      - |      - |  4221.18 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFilter          |   233.5353 ms |  3.2194 ms |  2.1295 ms |   230.3926 ms |         - |      - |      - | 16977.93 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      Legacy             |   809.3814 ms |  3.3375 ms |  1.7456 ms |   806.8662 ms |         - |      - |      - |  4279.66 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinSimple          | 2,451.5177 ms | 28.5173 ms | 18.8625 ms | 2,401.1861 ms |         - |      - |      - |  4279.95 KB |
|                             |                    |                         |               |            |            |               |           |        |        |             |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMerge           |     0.1074 ms |  0.0007 ms |  0.0005 ms |     0.1062 ms |    0.1221 |      - |      - |    10.57 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMerge           |     0.1078 ms |  0.0005 ms |  0.0003 ms |     0.1074 ms |    0.1221 |      - |      - |    10.66 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFast            |     0.1691 ms |  0.0010 ms |  0.0006 ms |     0.1683 ms |         - |      - |      - |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimized       |     0.2194 ms |  0.0070 ms |  0.0046 ms |     0.2139 ms |         - |      - |      - |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMergeFilter     |     0.2298 ms |  0.0010 ms |  0.0006 ms |     0.2292 ms |         - |      - |      - |    10.77 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMergeFilter     |     0.2391 ms |  0.0046 ms |  0.0030 ms |     0.2353 ms |         - |      - |      - |    10.69 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFast            |     0.2930 ms |  0.0136 ms |  0.0090 ms |     0.2771 ms |    5.3711 | 2.4414 | 2.4414 |   325.62 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimizedFilter |     0.4914 ms |  0.0075 ms |  0.0050 ms |     0.4830 ms |         - |      - |      - |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFastFilter      |     0.4965 ms |  0.0052 ms |  0.0034 ms |     0.4928 ms |         - |      - |      - |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimized       |     0.5544 ms |  0.0193 ms |  0.0115 ms |     0.5398 ms |   10.7422 | 6.8359 | 6.8359 |   667.75 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimizedFilter |     0.6206 ms |  0.0148 ms |  0.0098 ms |     0.6072 ms |    4.8828 | 1.9531 | 1.9531 |   325.66 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFastFilter      |     0.6233 ms |  0.0177 ms |  0.0117 ms |     0.6086 ms |    4.8828 | 1.9531 | 1.9531 |   341.87 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFilter          |     1.1458 ms |  0.0361 ms |  0.0239 ms |     1.0918 ms |         - |      - |      - |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFilter          |     1.2856 ms |  0.0230 ms |  0.0152 ms |     1.2579 ms |    3.9063 | 1.9531 | 1.9531 |   325.62 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinSimple          |     7.8852 ms |  0.1859 ms |  0.1106 ms |     7.7130 ms |         - |      - |      - |    10.52 KB |
| QueryBenchmarkReduced       | QueryReduced       |      Legacy             |    12.5737 ms |  0.2876 ms |  0.1902 ms |    12.2835 ms |         - |      - |      - |    10.52 KB |
```

* коммит: ...

