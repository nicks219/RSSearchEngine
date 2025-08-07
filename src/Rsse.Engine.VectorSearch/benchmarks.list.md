## Разработка и оптимизация алгортимов поиска сервиса RSSE

### Идеи и варианты реализации: **[Viktor Loshmanov](https://github.com/ViktorLoshmanov)**

---
### Результаты бенчмарков различных алгоритмов

* коммит: доработка поискового движка (добавил алгоритм поиска extended merge)
```
| Type                        | Method             | SearchType              | Mean          | Error      | StdDev    | Min           | Allocated   |
|---------------------------- |------------------- |------------------------ |--------------:|-----------:|----------:|--------------:|------------:|
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFilter          |    50.1939 ms |  0.3212 ms | 0.2124 ms |    49.9135 ms |  2031.65 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFilter          |    51.2786 ms |  0.2520 ms | 0.1500 ms |    50.9940 ms |  8886.75 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMergeFilter     |    52.5651 ms |  0.3651 ms | 0.2415 ms |    52.1803 ms |  4212.19 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMergeFilter     |    52.8014 ms |  0.4414 ms | 0.2308 ms |    52.3866 ms | 10636.63 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFastFilter      |    54.3699 ms |  0.4282 ms | 0.2548 ms |    53.8194 ms |  2031.69 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFastFilter      |    55.5851 ms |  0.2407 ms | 0.1259 ms |    55.2975 ms | 10152.17 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      Legacy             |   751.5548 ms |  7.7985 ms | 4.0788 ms |   747.3944 ms |  2060.84 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFast            |   771.5232 ms |  3.6433 ms | 2.4098 ms |   767.0588 ms |  2003.05 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOptimized       |   805.4549 ms |  2.0856 ms | 1.3795 ms |   802.8828 ms |  2003.05 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFast            |   811.6568 ms |  7.3090 ms | 4.8345 ms |   807.3620 ms | 57833.85 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOptimized       |   831.0106 ms |  2.5608 ms | 1.5239 ms |   828.7428 ms | 57833.90 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinSimple          |   956.8133 ms |  2.8564 ms | 1.8893 ms |   954.3153 ms |  2060.84 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMerge           | 1,103.2116 ms |  3.8053 ms | 2.2645 ms | 1,100.1770 ms | 10779.70 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMerge           | 1,141.4329 ms |  6.5611 ms | 4.3398 ms | 1,135.3190 ms | 13621.54 KB |
|                             |                    |                         |               |            |           |               |             |
| QueryBenchmarkExtended      | QueryExtended      |      GinMergeFilter     |     0.0014 ms |  0.0000 ms | 0.0000 ms |     0.0014 ms |     0.53 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMergeFilter     |     0.0016 ms |  0.0001 ms | 0.0000 ms |     0.0016 ms |     0.27 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFilter          |     0.0261 ms |  0.0002 ms | 0.0002 ms |     0.0258 ms |     0.27 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFilter          |     0.0286 ms |  0.0000 ms | 0.0000 ms |     0.0285 ms |    13.09 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFastFilter      |     0.0589 ms |  0.0001 ms | 0.0001 ms |     0.0589 ms |     0.27 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFastFilter      |     0.0640 ms |  0.0002 ms | 0.0001 ms |     0.0638 ms |    25.91 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinMerge           |     0.0645 ms |  0.0003 ms | 0.0001 ms |     0.0642 ms |     0.62 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMerge           |     0.0649 ms |  0.0009 ms | 0.0005 ms |     0.0641 ms |     0.53 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFast            |     1.1764 ms |  0.0521 ms | 0.0310 ms |     1.0942 ms |     0.24 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFast            |     1.2934 ms |  0.0467 ms | 0.0309 ms |     1.2488 ms |   252.62 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOptimized       |     1.4691 ms |  0.0681 ms | 0.0450 ms |     1.3979 ms |     0.24 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOptimized       |     1.6168 ms |  0.0478 ms | 0.0316 ms |     1.5590 ms |   252.64 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinSimple          |     6.2747 ms |  0.0466 ms | 0.0308 ms |     6.2248 ms |     0.31 KB |
| QueryBenchmarkExtended      | QueryExtended      |      Legacy             |    11.0469 ms |  0.2727 ms | 0.1804 ms |    10.8132 ms |     0.31 KB |
|                             |                    |                         |               |            |           |               |             |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimized       |    84.0787 ms |  0.3207 ms | 0.1677 ms |    83.8011 ms |  4221.23 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFast            |    88.8736 ms |  0.5475 ms | 0.3622 ms |    88.4086 ms |  4221.18 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimized       |    98.7104 ms |  0.4954 ms | 0.2591 ms |    98.2948 ms | 70059.80 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFast            |   104.1059 ms |  0.5747 ms | 0.3801 ms |   103.4504 ms | 70864.50 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMerge2          |   196.1923 ms |  0.8091 ms | 0.5352 ms |   195.5013 ms |  8597.31 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMerge2          |   197.6389 ms |  0.5357 ms | 0.3543 ms |   197.0882 ms | 10155.45 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimizedFilter |   206.1447 ms |  0.7180 ms | 0.4749 ms |   205.3767 ms |  4221.18 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFastFilter      |   219.5541 ms |  1.1813 ms | 0.7030 ms |   218.6329 ms |  4221.29 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFilter          |   226.1179 ms |  1.6003 ms | 0.8370 ms |   225.0684 ms |  4221.29 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFilter          |   230.3115 ms |  0.7237 ms | 0.4307 ms |   229.7503 ms | 17172.19 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimizedFilter |   233.8998 ms |  1.0971 ms | 0.6529 ms |   233.1729 ms | 17172.84 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMerge1          |   236.1325 ms |  1.2743 ms | 0.8429 ms |   235.0711 ms |  8597.20 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMerge1          |   236.8707 ms |  1.2559 ms | 0.7474 ms |   236.0835 ms | 10155.57 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFastFilter      |   239.4889 ms |  1.8046 ms | 1.1936 ms |   237.8421 ms | 26163.90 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMergeFilter2    |   244.2119 ms |  1.0243 ms | 0.6775 ms |   243.0860 ms |  6036.29 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMergeFilter1    |   244.3944 ms |  0.8432 ms | 0.5018 ms |   243.5359 ms |  6036.40 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMergeFilter2    |   245.4699 ms |  0.8593 ms | 0.5113 ms |   244.8630 ms | 19673.59 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMergeFilter1    |   250.2838 ms |  1.0159 ms | 0.6720 ms |   249.2310 ms | 19673.56 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      Legacy             |   782.7063 ms |  3.4169 ms | 2.2601 ms |   779.4612 ms |  4279.62 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinSimple          | 2,372.4010 ms | 11.9780 ms | 7.1279 ms | 2,354.0926 ms |  4279.38 KB |
|                             |                    |                         |               |            |           |               |             |
| QueryBenchmarkReduced       | QueryReduced       |      GinMerge2          |     0.0766 ms |  0.0007 ms | 0.0004 ms |     0.0755 ms |    10.69 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMerge2          |     0.0768 ms |  0.0005 ms | 0.0003 ms |     0.0764 ms |    10.60 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFast            |     0.1444 ms |  0.0014 ms | 0.0009 ms |     0.1420 ms |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMerge1          |     0.1688 ms |  0.0004 ms | 0.0002 ms |     0.1685 ms |    10.60 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMerge1          |     0.1696 ms |  0.0008 ms | 0.0005 ms |     0.1690 ms |    10.69 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimized       |     0.1951 ms |  0.0017 ms | 0.0010 ms |     0.1925 ms |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFast            |     0.2675 ms |  0.0052 ms | 0.0031 ms |     0.2646 ms |   325.62 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimizedFilter |     0.5145 ms |  0.0014 ms | 0.0009 ms |     0.5136 ms |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFastFilter      |     0.5342 ms |  0.0018 ms | 0.0011 ms |     0.5325 ms |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimized       |     0.5503 ms |  0.0197 ms | 0.0130 ms |     0.5330 ms |   667.76 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimizedFilter |     0.6345 ms |  0.0103 ms | 0.0068 ms |     0.6259 ms |   325.62 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFastFilter      |     0.6863 ms |  0.0097 ms | 0.0058 ms |     0.6738 ms |   446.23 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMergeFilter1    |     1.1050 ms |  0.0365 ms | 0.0242 ms |     1.0468 ms |    10.60 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMergeFilter2    |     1.1127 ms |  0.0253 ms | 0.0167 ms |     1.0689 ms |    10.60 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFilter          |     1.1314 ms |  0.0038 ms | 0.0025 ms |     1.1283 ms |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMergeFilter2    |     1.2050 ms |  0.0401 ms | 0.0265 ms |     1.1556 ms |   325.85 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFilter          |     1.2211 ms |  0.0268 ms | 0.0177 ms |     1.1929 ms |   325.62 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMergeFilter1    |     1.2378 ms |  0.0166 ms | 0.0110 ms |     1.2246 ms |   325.85 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinSimple          |     7.5302 ms |  0.0648 ms | 0.0339 ms |     7.4799 ms |    10.52 KB |
| QueryBenchmarkReduced       | QueryReduced       |      Legacy             |    11.3963 ms |  0.2806 ms | 0.1856 ms |    11.0638 ms |    10.52 KB |
```

