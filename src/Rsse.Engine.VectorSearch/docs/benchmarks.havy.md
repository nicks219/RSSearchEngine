## Разработка и оптимизация алгортимов поиска сервиса RSSE

### Идеи и варианты реализации: **[Viktor Loshmanov](https://github.com/ViktorLoshmanov)**

---

| Type                    | Method       | SearchType                  | Mean           | Error       | StdDev      | Min            | Allocated      |
|------------------------ |------------- |---------------------------- |---------------:|------------:|------------:|---------------:|---------------:|
| QueryBenchmarkReduced   | QueryReduced |      GinArrayMergeFilter    |      0.9419 ms |   0.0149 ms |   0.0099 ms |      0.9285 ms |         0.5 KB |
| QueryBenchmarkReduced   | QueryReduced | Pool GinArrayMergeFilter    |      0.9642 ms |   0.0113 ms |   0.0075 ms |      0.9522 ms |              - |
| QueryBenchmarkReduced   | QueryReduced | Pool GinOptimizedFilter     |      2.3346 ms |   0.0249 ms |   0.0165 ms |      2.2910 ms |           0 KB |
| QueryBenchmarkReduced   | QueryReduced |      GinArrayDirect         |      2.4517 ms |   0.0268 ms |   0.0177 ms |      2.4252 ms |        0.53 KB |
| QueryBenchmarkReduced   | QueryReduced | Pool GinFastFilter          |      2.4616 ms |   0.0176 ms |   0.0105 ms |      2.4446 ms |           0 KB |
| QueryBenchmarkReduced   | QueryReduced | Pool GinArrayDirect         |      2.5481 ms |   0.0240 ms |   0.0159 ms |      2.5243 ms |           0 KB |
| QueryBenchmarkReduced   | QueryReduced |      GinOptimizedFilter     |      2.6377 ms |   0.0285 ms |   0.0170 ms |      2.6119 ms |     1623.98 KB |
| QueryBenchmarkReduced   | QueryReduced |      GinFastFilter          |      2.6926 ms |   0.0341 ms |   0.0226 ms |      2.6610 ms |     1623.97 KB |
| QueryBenchmarkReduced   | QueryReduced | Pool GinOptimized           |      3.0741 ms |   0.0240 ms |   0.0158 ms |      3.0445 ms |           0 KB |
| QueryBenchmarkReduced   | QueryReduced | Pool GinFast                |      3.2498 ms |   0.0517 ms |   0.0342 ms |      3.1934 ms |           0 KB |
| QueryBenchmarkReduced   | QueryReduced |      GinArrayDirectFilterLs |      3.7615 ms |   0.2497 ms |   0.1652 ms |      3.4911 ms |        0.39 KB |
| QueryBenchmarkReduced   | QueryReduced |      GinArrayDirectFilterHs |      4.1046 ms |   0.3752 ms |   0.2233 ms |      3.9403 ms |        0.39 KB |
| QueryBenchmarkReduced   | QueryReduced |      GinFast                |      5.2074 ms |   0.1344 ms |   0.0889 ms |      5.1040 ms |      5896.4 KB |
| QueryBenchmarkReduced   | QueryReduced |      GinArrayDirectFilterBs |      5.3400 ms |   1.0952 ms |   0.6517 ms |      4.6670 ms |        0.39 KB |
| QueryBenchmarkReduced   | QueryReduced | Pool GinArrayDirectFilterLs |      5.6117 ms |   0.2985 ms |   0.1777 ms |      5.2222 ms |           0 KB |
| QueryBenchmarkReduced   | QueryReduced |      GinOptimized           |      5.6268 ms |   0.1040 ms |   0.0619 ms |      5.5560 ms |    12233.08 KB |
| QueryBenchmarkReduced   | QueryReduced | Pool GinArrayDirectFilterBs |      6.2768 ms |   0.3012 ms |   0.1992 ms |      5.9226 ms |              - |
| QueryBenchmarkReduced   | QueryReduced | Pool GinArrayDirectFilterHs |      7.3910 ms |   0.5275 ms |   0.3489 ms |      6.8880 ms |           0 KB |
|                         |              |                             |                |             |             |                |                |
| MtQueryBenchmarkReduced | QueryReduced |      GinFast                |             NA |          NA |          NA |             NA |             NA |
| MtQueryBenchmarkReduced | QueryReduced | Pool GinArrayMergeFilter    |    183.8290 ms |   1.2041 ms |   0.7964 ms |    182.3904 ms |       565.9 KB |
| MtQueryBenchmarkReduced | QueryReduced |      GinArrayMergeFilter    |    190.4329 ms |   0.9152 ms |   0.6053 ms |    189.1889 ms |       911.2 KB |
| MtQueryBenchmarkReduced | QueryReduced | Pool GinArrayDirect         |    240.9440 ms |   3.3010 ms |   1.9644 ms |    238.3531 ms |      566.27 KB |
| MtQueryBenchmarkReduced | QueryReduced |      GinArrayDirect         |    250.8211 ms |   1.3717 ms |   0.7174 ms |    249.5478 ms |       885.8 KB |
| MtQueryBenchmarkReduced | QueryReduced | Pool GinOptimizedFilter     |    450.2140 ms |   2.3806 ms |   1.5746 ms |    447.2965 ms |      566.16 KB |
| MtQueryBenchmarkReduced | QueryReduced | Pool GinFast                |    475.6706 ms |   2.2218 ms |   1.4696 ms |    473.5518 ms |      565.88 KB |
| MtQueryBenchmarkReduced | QueryReduced | Pool GinFastFilter          |    513.5353 ms |   4.7143 ms |   3.1182 ms |    508.7909 ms |      566.12 KB |
| MtQueryBenchmarkReduced | QueryReduced |      GinArrayDirectFilterLs |    625.0293 ms |  12.2551 ms |   8.1060 ms |    612.6847 ms |      827.72 KB |
| MtQueryBenchmarkReduced | QueryReduced | Pool GinArrayDirectFilterLs |    629.8475 ms |   3.2794 ms |   1.7152 ms |    627.0065 ms |      566.12 KB |
| MtQueryBenchmarkReduced | QueryReduced | Pool GinOptimized           |    666.2992 ms |   1.7078 ms |   1.0163 ms |    664.2160 ms |    25942.19 KB |
| MtQueryBenchmarkReduced | QueryReduced | Pool GinArrayDirectFilterBs |    795.9443 ms |  15.0663 ms |   9.9654 ms |    784.0698 ms |      566.77 KB |
| MtQueryBenchmarkReduced | QueryReduced |      GinOptimizedFilter     |    798.0385 ms |  34.3584 ms |  22.7259 ms |    743.2789 ms |  4709923.07 KB |
| MtQueryBenchmarkReduced | QueryReduced |      GinArrayDirectFilterBs |    798.5647 ms |   8.4169 ms |   5.0088 ms |    789.4685 ms |      827.59 KB |
| MtQueryBenchmarkReduced | QueryReduced |      GinFastFilter          |    865.4180 ms |  46.1273 ms |  30.5104 ms |    782.1193 ms |  4978358.19 KB |
| MtQueryBenchmarkReduced | QueryReduced |      GinArrayDirectFilterHs |  1,051.4844 ms |  11.0018 ms |   7.2770 ms |  1,043.6249 ms |      828.19 KB |
| MtQueryBenchmarkReduced | QueryReduced | Pool GinArrayDirectFilterHs |  1,052.9537 ms |  15.6731 ms |   9.3268 ms |  1,037.6073 ms |      566.45 KB |
| MtQueryBenchmarkReduced | QueryReduced |      GinOptimized           |  2,306.2258 ms | 209.7952 ms | 138.7665 ms |  2,139.3520 ms | 15522802.51 KB |
|                         |              |                             |                |             |             |                |                |
| StQueryBenchmarkReduced | QueryReduced | Pool GinArrayMergeFilter    |  2,750.2070 ms |  17.2471 ms |  10.2635 ms |  2,732.1093 ms |      401.67 KB |
| StQueryBenchmarkReduced | QueryReduced |      GinArrayMergeFilter    |  2,845.9523 ms |  18.5842 ms |  11.0591 ms |  2,823.3855 ms |      747.24 KB |
| StQueryBenchmarkReduced | QueryReduced | Pool GinArrayDirect         |  3,139.6302 ms |  34.0001 ms |  22.4890 ms |  3,094.7414 ms |      401.67 KB |
| StQueryBenchmarkReduced | QueryReduced |      GinArrayDirect         |  3,256.6254 ms |  38.7793 ms |  25.6501 ms |  3,184.7230 ms |       720.6 KB |
| StQueryBenchmarkReduced | QueryReduced | Pool GinOptimized           |  4,149.3203 ms |  26.6839 ms |  17.6497 ms |  4,116.9083 ms |      401.95 KB |
| StQueryBenchmarkReduced | QueryReduced | Pool GinFast                |  4,278.7463 ms |  48.9600 ms |  32.3840 ms |  4,218.6486 ms |      401.91 KB |
| StQueryBenchmarkReduced | QueryReduced |      GinFast                |  6,142.0653 ms |  42.4942 ms |  28.1073 ms |  6,104.9494 ms |  8324347.66 KB |
| StQueryBenchmarkReduced | QueryReduced | Pool GinOptimizedFilter     |  7,596.6857 ms |  31.6242 ms |  20.9175 ms |  7,544.8946 ms |      401.95 KB |
| StQueryBenchmarkReduced | QueryReduced |      GinOptimized           |  8,320.7700 ms |  43.7359 ms |  28.9286 ms |  8,262.3755 ms |  15522410.1 KB |
| StQueryBenchmarkReduced | QueryReduced |      GinOptimizedFilter     |  8,482.5471 ms |  73.3092 ms |  48.4895 ms |  8,359.5692 ms |  4709558.08 KB |
| StQueryBenchmarkReduced | QueryReduced | Pool GinFastFilter          |  8,859.5691 ms |  72.4227 ms |  47.9031 ms |  8,761.9781 ms |      401.67 KB |
| StQueryBenchmarkReduced | QueryReduced |      GinFastFilter          |  9,887.1515 ms |  57.5064 ms |  38.0369 ms |  9,819.3856 ms |  4978099.42 KB |
| StQueryBenchmarkReduced | QueryReduced |      GinArrayDirectFilterLs | 12,631.5320 ms | 145.1743 ms |  96.0238 ms | 12,479.3058 ms |      662.84 KB |
| StQueryBenchmarkReduced | QueryReduced | Pool GinArrayDirectFilterLs | 12,875.1407 ms | 207.8870 ms | 137.5044 ms | 12,659.5079 ms |      402.23 KB |
| StQueryBenchmarkReduced | QueryReduced | Pool GinArrayDirectFilterBs | 17,255.3034 ms | 261.5097 ms | 172.9725 ms | 17,008.5281 ms |      402.23 KB |
| StQueryBenchmarkReduced | QueryReduced |      GinArrayDirectFilterBs | 17,363.9528 ms | 262.5791 ms | 173.6798 ms | 17,081.0319 ms |      662.23 KB |
| StQueryBenchmarkReduced | QueryReduced |      GinArrayDirectFilterHs | 20,012.8451 ms | 268.2693 ms | 177.4436 ms | 19,790.8178 ms |      662.84 KB |
| StQueryBenchmarkReduced | QueryReduced | Pool GinArrayDirectFilterHs | 20,240.0850 ms | 322.6531 ms | 213.4150 ms | 19,834.6815 ms |      401.63 KB |

