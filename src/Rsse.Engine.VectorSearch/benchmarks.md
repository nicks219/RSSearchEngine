## Разработка и оптимизация алгортимов поиска сервиса RSSE

### Идеи и варианты реализации: **[Viktor Loshmanov](https://github.com/ViktorLoshmanov)**

---
#### Результаты бенчмарков различных алгоритмов на запросе `приключится вдруг вот верный друг выручить`

Предыдущие бенчмарки использовали запрос "преключиться вдруг верный друг", **query extended** фактически измерял скорость отсечки.

* коммит `develop` 718507052ef0880d992c034e76e5d95e02ec5240
```
| Type                        | Method             | SearchType              | Mean          | Error      | StdDev     | Min           | Gen0      | Gen1     | Gen2    | Allocated   |
|---------------------------- |------------------- |------------------------ |--------------:|-----------:|-----------:|--------------:|----------:|---------:|--------:|------------:|
| QueryBenchmarkExtended      | QueryExtended      |      GinMergeFilter     |     0.0131 ms |  0.0002 ms |  0.0001 ms |     0.0129 ms |    0.2899 |   0.0153 |  0.0153 |    21.74 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMergeFilter     |     0.0132 ms |  0.0001 ms |  0.0001 ms |     0.0131 ms |    0.2289 |        - |       - |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFilter          |     0.0137 ms |  0.0001 ms |  0.0000 ms |     0.0136 ms |    0.2289 |        - |       - |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFastFilter      |     0.0147 ms |  0.0002 ms |  0.0001 ms |     0.0146 ms |    0.2289 |        - |       - |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFilter          |     0.0150 ms |  0.0002 ms |  0.0002 ms |     0.0149 ms |    0.6409 |   0.0153 |  0.0153 |           - |
| QueryBenchmarkExtended      | QueryExtended      |      GinFastFilter      |     0.0155 ms |  0.0003 ms |  0.0002 ms |     0.0153 ms |    0.5493 |        - |       - |    11.15 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinMerge           |     0.4359 ms |  0.0086 ms |  0.0051 ms |     0.4268 ms |         - |        - |       - |     4.72 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinMerge           |     0.4402 ms |  0.0124 ms |  0.0065 ms |     0.4324 ms |         - |        - |       - |     5.86 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinFast            |     5.3847 ms |  0.1052 ms |  0.0626 ms |     5.2940 ms |         - |        - |       - |     4.69 KB |
| QueryBenchmarkExtended      | QueryExtended      | Pool GinOptimized       |     5.7069 ms |  0.1231 ms |  0.0733 ms |     5.5948 ms |         - |        - |       - |     4.69 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinFast            |     5.7530 ms |  0.2036 ms |  0.1347 ms |     5.5509 ms |   15.6250 |   7.8125 |  7.8125 |   531.03 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinOptimized       |     6.0692 ms |  0.1230 ms |  0.0813 ms |     5.9322 ms |   15.6250 |   7.8125 |  7.8125 |   531.06 KB |
| QueryBenchmarkExtended      | QueryExtended      |      Legacy             |    12.7316 ms |  0.1699 ms |  0.1124 ms |    12.5219 ms |         - |        - |       - |     4.75 KB |
| QueryBenchmarkExtended      | QueryExtended      |      GinSimple          |    14.0396 ms |  0.2110 ms |  0.1256 ms |    13.9092 ms |         - |        - |       - |     4.75 KB |
|                             |                    |                         |               |            |            |               |           |          |         |             |
| QueryBenchmarkReduced       | QueryReduced       |      GinMergeFilter     |     0.1262 ms |  0.0025 ms |  0.0013 ms |     0.1241 ms |    0.2441 |        - |       - |     5.44 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMergeFilter     |     0.1269 ms |  0.0016 ms |  0.0010 ms |     0.1257 ms |    0.2441 |        - |       - |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinMerge           |     0.2334 ms |  0.0031 ms |  0.0020 ms |     0.2299 ms |    0.2441 |        - |       - |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinMerge           |     0.2338 ms |  0.0066 ms |  0.0039 ms |     0.2283 ms |    0.2441 |        - |       - |     5.47 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFastFilter      |     0.2557 ms |  0.0040 ms |  0.0024 ms |     0.2513 ms |         - |        - |       - |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimizedFilter |     0.2592 ms |  0.0174 ms |  0.0115 ms |     0.2464 ms |         - |        - |       - |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimizedFilter |     0.2775 ms |  0.0055 ms |  0.0029 ms |     0.2737 ms |    0.9766 |        - |       - |   188.06 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFast            |     0.2899 ms |  0.0041 ms |  0.0024 ms |     0.2876 ms |         - |        - |       - |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFastFilter      |     0.2917 ms |  0.0076 ms |  0.0040 ms |     0.2856 ms |    8.7891 |   0.4883 |       - |   188.07 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinOptimized       |     0.2985 ms |  0.0058 ms |  0.0038 ms |     0.2936 ms |         - |        - |       - |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFast            |     0.6651 ms |  0.0178 ms |  0.0106 ms |     0.6518 ms |   21.4844 |  10.7422 |  8.7891 |   662.53 KB |
| QueryBenchmarkReduced       | QueryReduced       | Pool GinFilter          |     0.7527 ms |  0.0394 ms |  0.0260 ms |     0.7093 ms |         - |        - |       - |     5.03 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinFilter          |     0.8266 ms |  0.0393 ms |  0.0260 ms |     0.7682 ms |    7.8125 |        - |       - |   155.79 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinOptimized       |     1.1627 ms |  0.0333 ms |  0.0221 ms |     1.1321 ms |   37.1094 |  23.4375 | 23.4375 |  1372.41 KB |
| QueryBenchmarkReduced       | QueryReduced       |      Legacy             |    14.8304 ms |  0.1510 ms |  0.0999 ms |    14.6958 ms |         - |        - |       - |     5.09 KB |
| QueryBenchmarkReduced       | QueryReduced       |      GinSimple          |    15.8073 ms |  0.2446 ms |  0.1455 ms |    15.5902 ms |         - |        - |       - |      5.1 KB |
|                             |                    |                         |               |            |            |               |           |          |         |             |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFilter          |    51.3142 ms |  0.6863 ms |  0.4084 ms |    50.7357 ms |  100.0000 |        - |       - |  2031.69 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMergeFilter     |    52.0316 ms |  0.6524 ms |  0.3882 ms |    51.5118 ms |  100.0000 |        - |       - |  2031.68 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMergeFilter     |    53.7202 ms |  1.1177 ms |  0.7393 ms |    52.7601 ms |  500.0000 |        - |       - | 10857.56 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFastFilter      |    55.4568 ms |  1.8922 ms |  1.1260 ms |    54.5238 ms |         - |        - |       - |  2031.69 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFilter          |    56.5733 ms |  3.0068 ms |  1.9888 ms |    54.1768 ms |  333.3333 |        - |       - |  8674.65 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFastFilter      |    61.8140 ms | 11.1236 ms |  7.3576 ms |    53.8930 ms |  500.0000 |        - |       - |  9756.13 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      Legacy             |   857.1931 ms | 11.4070 ms |  6.7881 ms |   846.4382 ms |         - |        - |       - |  2061.07 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinFast            |   860.8807 ms |  5.1591 ms |  3.0701 ms |   855.2492 ms | 2000.0000 |        - |       - | 57835.02 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinFast            |   907.5941 ms | 17.3309 ms | 11.4633 ms |   897.3741 ms |         - |        - |       - |  2003.01 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinOptimized       |   921.6986 ms | 12.2325 ms |  8.0910 ms |   909.9787 ms |         - |        - |       - |  2003.01 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinOptimized       |   952.0759 ms |  6.0496 ms |  3.6000 ms |   946.7870 ms | 2000.0000 |        - |       - | 57834.98 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended | Pool GinMerge           | 1,039.3272 ms | 16.2452 ms | 10.7452 ms | 1,022.7398 ms |         - |        - |       - |  2032.37 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinMerge           | 1,050.4241 ms | 17.4550 ms | 11.5454 ms | 1,033.9952 ms |         - |        - |       - | 18482.58 KB |
| DuplicatesBenchmarkExtended | DuplicatesExtended |      GinSimple          | 1,078.7219 ms | 12.9481 ms |  8.5644 ms | 1,060.1420 ms |         - |        - |       - |  2061.12 KB |
|                             |                    |                         |               |            |            |               |           |          |         |             |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimizedFilter |    46.6740 ms |  1.0039 ms |  0.5974 ms |    45.9162 ms |  181.8182 |        - |       - |   4221.2 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFastFilter      |    48.0890 ms |  3.2896 ms |  2.1759 ms |    46.3085 ms |  181.8182 |        - |       - |  4221.17 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMergeFilter     |    51.1651 ms |  0.9576 ms |  0.6334 ms |    50.2518 ms |  200.0000 |        - |       - |  4221.17 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFastFilter      |    52.1482 ms |  0.6116 ms |  0.4045 ms |    51.7142 ms |  800.0000 |        - |       - | 17138.42 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMergeFilter     |    53.4324 ms |  1.1397 ms |  0.5961 ms |    52.6376 ms |  400.0000 |        - |       - |  8768.25 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimizedFilter |    56.1216 ms |  1.6330 ms |  1.0801 ms |    54.3079 ms |  777.7778 |        - |       - | 17138.22 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinOptimized       |   105.4645 ms |  3.8482 ms |  2.2900 ms |   102.2247 ms |  200.0000 |        - |       - |   4221.3 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFast            |   110.5170 ms |  5.8835 ms |  3.8916 ms |   104.8436 ms |  200.0000 |        - |       - |  4221.23 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFast            |   115.4327 ms |  1.1920 ms |  0.7094 ms |   114.6280 ms | 1600.0000 |        - |       - | 70864.69 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinOptimized       |   121.8067 ms |  4.5471 ms |  3.0076 ms |   117.7309 ms | 3500.0000 | 250.0000 |       - |  70060.3 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinFilter          |   250.0893 ms |  2.3153 ms |  1.3778 ms |   248.1254 ms |  666.6667 |        - |       - | 16132.75 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinFilter          |   266.6661 ms |  5.0722 ms |  3.3549 ms |   262.4765 ms |         - |        - |       - |   4221.2 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinMerge           |   273.8731 ms |  6.0585 ms |  3.1687 ms |   267.9196 ms |         - |        - |       - |  8746.63 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  | Pool GinMerge           |   284.1298 ms |  5.3427 ms |  3.5339 ms |   279.8877 ms |         - |        - |       - |   4221.2 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      Legacy             | 1,039.8626 ms | 40.1967 ms | 26.5876 ms | 1,010.1306 ms |         - |        - |       - |  4279.29 KB |
| DuplicatesBenchmarkReduced  | DuplicatesReduced  |      GinSimple          | 2,939.2779 ms | 62.1003 ms | 36.9549 ms | 2,896.4628 ms |         - |        - |       - |  4279.29 KB |
```