* коммит: доработка поискового движка (оптимизация фильтра reduced алгоритмов поиска)
```
| Type                        | Method             | SearchType              | Mean          | Error      | StdDev     | Min           | Allocated   |
|---------------------------- |------------------- |------------------------ |--------------:|-----------:|-----------:|--------------:|------------:|
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFilter          |    53.9560 ms |  1.4969 ms |  0.8908 ms |    52.8030 ms |  8886.73 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFilter          |    54.7122 ms |  4.3265 ms |  2.8617 ms |    51.5447 ms |  2031.69 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMergeFilter     |    55.4351 ms |  1.4693 ms |  0.8744 ms |    54.7208 ms |  3594.34 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMergeFilter     |    56.9556 ms |  1.7582 ms |  1.1629 ms |    55.2549 ms | 10018.76 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFastFilter      |    57.4798 ms |  1.9576 ms |  1.2948 ms |    55.2109 ms |  2031.69 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFastFilter      |    60.1209 ms |  1.6454 ms |  1.0883 ms |    57.8985 ms | 10152.17 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      Legacy             |   762.0704 ms |  4.9299 ms |  3.2608 ms |   756.6511 ms |  2061.12 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFast            |   791.3295 ms |  8.0365 ms |  5.3156 ms |   783.5618 ms |  2003.01 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOptimized       |   823.3200 ms |  3.2705 ms |  1.9462 ms |   819.4882 ms |  2003.34 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFast            |   836.7595 ms | 12.4342 ms |  8.2244 ms |   820.7797 ms | 57833.85 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOptimized       |   875.4625 ms | 17.3019 ms | 11.4442 ms |   862.1049 ms | 57834.13 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinSimple          |   975.5982 ms |  8.5607 ms |  5.6624 ms |   967.0466 ms |  2061.12 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMerge           | 1,141.4907 ms |  6.9964 ms |  4.1634 ms | 1,136.5512 ms |  8107.20 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMerge           | 1,162.8409 ms | 20.3285 ms | 13.4461 ms | 1,146.9443 ms | 10948.71 KB |
|                             |                    |                         |               |            |            |               |             |
| QueryBenchmarkExtended      | QueryExtended      |      GinMergeFilter     |     0.0014 ms |  0.0001 ms |  0.0000 ms |     0.0014 ms |     0.53 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMergeFilter     |     0.0016 ms |  0.0001 ms |  0.0000 ms |     0.0016 ms |     0.27 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFilter          |     0.0265 ms |  0.0001 ms |  0.0001 ms |     0.0264 ms |     0.27 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFilter          |     0.0292 ms |  0.0003 ms |  0.0002 ms |     0.0290 ms |    13.09 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFastFilter      |     0.0596 ms |  0.0004 ms |  0.0002 ms |     0.0593 ms |     0.27 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFastFilter      |     0.0672 ms |  0.0007 ms |  0.0005 ms |     0.0665 ms |    25.91 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMerge           |     0.0797 ms |  0.0007 ms |  0.0005 ms |     0.0791 ms |     0.50 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinMerge           |     0.0812 ms |  0.0019 ms |  0.0013 ms |     0.0796 ms |     0.59 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFast            |     1.2204 ms |  0.0348 ms |  0.0207 ms |     1.1848 ms |     0.24 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFast            |     1.3656 ms |  0.0505 ms |  0.0334 ms |     1.2917 ms |   252.62 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOptimized       |     1.5626 ms |  0.0198 ms |  0.0118 ms |     1.5426 ms |     0.24 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOptimized       |     1.6890 ms |  0.0489 ms |  0.0291 ms |     1.6299 ms |   252.62 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinSimple          |     6.6667 ms |  0.1293 ms |  0.0856 ms |     6.5541 ms |     0.31 KB |
| QueryBenchmarkExtended      | QueryExtended      |      Legacy             |    11.9285 ms |  0.3431 ms |  0.2042 ms |    11.5677 ms |     0.31 KB |
|                             |                    |                         |               |            |            |               |             |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFastFilter      |    43.5986 ms |  2.7919 ms |  1.8467 ms |    41.5647 ms |  4221.20 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimizedFilter |    44.2094 ms |  2.0811 ms |  1.3765 ms |    42.4335 ms |  4221.17 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMergeFilter     |    49.4845 ms |  2.1184 ms |  1.4012 ms |    47.5551 ms |  8763.18 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMergeFilter     |    50.3592 ms |  3.2401 ms |  2.1431 ms |    47.4298 ms |  7205.08 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimizedFilter |    50.4179 ms |  2.1390 ms |  1.4148 ms |    49.0331 ms | 18043.92 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFastFilter      |    51.3638 ms |  1.4769 ms |  0.8789 ms |    49.9492 ms | 18044.15 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimized       |    92.6763 ms |  5.9766 ms |  3.9531 ms |    88.2824 ms |  4221.22 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFast            |    93.7737 ms |  1.8520 ms |  1.2250 ms |    91.8368 ms |  4221.18 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimized       |   103.2977 ms |  1.4594 ms |  0.9653 ms |   101.5832 ms | 70059.82 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFast            |   111.5162 ms |  1.9068 ms |  1.2612 ms |   110.0219 ms | 70864.50 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMerge           |   209.4256 ms |  3.8929 ms |  2.5749 ms |   205.0923 ms |  8746.45 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMerge           |   211.8614 ms |  4.5806 ms |  3.0298 ms |   207.3218 ms |  7188.84 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFilter          |   231.3982 ms |  2.8217 ms |  1.6791 ms |   228.3870 ms |  4221.18 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFilter          |   233.5353 ms |  3.2194 ms |  2.1295 ms |   230.3926 ms | 16977.93 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      Legacy             |   809.3814 ms |  3.3375 ms |  1.7456 ms |   806.8662 ms |  4279.66 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinSimple          | 2,451.5177 ms | 28.5173 ms | 18.8625 ms | 2,401.1861 ms |  4279.95 KB |
|                             |                    |                         |               |            |            |               |             |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMerge           |     0.1074 ms |  0.0007 ms |  0.0005 ms |     0.1062 ms |    10.57 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMerge           |     0.1078 ms |  0.0005 ms |  0.0003 ms |     0.1074 ms |    10.66 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFast            |     0.1691 ms |  0.0010 ms |  0.0006 ms |     0.1683 ms |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimized       |     0.2194 ms |  0.0070 ms |  0.0046 ms |     0.2139 ms |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMergeFilter     |     0.2298 ms |  0.0010 ms |  0.0006 ms |     0.2292 ms |    10.77 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMergeFilter     |     0.2391 ms |  0.0046 ms |  0.0030 ms |     0.2353 ms |    10.69 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFast            |     0.2930 ms |  0.0136 ms |  0.0090 ms |     0.2771 ms |   325.62 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimizedFilter |     0.4914 ms |  0.0075 ms |  0.0050 ms |     0.4830 ms |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFastFilter      |     0.4965 ms |  0.0052 ms |  0.0034 ms |     0.4928 ms |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimized       |     0.5544 ms |  0.0193 ms |  0.0115 ms |     0.5398 ms |   667.75 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimizedFilter |     0.6206 ms |  0.0148 ms |  0.0098 ms |     0.6072 ms |   325.66 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFastFilter      |     0.6233 ms |  0.0177 ms |  0.0117 ms |     0.6086 ms |   341.87 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFilter          |     1.1458 ms |  0.0361 ms |  0.0239 ms |     1.0918 ms |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFilter          |     1.2856 ms |  0.0230 ms |  0.0152 ms |     1.2579 ms |   325.62 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinSimple          |     7.8852 ms |  0.1859 ms |  0.1106 ms |     7.7130 ms |    10.52 KB |
| QueryBenchmarkReduced       | QueryReduced       |      Legacy             |    12.5737 ms |  0.2876 ms |  0.1902 ms |    12.2835 ms |    10.52 KB |
```