| Method       | SearchType                  | SearchQuery                                | Mean       | Error     | StdDev    | Min        | Allocated |
|------------- |---------------------------- |------------------------------------------- |-----------:|----------:|----------:|-----------:|----------:|
| QueryReduced | Pool GinArrayDirectFilterLs | пляшем на                                  |  0.0101 ms | 0.0001 ms | 0.0000 ms |  0.0101 ms |         - |
| QueryReduced | Pool GinArrayDirectFilterLs | b b b b b                                  |  0.0125 ms | 0.0002 ms | 0.0001 ms |  0.0123 ms |         - |
| QueryReduced | Pool GinArrayDirectFilterLs | b b b b b b                                |  0.0125 ms | 0.0001 ms | 0.0000 ms |  0.0124 ms |         - |
| QueryReduced | Pool GinArrayDirectFilterLs | b                                          |  0.0125 ms | 0.0003 ms | 0.0002 ms |  0.0123 ms |         - |
| QueryReduced | Pool GinArrayDirectFilterLs | b b b b                                    |  0.0167 ms | 0.0004 ms | 0.0002 ms |  0.0163 ms |         - |
| QueryReduced | Pool GinArrayDirectFilterLs | a b c d .,/#                               |  0.4922 ms | 0.0173 ms | 0.0115 ms |  0.4691 ms |      0 KB |
| QueryReduced | Pool GinArrayDirectFilterLs | чорт з ным зо сталом                       |  1.4231 ms | 0.0225 ms | 0.0134 ms |  1.4064 ms |      0 KB |
| QueryReduced | Pool GinArrayDirectFilterLs | на                                         |  2.2574 ms | 0.0330 ms | 0.0218 ms |  2.2397 ms |      0 KB |
| QueryReduced | Pool GinArrayDirectFilterLs | приключится вдруг верный друг              |  3.0441 ms | 0.1155 ms | 0.0688 ms |  2.9532 ms |         - |
| QueryReduced | Pool GinArrayDirectFilterLs | преключиться вдруг верный друг             |  3.0802 ms | 0.1075 ms | 0.0711 ms |  2.9956 ms |      0 KB |
| QueryReduced | Pool GinArrayDirectFilterLs | приключится вдруг вот верный друг выручить |  3.5139 ms | 0.1797 ms | 0.0940 ms |  3.3725 ms |      0 KB |
| QueryReduced | Pool GinArrayDirectFilterLs | оно шла по палубе в молчаний               |  7.0757 ms | 0.3042 ms | 0.2012 ms |  6.7650 ms |      0 KB |
| QueryReduced | Pool GinArrayDirectFilterLs | ты шла по палубе в молчаний                |  7.6378 ms | 0.8932 ms | 0.5908 ms |  6.8294 ms |   0.01 KB |
| QueryReduced | Pool GinArrayDirectFilterLs | пляшем на столе за детей                   | 31.9493 ms | 0.8524 ms | 0.5638 ms | 30.9628 ms |   0.04 KB |
| QueryReduced | Pool GinArrayDirectFilterLs | чёрт с ними за столом                      | 34.5176 ms | 1.1384 ms | 0.7530 ms | 33.1669 ms |   0.05 KB |
| QueryReduced | Pool GinArrayDirectFilterLs | с ними за столом чёрт                      | 34.8124 ms | 0.9009 ms | 0.5959 ms | 34.0761 ms |   0.04 KB |
| QueryReduced | Pool GinArrayDirectFilterLs | удача с ними за столом                     | 35.8663 ms | 1.5505 ms | 1.0255 ms | 34.7561 ms |   0.05 KB |
| QueryReduced | Pool GinArrayDirectFilterLs | я ты он она                                | 39.0144 ms | 0.7264 ms | 0.4805 ms | 38.0602 ms |   0.03 KB |