* коммит: доработка поискового движка (оптимизация extended merge. алгоритмов поиска)
```
| Type                        | Method             | SearchType              | Mean          | Error      | StdDev     | Min           | Allocated   |
|---------------------------- |------------------- |------------------------ |--------------:|-----------:|-----------:|--------------:|------------:|
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFilter          |    50.1577 ms |  3.0802 ms |  2.0373 ms |    47.5964 ms |  8674.57 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFilter          |    51.0422 ms |  2.3626 ms |  1.5627 ms |    48.4466 ms |  2031.69 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMergeFilter     |    53.3585 ms |  2.3121 ms |  1.5293 ms |    49.9762 ms |  2031.69 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMergeFilter     |    55.2457 ms |  2.4130 ms |  1.5960 ms |    53.1031 ms | 10857.25 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFastFilter      |    55.5825 ms |  3.2505 ms |  2.1500 ms |    52.1236 ms |  9755.88 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFastFilter      |    57.1350 ms |  2.7090 ms |  1.7919 ms |    54.3452 ms |  2031.66 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      Legacy             |   785.3131 ms |  9.5459 ms |  6.3140 ms |   777.9018 ms |  2061.12 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFast            |   818.5429 ms |  3.9576 ms |  2.6177 ms |   814.5323 ms |  2002.77 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOptimized       |   830.1739 ms |  8.7512 ms |  5.7884 ms |   819.9766 ms |  2003.34 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFast            |   844.0156 ms |  8.8232 ms |  5.2506 ms |   836.9427 ms | 57834.18 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOptimized       |   883.4390 ms | 13.5857 ms |  8.0846 ms |   872.7538 ms | 57834.18 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMerge           |   967.9809 ms |  7.7379 ms |  5.1182 ms |   957.2902 ms |  2032.09 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMerge           |   980.4491 ms | 21.8488 ms | 13.0019 ms |   962.4878 ms | 18482.63 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinSimple          |   990.1311 ms |  8.5468 ms |  5.6532 ms |   982.3073 ms |  2061.12 KB |
|                             |                    |                         |               |            |            |               |             |
| QueryBenchmarkExtended      | QueryExtended      |      GinFilter          |     0.0009 ms |  0.0001 ms |  0.0000 ms |     0.0009 ms |     0.45 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinMergeFilter     |     0.0010 ms |  0.0001 ms |  0.0000 ms |     0.0010 ms |     0.42 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFastFilter      |     0.0011 ms |  0.0001 ms |  0.0000 ms |     0.0010 ms |     0.45 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFastFilter      |     0.0012 ms |  0.0001 ms |  0.0001 ms |     0.0011 ms |     0.27 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFilter          |     0.0012 ms |  0.0001 ms |  0.0000 ms |     0.0011 ms |     0.27 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMergeFilter     |     0.0012 ms |  0.0001 ms |  0.0000 ms |     0.0011 ms |     0.27 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMerge           |     0.0718 ms |  0.0013 ms |  0.0008 ms |     0.0711 ms |     0.27 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinMerge           |     0.0733 ms |  0.0014 ms |  0.0009 ms |     0.0720 ms |     1.07 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFast            |     1.2576 ms |  0.0809 ms |  0.0535 ms |     1.1825 ms |     0.24 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFast            |     1.4826 ms |  0.0883 ms |  0.0525 ms |     1.4252 ms |   252.62 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOptimized       |     1.6790 ms |  0.0693 ms |  0.0362 ms |     1.6010 ms |     0.24 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOptimized       |     1.8075 ms |  0.1181 ms |  0.0703 ms |     1.6905 ms |   252.62 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinSimple          |     7.9831 ms |  0.3395 ms |  0.2246 ms |     7.7126 ms |     0.31 KB |
| QueryBenchmarkExtended      | QueryExtended      |      Legacy             |    13.3029 ms |  0.3322 ms |  0.1977 ms |    12.9050 ms |     0.31 KB |
|                             |                    |                         |               |            |            |               |             |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimizedFilter |    43.4428 ms |  2.4689 ms |  1.6330 ms |    41.6220 ms |  4221.20 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFastFilter      |    44.5187 ms |  2.3953 ms |  1.5843 ms |    42.4940 ms |  4221.20 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMergeFilter     |    49.5140 ms |  1.3661 ms |  0.9036 ms |    47.8327 ms |  4221.20 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMergeFilter     |    49.8822 ms |  1.6967 ms |  1.1222 ms |    47.8144 ms |  8768.14 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFastFilter      |    51.7484 ms |  2.3672 ms |  1.4087 ms |    49.7948 ms | 17138.07 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimizedFilter |    53.0593 ms |  2.5872 ms |  1.7113 ms |    51.0273 ms | 17137.87 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimized       |    92.6655 ms |  1.8921 ms |  1.2515 ms |    89.6886 ms |  4221.22 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFast            |    96.7399 ms |  2.5907 ms |  1.7136 ms |    93.8034 ms |  4221.23 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimized       |   109.4775 ms |  3.0508 ms |  2.0179 ms |   105.9789 ms | 70059.82 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFast            |   115.2140 ms |  1.7050 ms |  1.0146 ms |   113.5302 ms | 70864.51 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFilter          |   220.7092 ms |  1.9997 ms |  1.0459 ms |   218.8145 ms |  4221.29 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFilter          |   226.0604 ms |  2.6005 ms |  1.5475 ms |   223.7284 ms | 16132.68 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMerge           |   251.3031 ms |  3.9088 ms |  2.0444 ms |   248.3502 ms |  4221.32 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMerge           |   257.9941 ms |  6.3488 ms |  4.1994 ms |   250.8796 ms |  8747.06 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      Legacy             |   835.2069 ms |  9.5786 ms |  6.3356 ms |   823.8883 ms |  4279.95 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinSimple          | 2,440.9169 ms | 30.8702 ms | 20.4188 ms | 2,414.6679 ms |  4279.66 KB |
|                             |                    |                         |               |            |            |               |             |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMergeFilter     |     0.1094 ms |  0.0010 ms |  0.0007 ms |     0.1083 ms |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMergeFilter     |     0.1109 ms |  0.0018 ms |  0.0012 ms |     0.1098 ms |    10.77 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMerge           |     0.1172 ms |  0.0007 ms |  0.0005 ms |     0.1163 ms |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMerge           |     0.1175 ms |  0.0007 ms |  0.0004 ms |     0.1168 ms |    10.66 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFast            |     0.1526 ms |  0.0022 ms |  0.0015 ms |     0.1504 ms |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimized       |     0.2081 ms |  0.0017 ms |  0.0012 ms |     0.2069 ms |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimizedFilter |     0.2160 ms |  0.0017 ms |  0.0011 ms |     0.2145 ms |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimizedFilter |     0.2482 ms |  0.0024 ms |  0.0014 ms |     0.2466 ms |   193.40 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFast            |     0.2920 ms |  0.0065 ms |  0.0043 ms |     0.2870 ms |   325.62 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFastFilter      |     0.3440 ms |  0.0031 ms |  0.0018 ms |     0.3409 ms |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFastFilter      |     0.3903 ms |  0.0060 ms |  0.0039 ms |     0.3862 ms |   177.38 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFilter          |     0.4340 ms |  0.0224 ms |  0.0148 ms |     0.4197 ms |    10.45 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFilter          |     0.4863 ms |  0.0103 ms |  0.0068 ms |     0.4770 ms |   161.13 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimized       |     0.5743 ms |  0.0244 ms |  0.0161 ms |     0.5462 ms |   667.76 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinSimple          |     8.4266 ms |  0.2931 ms |  0.1939 ms |     8.1289 ms |    10.52 KB |
| QueryBenchmarkReduced       | QueryReduced       |      Legacy             |    13.8913 ms |  0.3025 ms |  0.2001 ms |    13.5680 ms |    10.52 KB |
```