| Method       | SearchType               | SearchQuery                                | Mean      | Error     | StdDev    | Min       | Allocated |
|------------- |------------------------- |------------------------------------------- |----------:|----------:|----------:|----------:|----------:|
| QueryReduced | Pool GinArrayMergeFilter | b b b b                                    | 0.0126 ms | 0.0003 ms | 0.0002 ms | 0.0122 ms |         - |
| QueryReduced | Pool GinArrayMergeFilter | b                                          | 0.0126 ms | 0.0001 ms | 0.0001 ms | 0.0125 ms |         - |
| QueryReduced | Pool GinArrayMergeFilter | b b b b b b                                | 0.0129 ms | 0.0001 ms | 0.0000 ms | 0.0128 ms |         - |
| QueryReduced | Pool GinArrayMergeFilter | b b b b b                                  | 0.0129 ms | 0.0001 ms | 0.0000 ms | 0.0128 ms |         - |
| QueryReduced | Pool GinArrayMergeFilter | пляшем на                                  | 0.0303 ms | 0.0004 ms | 0.0003 ms | 0.0300 ms |         - |
| QueryReduced | Pool GinArrayMergeFilter | a b c d .,/#                               | 0.1580 ms | 0.0009 ms | 0.0006 ms | 0.1570 ms |         - |
| QueryReduced | Pool GinArrayMergeFilter | приключится вдруг верный друг              | 0.9661 ms | 0.0086 ms | 0.0057 ms | 0.9593 ms |         - |
| QueryReduced | Pool GinArrayMergeFilter | преключиться вдруг верный друг             | 0.9952 ms | 0.0175 ms | 0.0116 ms | 0.9806 ms |      0 KB |
| QueryReduced | Pool GinArrayMergeFilter | приключится вдруг вот верный друг выручить | 1.0724 ms | 0.0140 ms | 0.0093 ms | 1.0473 ms |         - |
| QueryReduced | Pool GinArrayMergeFilter | чорт з ным зо сталом                       | 1.0810 ms | 0.0110 ms | 0.0065 ms | 1.0708 ms |      0 KB |
| QueryReduced | Pool GinArrayMergeFilter | на                                         | 2.2881 ms | 0.0181 ms | 0.0108 ms | 2.2688 ms |      0 KB |
| QueryReduced | Pool GinArrayMergeFilter | оно шла по палубе в молчаний               | 3.2295 ms | 0.0343 ms | 0.0227 ms | 3.2016 ms |         - |
| QueryReduced | Pool GinArrayMergeFilter | ты шла по палубе в молчаний                | 3.5381 ms | 0.0482 ms | 0.0287 ms | 3.4741 ms |         - |
| QueryReduced | Pool GinArrayMergeFilter | я ты он она                                | 5.7602 ms | 0.0544 ms | 0.0360 ms | 5.6824 ms |         - |
| QueryReduced | Pool GinArrayMergeFilter | удача с ними за столом                     | 6.5368 ms | 0.0687 ms | 0.0455 ms | 6.4656 ms |      0 KB |
| QueryReduced | Pool GinArrayMergeFilter | чёрт с ними за столом                      | 7.5099 ms | 0.0458 ms | 0.0273 ms | 7.4447 ms |      0 KB |
| QueryReduced | Pool GinArrayMergeFilter | с ними за столом чёрт                      | 7.5130 ms | 0.0816 ms | 0.0485 ms | 7.4390 ms |         - |
| QueryReduced | Pool GinArrayMergeFilter | пляшем на столе за детей                   | 7.8881 ms | 0.0638 ms | 0.0422 ms | 7.8287 ms |   0.01 KB |