* коммит: доработка поискового движка (добавлен индекс сохраняющий позиции токенов в документах)
```
| Type                        | Method             | SearchType              | Mean          | Error      | StdDev     | Min           | Allocated   |
|---------------------------- |------------------- |------------------------ |--------------:|-----------:|-----------:|--------------:|------------:|
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFilter          |    45.3653 ms |  0.4438 ms |  0.2936 ms |    45.0009 ms |  2031.65 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMergeFilter     |    45.9555 ms |  0.2745 ms |  0.1634 ms |    45.7649 ms |  2031.68 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFilter          |    46.7901 ms |  0.6469 ms |  0.4279 ms |    46.0010 ms |  8674.57 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMergeFilter     |    47.8381 ms |  0.7564 ms |  0.5003 ms |    47.2604 ms | 10857.24 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFastFilter      |    48.8358 ms |  0.3236 ms |  0.2140 ms |    48.5159 ms |  2031.68 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFastFilter      |    50.4963 ms |  1.0184 ms |  0.6060 ms |    49.7677 ms |  9755.88 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOffsetFilter    |    61.8505 ms |  0.6115 ms |  0.4045 ms |    61.2815 ms | 16957.72 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOffsetFilter    |    62.5459 ms |  0.8291 ms |  0.5484 ms |    61.8919 ms | 30489.34 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      Legacy             |   745.6730 ms |  2.1763 ms |  1.4395 ms |   743.0039 ms |  2060.84 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFast            |   777.7762 ms |  2.7763 ms |  1.8363 ms |   775.5342 ms |  2003.05 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOptimized       |   788.1736 ms |  7.4548 ms |  4.4362 ms |   776.4198 ms |  2003.05 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFast            |   811.9677 ms |  3.1023 ms |  2.0520 ms |   808.8014 ms | 57834.46 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOptimized       |   817.6603 ms |  2.2516 ms |  1.3399 ms |   816.1847 ms | 57834.13 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinSimple          |   915.3552 ms |  6.1694 ms |  4.0807 ms |   910.5364 ms |  2060.79 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMerge           |   951.1542 ms |  8.8103 ms |  5.2429 ms |   944.7568 ms | 18482.63 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMerge           |   953.3569 ms |  6.5307 ms |  4.3196 ms |   942.2270 ms |  2032.09 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOffset          | 2,689.4743 ms | 26.0290 ms | 17.2166 ms | 2,642.0403 ms | 15534.73 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOffset          | 2,704.3974 ms | 25.1096 ms | 13.1328 ms | 2,675.3342 ms |  2003.05 KB |
|                             |                    |                         |               |            |            |               |             |
| QueryBenchmarkExtended      | QueryExtended      |      GinMergeFilter     |     0.0105 ms |  0.0000 ms |  0.0000 ms |     0.0104 ms |     5.49 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMergeFilter     |     0.0106 ms |  0.0001 ms |  0.0001 ms |     0.0105 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFilter          |     0.0109 ms |  0.0000 ms |  0.0000 ms |     0.0109 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFastFilter      |     0.0112 ms |  0.0000 ms |  0.0000 ms |     0.0112 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFilter          |     0.0121 ms |  0.0001 ms |  0.0000 ms |     0.0120 ms |    10.92 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFastFilter      |     0.0122 ms |  0.0001 ms |  0.0000 ms |     0.0121 ms |    11.15 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOffsetFilter    |     0.0197 ms |  0.0002 ms |  0.0001 ms |     0.0194 ms |     5.88 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOffsetFilter    |     0.0203 ms |  0.0006 ms |  0.0004 ms |     0.0198 ms |     6.43 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOffset          |     0.2352 ms |  0.0042 ms |  0.0028 ms |     0.2306 ms |     5.23 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOffset          |     0.2525 ms |  0.0011 ms |  0.0007 ms |     0.2512 ms |     4.69 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMerge           |     0.5223 ms |  0.0168 ms |  0.0111 ms |     0.5023 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinMerge           |     0.5306 ms |  0.0146 ms |  0.0096 ms |     0.5066 ms |     5.86 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFast            |     3.0391 ms |  0.1192 ms |  0.0789 ms |     2.8665 ms |     4.69 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFast            |     3.4633 ms |  0.0840 ms |  0.0556 ms |     3.3241 ms |   531.04 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOptimized       |     3.8481 ms |  0.1056 ms |  0.0698 ms |     3.7273 ms |   531.05 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOptimized       |     4.1324 ms |  0.9857 ms |  0.6520 ms |     3.4466 ms |     4.69 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinSimple          |    11.6284 ms |  0.1585 ms |  0.0943 ms |    11.4818 ms |     4.75 KB |
| QueryBenchmarkExtended      | QueryExtended      |      Legacy             |    13.0268 ms |  0.1329 ms |  0.0695 ms |    12.9361 ms |     4.75 KB |
|                             |                    |                         |               |            |            |               |             |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimizedFilter |    39.4003 ms |  0.2005 ms |  0.1193 ms |    39.2110 ms |  4221.19 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFastFilter      |    39.4295 ms |  0.1348 ms |  0.0891 ms |    39.3225 ms |  4221.19 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMergeFilter     |    44.4011 ms |  0.2168 ms |  0.1434 ms |    44.2115 ms |  4221.20 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMergeFilter     |    45.6786 ms |  0.1845 ms |  0.1220 ms |    45.4539 ms |  8768.11 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimizedFilter |    46.8652 ms |  0.3721 ms |  0.1946 ms |    46.6806 ms | 17137.86 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFastFilter      |    46.9087 ms |  0.2795 ms |  0.1849 ms |    46.6343 ms | 17138.05 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimized       |    84.8419 ms |  0.6189 ms |  0.4094 ms |    84.3486 ms |  4221.17 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFast            |    89.3428 ms |  0.5457 ms |  0.3610 ms |    88.8463 ms |  4221.17 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimized       |    98.2078 ms |  0.5132 ms |  0.3395 ms |    97.4848 ms | 70059.80 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFast            |   107.8532 ms |  3.6813 ms |  2.4350 ms |   105.4417 ms | 70864.51 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFilter          |   207.7001 ms |  0.5203 ms |  0.3096 ms |   207.2132 ms |  4221.29 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFilter          |   212.4016 ms |  1.0168 ms |  0.6726 ms |   211.4470 ms | 16132.68 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMerge           |   220.5222 ms |  0.8459 ms |  0.5595 ms |   219.5741 ms |  4221.48 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMerge           |   221.6636 ms |  0.9559 ms |  0.5688 ms |   220.6983 ms |  8746.52 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      Legacy             |   787.6572 ms |  6.4279 ms |  4.2517 ms |   775.8852 ms |  4279.66 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinSimple          | 2,327.1324 ms | 20.0722 ms | 13.2765 ms | 2,290.5303 ms |  4279.95 KB |
|                             |                    |                         |               |            |            |               |             |
| QueryBenchmarkReduced       | QueryReduced       |      GinMergeFilter     |     0.1065 ms |  0.0011 ms |  0.0007 ms |     0.1046 ms |     5.44 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMergeFilter     |     0.1120 ms |  0.0002 ms |  0.0001 ms |     0.1117 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimizedFilter |     0.2104 ms |  0.0021 ms |  0.0014 ms |     0.2065 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFastFilter      |     0.2241 ms |  0.0003 ms |  0.0002 ms |     0.2238 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFast            |     0.2356 ms |  0.0022 ms |  0.0015 ms |     0.2314 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMerge           |     0.2376 ms |  0.0015 ms |  0.0010 ms |     0.2361 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMerge           |     0.2438 ms |  0.0046 ms |  0.0030 ms |     0.2382 ms |     5.47 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimized       |     0.2525 ms |  0.0006 ms |  0.0004 ms |     0.2517 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimizedFilter |     0.2556 ms |  0.0010 ms |  0.0005 ms |     0.2550 ms |   188.06 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFastFilter      |     0.2647 ms |  0.0008 ms |  0.0004 ms |     0.2641 ms |   188.06 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFilter          |     0.5098 ms |  0.0104 ms |  0.0062 ms |     0.4980 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFilter          |     0.5381 ms |  0.0118 ms |  0.0070 ms |     0.5250 ms |   155.79 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFast            |     0.5492 ms |  0.0143 ms |  0.0095 ms |     0.5361 ms |   662.51 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimized       |     0.9851 ms |  0.0212 ms |  0.0140 ms |     0.9649 ms |  1372.41 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinSimple          |    11.0786 ms |  0.0476 ms |  0.0315 ms |    11.0312 ms |     5.10 KB |
| QueryBenchmarkReduced       | QueryReduced       |      Legacy             |    13.2792 ms |  0.2004 ms |  0.1325 ms |    13.0795 ms |     5.10 KB |
```

* коммит: доработка поискового движка (добавлено хранение позиций токенов в индексе документов)
```
| Type                        | Method             | SearchType                 | Mean          | Error      | StdDev     | Min           | Allocated   |
|---------------------------- |------------------- |--------------------------- |--------------:|-----------:|-----------:|--------------:|------------:|
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFilter             |    45.3634 ms |  0.3740 ms |  0.2474 ms |    44.9224 ms |  2031.68 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFilter             |    46.2773 ms |  0.4001 ms |  0.2381 ms |    46.0166 ms |  8674.57 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMergeFilter        |    46.4073 ms |  0.2845 ms |  0.1693 ms |    46.2134 ms |  2031.68 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMergeFilter        |    47.9377 ms |  0.3531 ms |  0.2101 ms |    47.7174 ms | 10828.20 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFastFilter         |    50.3401 ms |  0.9098 ms |  0.5414 ms |    49.1990 ms |  2031.65 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFastFilter         |    50.3917 ms |  0.1288 ms |  0.0852 ms |    50.2491 ms |  9755.88 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOffsetFilter       |    61.8528 ms |  0.7559 ms |  0.4498 ms |    61.1761 ms | 16957.72 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOffsetFilter       |    62.9963 ms |  0.6053 ms |  0.4004 ms |    62.5836 ms | 30489.32 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinDirectOffsetFilter |    69.1736 ms |  0.5635 ms |  0.3354 ms |    68.7784 ms | 10828.24 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinDirectOffsetFilter |    69.4599 ms |  2.8788 ms |  1.9041 ms |    67.6473 ms |  2031.70 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      Legacy                |   721.3484 ms |  2.3844 ms |  1.4189 ms |   718.9910 ms |  2061.02 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFast               |   783.2577 ms | 20.1714 ms | 12.0037 ms |   760.7024 ms |  2002.96 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFast               |   798.1707 ms |  3.8452 ms |  2.5434 ms |   792.9545 ms | 57834.18 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOptimized          |   850.7294 ms |  5.8897 ms |  3.8957 ms |   846.1960 ms | 57834.18 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOptimized          |   871.8012 ms |  2.8952 ms |  1.9150 ms |   868.5109 ms |  2003.01 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinSimple             |   925.7923 ms | 12.3777 ms |  8.1871 ms |   908.9929 ms |  2060.74 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMerge              |   951.4626 ms |  6.5866 ms |  4.3566 ms |   939.9023 ms |  2032.04 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMerge              |   952.8544 ms |  1.5659 ms |  1.0357 ms |   950.8843 ms | 18482.63 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinDirectOffset       | 1,689.5011 ms | 24.9483 ms | 16.5018 ms | 1,657.8395 ms |  2031.99 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinDirectOffset       | 1,715.1356 ms |  8.1782 ms |  5.4094 ms | 1,706.9112 ms | 18482.91 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOffset             | 2,747.8063 ms | 40.3121 ms | 21.0840 ms | 2,696.2498 ms |  2003.05 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOffset             | 2,783.6697 ms |  4.2081 ms |  2.7834 ms | 2,780.3680 ms | 15534.73 KB |
|                             |                    |                            |               |            |            |               |             |
| QueryBenchmarkExtended      | QueryExtended      |      GinMergeFilter        |     0.0106 ms |  0.0001 ms |  0.0000 ms |     0.0106 ms |     5.46 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFilter             |     0.0108 ms |  0.0000 ms |  0.0000 ms |     0.0108 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMergeFilter        |     0.0109 ms |  0.0000 ms |  0.0000 ms |     0.0109 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFastFilter         |     0.0112 ms |  0.0000 ms |  0.0000 ms |     0.0112 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFilter             |     0.0118 ms |  0.0000 ms |  0.0000 ms |     0.0117 ms |    10.92 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFastFilter         |     0.0122 ms |  0.0000 ms |  0.0000 ms |     0.0121 ms |    11.15 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinDirectOffsetFilter |     0.0128 ms |  0.0001 ms |  0.0001 ms |     0.0127 ms |     5.46 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinDirectOffsetFilter |     0.0132 ms |  0.0002 ms |  0.0001 ms |     0.0129 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOffsetFilter       |     0.0198 ms |  0.0001 ms |  0.0001 ms |     0.0197 ms |     5.88 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOffsetFilter       |     0.0203 ms |  0.0001 ms |  0.0000 ms |     0.0202 ms |     6.43 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOffset             |     0.2452 ms |  0.0017 ms |  0.0011 ms |     0.2429 ms |     5.23 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOffset             |     0.2576 ms |  0.0013 ms |  0.0008 ms |     0.2565 ms |     4.69 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMerge              |     0.5288 ms |  0.0185 ms |  0.0123 ms |     0.5090 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinMerge              |     0.5461 ms |  0.0147 ms |  0.0088 ms |     0.5292 ms |     5.86 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinDirectOffset       |     0.6350 ms |  0.0136 ms |  0.0090 ms |     0.6195 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinDirectOffset       |     0.6414 ms |  0.0138 ms |  0.0091 ms |     0.6177 ms |     5.86 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFast               |     3.2417 ms |  0.0664 ms |  0.0395 ms |     3.1842 ms |     4.69 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOptimized          |     3.6110 ms |  0.0829 ms |  0.0549 ms |     3.5245 ms |     4.69 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFast               |     3.6866 ms |  0.0654 ms |  0.0433 ms |     3.6035 ms |   531.04 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOptimized          |     3.9295 ms |  0.0623 ms |  0.0412 ms |     3.8517 ms |   531.02 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinSimple             |    11.6781 ms |  0.2373 ms |  0.1570 ms |    11.3556 ms |     4.75 KB |
| QueryBenchmarkExtended      | QueryExtended      |      Legacy                |    17.5485 ms |  0.4611 ms |  0.2744 ms |    17.0777 ms |     4.76 KB |
|                             |                    |                            |               |            |            |               |             |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFastFilter         |    39.2068 ms |  0.2407 ms |  0.1592 ms |    39.0256 ms |  4221.19 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimizedFilter    |    39.6908 ms |  0.1634 ms |  0.0973 ms |    39.5290 ms |  4221.19 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMergeFilter        |    45.3514 ms |  0.1322 ms |  0.0875 ms |    45.1926 ms |  4221.20 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMergeFilter        |    45.4177 ms |  0.1878 ms |  0.1118 ms |    45.2590 ms |  8768.14 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimizedFilter    |    46.5897 ms |  0.1575 ms |  0.0937 ms |    46.4888 ms | 17137.86 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFastFilter         |    47.0136 ms |  0.1935 ms |  0.1152 ms |    46.8814 ms | 17138.05 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimized          |    83.9900 ms |  0.9092 ms |  0.6014 ms |    83.2268 ms |  4221.23 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFast               |    88.7941 ms |  0.4868 ms |  0.2897 ms |    88.1662 ms |  4221.28 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimized          |    98.2477 ms |  0.3680 ms |  0.1925 ms |    97.8453 ms | 70059.76 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFast               |   104.1880 ms |  0.6075 ms |  0.4018 ms |   103.6088 ms | 70864.50 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFilter             |   207.8912 ms |  0.8107 ms |  0.4824 ms |   207.2443 ms |  4221.29 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFilter             |   213.3541 ms |  3.6714 ms |  1.9202 ms |   208.9296 ms | 16132.57 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMerge              |   220.1975 ms |  0.8597 ms |  0.4497 ms |   219.6185 ms |  8746.52 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMerge              |   225.0566 ms |  0.7794 ms |  0.4638 ms |   223.8714 ms |  4221.29 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      Legacy                |   789.4884 ms |  5.7049 ms |  3.7735 ms |   779.3898 ms |  4279.62 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinSimple             | 2,312.1815 ms | 17.0885 ms | 10.1691 ms | 2,286.3213 ms |  4279.57 KB |
|                             |                    |                            |               |            |            |               |             |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMergeFilter        |     0.1089 ms |  0.0003 ms |  0.0002 ms |     0.1084 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMergeFilter        |     0.1119 ms |  0.0003 ms |  0.0002 ms |     0.1116 ms |     5.44 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimizedFilter    |     0.2098 ms |  0.0020 ms |  0.0013 ms |     0.2060 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFastFilter         |     0.2277 ms |  0.0002 ms |  0.0001 ms |     0.2275 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFast               |     0.2326 ms |  0.0023 ms |  0.0015 ms |     0.2282 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMerge              |     0.2347 ms |  0.0040 ms |  0.0027 ms |     0.2299 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMerge              |     0.2393 ms |  0.0007 ms |  0.0005 ms |     0.2386 ms |     5.47 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimizedFilter    |     0.2489 ms |  0.0009 ms |  0.0006 ms |     0.2483 ms |   188.06 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFastFilter         |     0.2553 ms |  0.0007 ms |  0.0004 ms |     0.2549 ms |   188.06 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimized          |     0.2623 ms |  0.0039 ms |  0.0026 ms |     0.2584 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFilter             |     0.5132 ms |  0.0185 ms |  0.0122 ms |     0.4939 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFast               |     0.5307 ms |  0.0155 ms |  0.0081 ms |     0.5110 ms |   662.51 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFilter             |     0.5467 ms |  0.0103 ms |  0.0068 ms |     0.5335 ms |   155.79 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimized          |     0.9097 ms |  0.0230 ms |  0.0152 ms |     0.8841 ms |  1372.39 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinSimple             |    11.1937 ms |  0.0579 ms |  0.0383 ms |    11.1327 ms |     5.10 KB |
| QueryBenchmarkReduced       | QueryReduced       |      Legacy                |    17.0558 ms |  0.3242 ms |  0.2144 ms |    16.7316 ms |     5.10 KB |
```