* коммит: удача с ними за столом (x500)
Global total time: 00:50:12 (3012.17 sec), executed benchmarks: 32
| Type                     | Method        | SearchType                  | SearchQuery            | Mean        | Error     | StdDev    | Min         | Allocated   |
|------------------------- |-------------- |---------------------------- |----------------------- |------------:|----------:|----------:|------------:|------------:|
| StQueryBenchmarkReduced  | QueryReduced  |      GinArrayDirect         | удача с ними за столом |   5.1661 ms | 0.0764 ms | 0.0455 ms |   5.0617 ms |     4.26 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinArrayDirect         | удача с ними за столом |   6.0604 ms | 0.0819 ms | 0.0428 ms |   6.0051 ms |     0.01 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinArrayMergeFilter    | удача с ними за столом |   6.6738 ms | 0.0409 ms | 0.0271 ms |   6.6326 ms |        0 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinFast                | удача с ними за столом |   6.6845 ms | 0.0545 ms | 0.0285 ms |   6.6319 ms |        0 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinArrayMergeFilter    | удача с ними за столом |   7.1057 ms | 0.1185 ms | 0.0784 ms |   6.9569 ms |     4.01 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinFast                | удача с ними за столом |  13.3122 ms | 0.6845 ms | 0.4527 ms |  12.6444 ms | 20037.44 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinFastFilter          | удача с ними за столом |  18.2029 ms | 0.2475 ms | 0.1473 ms |  18.0037 ms |     0.01 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinFastFilter          | удача с ними за столом |  22.0842 ms | 0.3692 ms | 0.2442 ms |  21.6321 ms | 10630.98 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinArrayDirectFilterLs | удача с ними за столом |  36.3522 ms | 0.6258 ms | 0.4139 ms |  35.8221 ms |     0.01 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinArrayDirectFilterLs | удача с ними за столом |  37.1655 ms | 0.8023 ms | 0.5307 ms |  36.3859 ms |     3.12 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinArrayDirectFilterBs | удача с ними за столом |  48.2469 ms | 1.8182 ms | 1.2026 ms |  46.9794 ms |     3.15 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinArrayDirectFilterBs | удача с ними за столом |  48.6745 ms | 0.9312 ms | 0.6159 ms |  47.8861 ms |     0.01 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinArrayDirectFilterHs | удача с ними за столом |  55.3002 ms | 1.9348 ms | 1.2798 ms |  53.5497 ms |     0.01 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinArrayDirectFilterHs | удача с ними за столом |  56.0140 ms | 0.6196 ms | 0.4098 ms |  55.4969 ms |     3.12 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      Legacy                 | удача с ними за столом | 187.4666 ms | 4.0241 ms | 2.6617 ms | 184.6752 ms |     0.29 KB |
|                          |               |                             |                        |             |           |           |             |             |
| StQueryBenchmarkExtended | QueryExtended | Pool GinArrayDirectFilterBs | удача с ними за столом |   0.2007 ms | 0.0047 ms | 0.0031 ms |   0.1941 ms |     0.25 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinArrayDirectFilterLs | удача с ними за столом |   0.2029 ms | 0.0036 ms | 0.0024 ms |   0.1979 ms |     0.25 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinArrayDirectFilterBs | удача с ними за столом |   0.2040 ms | 0.0048 ms | 0.0032 ms |   0.1993 ms |     6.19 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinArrayDirectFilterLs | удача с ними за столом |   0.2067 ms | 0.0042 ms | 0.0025 ms |   0.2013 ms |     6.19 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinArrayDirectFilterHs | удача с ними за столом |   0.2555 ms | 0.0043 ms | 0.0028 ms |   0.2483 ms |     0.25 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinArrayDirectFilterHs | удача с ними за столом |   0.2644 ms | 0.0120 ms | 0.0079 ms |   0.2531 ms |     6.19 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinOffsetFilter        | удача с ними за столом |   0.7749 ms | 0.0166 ms | 0.0110 ms |   0.7542 ms |    13.94 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinOffsetFilter        | удача с ними за столом |   0.7772 ms | 0.0133 ms | 0.0088 ms |   0.7608 ms |     9.56 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinOffset              | удача с ними за столом |   6.6985 ms | 0.0397 ms | 0.0262 ms |   6.6594 ms |        0 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinOffset              | удача с ними за столом |   8.3463 ms | 0.1105 ms | 0.0731 ms |   8.2249 ms |     4.38 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinArrayDirectLs       | удача с ними за столом |  56.3665 ms | 0.8883 ms | 0.5875 ms |  55.2886 ms |     9.48 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinArrayDirectLs       | удача с ними за столом |  57.1312 ms | 1.0031 ms | 0.6635 ms |  56.2624 ms |     0.32 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinArrayDirectBs       | удача с ними за столом |  68.5783 ms | 0.6363 ms | 0.4209 ms |  67.8695 ms |     0.26 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinArrayDirectBs       | удача с ними за столом |  68.6727 ms | 1.6557 ms | 1.0951 ms |  66.7692 ms |     9.53 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinArrayDirectHs       | удача с ними за столом |  71.9112 ms | 0.6485 ms | 0.4290 ms |  71.0865 ms |     0.27 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinArrayDirectHs       | удача с ними за столом |  71.9685 ms | 1.1463 ms | 0.6822 ms |  70.4980 ms |     9.55 KB |
| StQueryBenchmarkExtended | QueryExtended |      Legacy                 | удача с ними за столом | 175.6056 ms | 3.1932 ms | 2.1121 ms | 172.4209 ms |     0.29 KB |