* коммит: доработка поискового движка (добавлен direct index спользующий FrozenDictionary)
```
| Type                        | Method             | SearchType                 | Mean          | Error      | StdDev     | Min           | Allocated   |
|---------------------------- |------------------- |--------------------------- |--------------:|-----------:|-----------:|--------------:|------------:|
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMergeFilter        |    37.5172 ms |  0.5316 ms |  0.3516 ms |    36.8612 ms |  2031.70 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFilter             |    38.2461 ms |  1.8538 ms |  1.2262 ms |    36.8470 ms |  2031.68 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFilter             |    38.4484 ms |  0.1472 ms |  0.0876 ms |    38.3439 ms |  8674.55 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFastFilter         |    39.4804 ms |  0.1164 ms |  0.0609 ms |    39.3683 ms |  2031.70 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMergeFilter        |    39.7654 ms |  0.2191 ms |  0.1304 ms |    39.6071 ms | 10828.24 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFastFilter         |    41.6333 ms |  0.3314 ms |  0.1972 ms |    41.3811 ms |  9755.87 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFrozenDirectFilter |    42.5653 ms |  0.6620 ms |  0.4379 ms |    42.2251 ms |  2031.65 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFrozenDirectFilter |    43.4794 ms |  0.4154 ms |  0.2748 ms |    43.0786 ms | 10828.31 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinDirectOffsetFilter |    44.1332 ms |  0.3578 ms |  0.2367 ms |    43.6573 ms |  2031.70 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinDirectOffsetFilter |    45.4021 ms |  0.3853 ms |  0.2015 ms |    45.0139 ms | 10828.20 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOffsetFilter       |    61.9064 ms |  1.0562 ms |  0.6285 ms |    61.1969 ms | 16957.69 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOffsetFilter       |    63.0697 ms |  0.7403 ms |  0.4897 ms |    62.3271 ms | 30489.36 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      Legacy                |   742.3334 ms |  1.7427 ms |  1.1527 ms |   740.2266 ms |  2061.40 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFast               |   789.5964 ms |  3.9947 ms |  2.0893 ms |   785.6902 ms |  2003.29 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFast               |   807.4498 ms |  3.4599 ms |  2.0589 ms |   803.7431 ms | 57834.23 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOptimized          |   826.1901 ms | 36.1015 ms | 23.8789 ms |   794.6837 ms |  2003.01 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOptimized          |   831.7380 ms |  8.6409 ms |  5.7154 ms |   816.2889 ms | 57833.85 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinSimple             |   932.6554 ms | 10.0593 ms |  6.6536 ms |   913.9878 ms |  2061.40 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMerge              |   943.0782 ms | 11.2133 ms |  6.6729 ms |   925.8545 ms | 18483.19 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMerge              |   952.6037 ms |  4.1389 ms |  2.7376 ms |   946.3535 ms |  2031.76 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFrozenDirect       | 1,278.0719 ms |  9.5508 ms |  5.6835 ms | 1,270.2404 ms | 18483.52 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFrozenDirect       | 1,280.2323 ms | 20.5741 ms | 13.6085 ms | 1,265.4866 ms |  2032.09 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinDirectOffset       | 1,745.3627 ms | 15.1186 ms |  8.9968 ms | 1,722.7237 ms | 18483.19 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinDirectOffset       | 1,786.0134 ms | 70.0349 ms | 46.3237 ms | 1,730.7900 ms |  2032.32 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOffset             | 2,749.4751 ms |  3.3847 ms |  2.2388 ms | 2,746.8973 ms |  2003.29 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOffset             | 2,754.8245 ms |  3.5230 ms |  2.3302 ms | 2,751.6108 ms | 15534.41 KB |
|                             |                    |                            |               |            |            |               |             |
| QueryBenchmarkExtended      | QueryExtended      |      GinFrozenDirectFilter |     0.0092 ms |  0.0001 ms |  0.0000 ms |     0.0092 ms |     5.46 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFrozenDirectFilter |     0.0093 ms |  0.0003 ms |  0.0002 ms |     0.0091 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinMergeFilter        |     0.0099 ms |  0.0000 ms |  0.0000 ms |     0.0099 ms |     5.46 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFilter             |     0.0102 ms |  0.0002 ms |  0.0001 ms |     0.0099 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMergeFilter        |     0.0102 ms |  0.0003 ms |  0.0002 ms |     0.0099 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinDirectOffsetFilter |     0.0104 ms |  0.0008 ms |  0.0005 ms |     0.0099 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinDirectOffsetFilter |     0.0106 ms |  0.0011 ms |  0.0007 ms |     0.0099 ms |     5.46 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFastFilter         |     0.0107 ms |  0.0002 ms |  0.0001 ms |     0.0105 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFilter             |     0.0110 ms |  0.0000 ms |  0.0000 ms |     0.0110 ms |    10.92 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFastFilter         |     0.0111 ms |  0.0000 ms |  0.0000 ms |     0.0111 ms |    11.15 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOffsetFilter       |     0.0203 ms |  0.0001 ms |  0.0001 ms |     0.0202 ms |     6.43 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOffsetFilter       |     0.0206 ms |  0.0005 ms |  0.0003 ms |     0.0202 ms |     5.88 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOffset             |     0.2397 ms |  0.0006 ms |  0.0004 ms |     0.2389 ms |     5.23 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOffset             |     0.2462 ms |  0.0038 ms |  0.0025 ms |     0.2409 ms |     4.69 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFrozenDirect       |     0.5064 ms |  0.0103 ms |  0.0068 ms |     0.4948 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFrozenDirect       |     0.5151 ms |  0.0082 ms |  0.0054 ms |     0.4998 ms |     5.86 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinDirectOffset       |     0.5257 ms |  0.0177 ms |  0.0105 ms |     0.5069 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMerge              |     0.5318 ms |  0.0117 ms |  0.0078 ms |     0.5212 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinDirectOffset       |     0.5363 ms |  0.0189 ms |  0.0125 ms |     0.5241 ms |     5.86 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinMerge              |     0.5511 ms |  0.0012 ms |  0.0007 ms |     0.5502 ms |     5.86 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFast               |     3.6807 ms |  0.1442 ms |  0.0954 ms |     3.5349 ms |   531.02 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFast               |     3.8360 ms |  0.2358 ms |  0.1560 ms |     3.5311 ms |     4.69 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOptimized          |     4.3536 ms |  0.4204 ms |  0.2780 ms |     4.0727 ms |   531.02 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOptimized          |     4.4959 ms |  0.4800 ms |  0.3175 ms |     4.0049 ms |     4.69 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinSimple             |    12.1237 ms |  0.2863 ms |  0.1894 ms |    11.7531 ms |     4.76 KB |
| QueryBenchmarkExtended      | QueryExtended      |      Legacy                |    19.4016 ms |  0.4534 ms |  0.2698 ms |    18.7207 ms |     4.76 KB |
|                             |                    |                            |               |            |            |               |             |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimizedFilter    |    40.8560 ms |  0.7956 ms |  0.5262 ms |    40.0504 ms |  4221.17 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFastFilter         |    41.3381 ms |  1.2604 ms |  0.8337 ms |    40.4464 ms |  4221.17 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMergeFilter        |    45.9266 ms |  0.1668 ms |  0.0872 ms |    45.8372 ms |  4221.23 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMergeFilter        |    47.8685 ms |  2.0764 ms |  1.3734 ms |    46.1184 ms |  8768.17 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimizedFilter    |    47.9634 ms |  1.3632 ms |  0.8112 ms |    46.9283 ms | 17137.86 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFastFilter         |    49.1890 ms |  5.8415 ms |  3.8638 ms |    46.0952 ms | 17138.03 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimized          |    86.1037 ms |  3.6912 ms |  2.4415 ms |    82.8835 ms |  4221.28 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFast               |    90.3204 ms |  2.0041 ms |  1.1926 ms |    88.3759 ms |  4221.28 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimized          |   104.3338 ms |  8.1658 ms |  5.4011 ms |    97.0605 ms | 70059.78 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFast               |   109.4320 ms |  9.8612 ms |  6.5226 ms |   103.9154 ms | 70864.48 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFilter             |   210.7030 ms |  3.9211 ms |  2.5935 ms |   207.1481 ms |  4221.31 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFilter             |   211.6016 ms |  3.7599 ms |  2.2375 ms |   207.5610 ms | 16132.49 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMerge              |   220.4534 ms |  5.8019 ms |  3.8376 ms |   214.9737 ms |  8746.52 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMerge              |   221.6498 ms |  4.2013 ms |  2.5001 ms |   216.2557 ms |  4221.39 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      Legacy                |   815.6551 ms | 20.0913 ms | 13.2891 ms |   793.5044 ms |  4279.34 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinSimple             | 2,404.3309 ms | 32.9089 ms | 21.7672 ms | 2,366.1506 ms |  4279.95 KB |
|                             |                    |                            |               |            |            |               |             |
| QueryBenchmarkReduced       | QueryReduced       |      GinMergeFilter        |     0.1132 ms |  0.0016 ms |  0.0009 ms |     0.1113 ms |     5.44 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMergeFilter        |     0.1133 ms |  0.0020 ms |  0.0012 ms |     0.1116 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimizedFilter    |     0.2104 ms |  0.0052 ms |  0.0027 ms |     0.2072 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFastFilter         |     0.2278 ms |  0.0051 ms |  0.0034 ms |     0.2238 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMerge              |     0.2348 ms |  0.0045 ms |  0.0027 ms |     0.2304 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMerge              |     0.2390 ms |  0.0065 ms |  0.0043 ms |     0.2321 ms |     5.47 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFast               |     0.2438 ms |  0.0042 ms |  0.0028 ms |     0.2402 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimizedFilter    |     0.2533 ms |  0.0031 ms |  0.0016 ms |     0.2509 ms |   188.07 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimized          |     0.2667 ms |  0.0057 ms |  0.0034 ms |     0.2612 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFastFilter         |     0.2732 ms |  0.0151 ms |  0.0100 ms |     0.2611 ms |   188.07 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFilter             |     0.5120 ms |  0.0092 ms |  0.0055 ms |     0.5040 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFast               |     0.5208 ms |  0.0178 ms |  0.0118 ms |     0.5031 ms |   662.50 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFilter             |     0.5636 ms |  0.0297 ms |  0.0196 ms |     0.5457 ms |   155.79 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimized          |     0.9094 ms |  0.0244 ms |  0.0162 ms |     0.8802 ms |  1372.38 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinSimple             |    11.4292 ms |  0.4317 ms |  0.2856 ms |    11.0860 ms |     5.09 KB |
| QueryBenchmarkReduced       | QueryReduced       |      Legacy                |    18.3291 ms |  0.4829 ms |  0.3194 ms |    17.8421 ms |     5.10 KB |
```

* коммит: доработка поискового движка (оптимизация extended алгоритмов - добавлена проверка по дополнительному индексу, удалены simple лгоритмы)
```
| Type                        | Method             | SearchType                 | Mean          | Error      | StdDev     | Min           | Allocated   |
|---------------------------- |------------------- |--------------------------- |--------------:|-----------:|-----------:|--------------:|------------:|
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMergeFilter        |    34.3640 ms |  0.2166 ms |  0.1433 ms |    34.1610 ms |  2031.69 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFrozenDirectFilter |    35.5386 ms |  0.2670 ms |  0.1396 ms |    35.3531 ms |  2031.66 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMergeFilter        |    35.9256 ms |  0.1576 ms |  0.0824 ms |    35.8098 ms | 10920.97 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinDirectOffsetFilter |    36.7644 ms |  0.2050 ms |  0.1220 ms |    36.5935 ms |  2031.68 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFrozenDirectFilter |    36.7889 ms |  0.5643 ms |  0.3358 ms |    36.4342 ms | 10920.97 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFilter             |    37.5670 ms |  0.2329 ms |  0.1386 ms |    37.3324 ms |  2031.70 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinDirectOffsetFilter |    37.7353 ms |  0.3276 ms |  0.2167 ms |    37.4389 ms | 10920.97 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFilter             |    39.0511 ms |  0.4333 ms |  0.2866 ms |    38.7507 ms |  8674.57 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFastFilter         |    39.6820 ms |  0.2637 ms |  0.1569 ms |    39.4795 ms |  2031.65 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFastFilter         |    41.0235 ms |  0.1847 ms |  0.1221 ms |    40.8212 ms |  9755.87 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOffsetFilter       |    59.9246 ms |  0.4270 ms |  0.2233 ms |    59.6331 ms | 16957.72 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOffsetFilter       |    62.9200 ms |  0.5012 ms |  0.3315 ms |    62.4604 ms | 30489.31 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      Legacy                |   740.9603 ms |  2.0514 ms |  1.0729 ms |   738.7639 ms |  2061.12 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFast               |   774.7893 ms |  2.9454 ms |  1.5405 ms |   772.3594 ms |  2003.34 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFast               |   789.9318 ms |  9.7028 ms |  5.7740 ms |   775.0847 ms | 57834.46 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMerge              |   945.7250 ms |  2.1726 ms |  1.2929 ms |   943.5432 ms | 18482.91 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMerge              |   950.2637 ms |  3.1893 ms |  1.8979 ms |   947.5857 ms |  2032.32 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFrozenDirect       | 1,217.6471 ms | 16.7668 ms | 11.0902 ms | 1,205.6677 ms | 18483.19 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFrozenDirect       | 1,222.2401 ms | 20.7131 ms | 13.7005 ms | 1,209.1159 ms |  2032.60 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinDirectOffset       | 1,679.4113 ms | 13.2906 ms |  8.7909 ms | 1,656.4046 ms |  2032.37 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinDirectOffset       | 1,708.9608 ms |  7.6226 ms |  5.0419 ms | 1,699.3074 ms | 18482.95 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOffset             | 2,622.7864 ms | 43.1637 ms | 28.5501 ms | 2,568.8542 ms | 15535.02 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOffset             | 2,631.0195 ms | 24.4551 ms | 16.1755 ms | 2,614.2994 ms |  2003.01 KB |
|                             |                    |                            |               |            |            |               |             |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFrozenDirectFilter |     0.0082 ms |  0.0000 ms |  0.0000 ms |     0.0082 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinDirectOffsetFilter |     0.0087 ms |  0.0000 ms |  0.0000 ms |     0.0086 ms |     5.46 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFrozenDirectFilter |     0.0087 ms |  0.0003 ms |  0.0002 ms |     0.0085 ms |     5.46 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinDirectOffsetFilter |     0.0090 ms |  0.0000 ms |  0.0000 ms |     0.0090 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMergeFilter        |     0.0092 ms |  0.0001 ms |  0.0001 ms |     0.0091 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinMergeFilter        |     0.0099 ms |  0.0006 ms |  0.0004 ms |     0.0095 ms |     5.46 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFilter             |     0.0101 ms |  0.0000 ms |  0.0000 ms |     0.0100 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFastFilter         |     0.0105 ms |  0.0000 ms |  0.0000 ms |     0.0104 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFastFilter         |     0.0113 ms |  0.0000 ms |  0.0000 ms |     0.0113 ms |    11.15 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFilter             |     0.0117 ms |  0.0006 ms |  0.0004 ms |     0.0112 ms |    10.92 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOffsetFilter       |     0.0184 ms |  0.0001 ms |  0.0001 ms |     0.0183 ms |     5.88 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOffsetFilter       |     0.0185 ms |  0.0001 ms |  0.0001 ms |     0.0184 ms |     6.43 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOffset             |     0.2305 ms |  0.0004 ms |  0.0002 ms |     0.2301 ms |     4.69 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOffset             |     0.2479 ms |  0.0009 ms |  0.0006 ms |     0.2473 ms |     5.24 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFrozenDirect       |     0.5099 ms |  0.0127 ms |  0.0084 ms |     0.4965 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinDirectOffset       |     0.5169 ms |  0.0193 ms |  0.0128 ms |     0.4968 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinDirectOffset       |     0.5209 ms |  0.0020 ms |  0.0014 ms |     0.5193 ms |     5.86 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFrozenDirect       |     0.5266 ms |  0.0136 ms |  0.0081 ms |     0.5129 ms |     5.86 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMerge              |     0.5394 ms |  0.0059 ms |  0.0039 ms |     0.5299 ms |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinMerge              |     0.5588 ms |  0.0199 ms |  0.0131 ms |     0.5393 ms |     5.86 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFast               |     3.3092 ms |  0.0769 ms |  0.0508 ms |     3.1695 ms |     4.69 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFast               |     3.7309 ms |  0.0514 ms |  0.0306 ms |     3.6504 ms |   531.02 KB |
| QueryBenchmarkExtended      | QueryExtended      |      Legacy                |    18.3388 ms |  0.4255 ms |  0.2814 ms |    17.9457 ms |     4.78 KB |
|                             |                    |                            |               |            |            |               |             |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFastFilter         |    39.2558 ms |  0.1630 ms |  0.1078 ms |    39.0839 ms |  4221.17 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimizedFilter    |    39.7435 ms |  0.1024 ms |  0.0678 ms |    39.6486 ms |  4221.22 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMergeFilter        |    45.7333 ms |  0.2128 ms |  0.1407 ms |    45.5397 ms |  8768.14 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMergeFilter        |    45.7616 ms |  0.3423 ms |  0.2264 ms |    45.5342 ms |  4221.23 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimizedFilter    |    46.2932 ms |  0.1763 ms |  0.1166 ms |    46.0901 ms | 17137.86 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFastFilter         |    46.5884 ms |  0.2246 ms |  0.1337 ms |    46.3658 ms | 17138.02 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimized          |    81.3926 ms |  0.3601 ms |  0.2382 ms |    80.9578 ms |  4221.26 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFast               |    87.7305 ms |  0.4958 ms |  0.3279 ms |    87.0589 ms |  4221.32 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimized          |    94.8718 ms |  0.3972 ms |  0.2627 ms |    94.5087 ms | 70059.78 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFast               |   103.8728 ms |  0.4201 ms |  0.2779 ms |   103.5178 ms | 70864.42 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFilter             |   206.5976 ms |  1.6045 ms |  0.9548 ms |   205.4582 ms |  4221.39 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFilter             |   211.4996 ms |  0.5843 ms |  0.3865 ms |   210.7724 ms | 16132.47 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMerge              |   218.5002 ms |  0.6887 ms |  0.4099 ms |   217.6461 ms |  8746.43 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMerge              |   220.1884 ms |  1.9848 ms |  1.3128 ms |   216.6721 ms |  4221.18 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      Legacy                |   776.4705 ms | 12.3504 ms |  7.3495 ms |   762.3752 ms |  4279.95 KB |
|                             |                    |                            |               |            |            |               |             |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMergeFilter        |     0.1103 ms |  0.0001 ms |  0.0001 ms |     0.1102 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMergeFilter        |     0.1108 ms |  0.0016 ms |  0.0011 ms |     0.1092 ms |     5.44 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimized          |     0.2083 ms |  0.0021 ms |  0.0014 ms |     0.2046 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimizedFilter    |     0.2205 ms |  0.0020 ms |  0.0013 ms |     0.2167 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFastFilter         |     0.2258 ms |  0.0002 ms |  0.0001 ms |     0.2255 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMerge              |     0.2336 ms |  0.0014 ms |  0.0009 ms |     0.2319 ms |     5.47 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMerge              |     0.2340 ms |  0.0021 ms |  0.0014 ms |     0.2310 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFast               |     0.2401 ms |  0.0022 ms |  0.0015 ms |     0.2360 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimizedFilter    |     0.2517 ms |  0.0019 ms |  0.0011 ms |     0.2496 ms |   188.06 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFastFilter         |     0.2590 ms |  0.0030 ms |  0.0016 ms |     0.2573 ms |   188.06 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFast               |     0.5130 ms |  0.0151 ms |  0.0100 ms |     0.4959 ms |   662.49 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFilter             |     0.5256 ms |  0.0165 ms |  0.0109 ms |     0.5040 ms |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFilter             |     0.5611 ms |  0.0095 ms |  0.0063 ms |     0.5503 ms |   155.79 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimized          |     0.8019 ms |  0.0161 ms |  0.0107 ms |     0.7890 ms |  1372.38 KB |
| QueryBenchmarkReduced       | QueryReduced       |      Legacy                |    17.3723 ms |  0.4685 ms |  0.3099 ms |    16.8853 ms |     5.10 KB |
```

* коммит: ...
