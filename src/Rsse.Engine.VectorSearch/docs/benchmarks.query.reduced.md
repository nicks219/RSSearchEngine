## Разработка и оптимизация алгортимов поиска сервиса RSSE

### Идеи и варианты реализации: **[Viktor Loshmanov](https://github.com/ViktorLoshmanov)**

---
### Результаты бенчмарков различных алгоритмов в однопоточном запуске на наборе запросов

* коммит: доработка поискового движка (оптимизация GinArrayDirect)
```
| Method        | SearchType                  | SearchQuery                                | Mean       | Error     | StdDev    | Min        | Allocated  |
|-------------- |---------------------------- |------------------------------------------- |-----------:|----------:|----------:|-----------:|-----------:|
| QueryReduced  |      GinFilter              | пляшем на                                  |  0.0042 ms | 0.0000 ms | 0.0000 ms |  0.0042 ms |    8.34 KB |
| QueryReduced  |      GinMergeFilter         | пляшем на                                  |  0.0045 ms | 0.0001 ms | 0.0000 ms |  0.0045 ms |    5.26 KB |
| QueryReduced  |      GinOptimizedFilter     | пляшем на                                  |  0.0049 ms | 0.0001 ms | 0.0000 ms |  0.0049 ms |    8.38 KB |
| QueryReduced  |      GinFastFilter          | пляшем на                                  |  0.0056 ms | 0.0001 ms | 0.0000 ms |  0.0056 ms |    8.98 KB |
| QueryReduced  |      GinMergeFilter         | b                                          |  0.0086 ms | 0.0002 ms | 0.0001 ms |  0.0085 ms |   22.30 KB |
| QueryReduced  |      GinMergeFilter         | b b b b                                    |  0.0087 ms | 0.0002 ms | 0.0001 ms |  0.0085 ms |   22.47 KB |
| QueryReduced  |      GinMergeFilter         | b b b b b b                                |  0.0088 ms | 0.0003 ms | 0.0002 ms |  0.0086 ms |   22.48 KB |
| QueryReduced  |      GinMerge               | b b b b                                    |  0.0089 ms | 0.0001 ms | 0.0001 ms |  0.0088 ms |   87.56 KB |
| QueryReduced  |      GinFastFilter          | b b b b b b                                |  0.0089 ms | 0.0001 ms | 0.0001 ms |  0.0088 ms |   22.48 KB |
| QueryReduced  |      GinFast                | b                                          |  0.0090 ms | 0.0005 ms | 0.0003 ms |  0.0086 ms |   22.30 KB |
| QueryReduced  |      GinMerge               | b                                          |  0.0090 ms | 0.0004 ms | 0.0002 ms |  0.0086 ms |   22.30 KB |
| QueryReduced  |      GinFastFilter          | b b b b                                    |  0.0090 ms | 0.0004 ms | 0.0002 ms |  0.0088 ms |   22.47 KB |
| QueryReduced  |      GinFastFilter          | b                                          |  0.0091 ms | 0.0005 ms | 0.0003 ms |  0.0086 ms |   22.30 KB |
| QueryReduced  |      GinMergeFilter         | b b b b b                                  |  0.0091 ms | 0.0005 ms | 0.0003 ms |  0.0086 ms |   22.48 KB |
| QueryReduced  |      GinFastFilter          | b b b b b                                  |  0.0092 ms | 0.0001 ms | 0.0001 ms |  0.0090 ms |   22.48 KB |
| QueryReduced  |      GinMerge               | b b b b b b                                |  0.0093 ms | 0.0002 ms | 0.0001 ms |  0.0091 ms |   22.48 KB |
| QueryReduced  |      GinFast                | b b b b b b                                |  0.0093 ms | 0.0003 ms | 0.0002 ms |  0.0090 ms |   22.48 KB |
| QueryReduced  |      GinFast                | b b b b                                    |  0.0093 ms | 0.0004 ms | 0.0003 ms |  0.0090 ms |   22.47 KB |
| QueryReduced  |      GinFast                | b b b b b                                  |  0.0095 ms | 0.0003 ms | 0.0002 ms |  0.0092 ms |   22.48 KB |
| QueryReduced  |      GinMerge               | b b b b b                                  |  0.0096 ms | 0.0005 ms | 0.0003 ms |  0.0089 ms |   22.48 KB |
| QueryReduced  |      GinOptimizedFilter     | b                                          |  0.0130 ms | 0.0004 ms | 0.0003 ms |  0.0127 ms |   38.02 KB |
| QueryReduced  |      GinOptimized           | b                                          |  0.0130 ms | 0.0003 ms | 0.0001 ms |  0.0127 ms |   37.90 KB |
| QueryReduced  |      GinOptimizedFilter     | b b b b                                    |  0.0131 ms | 0.0003 ms | 0.0002 ms |  0.0127 ms |   38.19 KB |
| QueryReduced  |      GinOptimizedFilter     | b b b b b                                  |  0.0132 ms | 0.0005 ms | 0.0003 ms |  0.0127 ms |   38.20 KB |
| QueryReduced  |      GinOptimized           | b b b b b b                                |  0.0133 ms | 0.0003 ms | 0.0002 ms |  0.0130 ms |   38.08 KB |
| QueryReduced  |      GinOptimized           | b b b b                                    |  0.0133 ms | 0.0004 ms | 0.0003 ms |  0.0129 ms |   38.07 KB |
| QueryReduced  |      GinOptimized           | b b b b b                                  |  0.0133 ms | 0.0004 ms | 0.0002 ms |  0.0129 ms |   38.08 KB |
| QueryReduced  |      GinOptimizedFilter     | b b b b b b                                |  0.0134 ms | 0.0004 ms | 0.0002 ms |  0.0130 ms |   38.20 KB |
| QueryReduced  |      GinFilter              | b b b b b                                  |  0.0175 ms | 0.0002 ms | 0.0001 ms |  0.0173 ms |   38.16 KB |
| QueryReduced  |      GinFilter              | b b b b                                    |  0.0175 ms | 0.0002 ms | 0.0001 ms |  0.0173 ms |   38.16 KB |
| QueryReduced  |      GinFilter              | b b b b b b                                |  0.0176 ms | 0.0003 ms | 0.0002 ms |  0.0173 ms |   38.16 KB |
| QueryReduced  |      GinFilter              | b                                          |  0.0180 ms | 0.0008 ms | 0.0005 ms |  0.0172 ms |   37.98 KB |
| QueryReduced  |      GinMergeFilter         | a b c d .,/#                               |  0.0366 ms | 0.0001 ms | 0.0001 ms |  0.0365 ms |   47.77 KB |
| QueryReduced  |      GinOptimizedFilter     | a b c d .,/#                               |  0.0400 ms | 0.0004 ms | 0.0003 ms |  0.0395 ms |   81.26 KB |
| QueryReduced  |      GinMerge               | a b c d .,/#                               |  0.0406 ms | 0.0011 ms | 0.0008 ms |  0.0397 ms |   47.66 KB |
| QueryReduced  |      GinFastFilter          | a b c d .,/#                               |  0.0470 ms | 0.0003 ms | 0.0002 ms |  0.0467 ms |  118.91 KB |
| QueryReduced  |      GinOptimized           | a b c d .,/#                               |  0.0484 ms | 0.0012 ms | 0.0007 ms |  0.0474 ms |  118.91 KB |
| QueryReduced  |      GinFast                | a b c d .,/#                               |  0.0488 ms | 0.0016 ms | 0.0010 ms |  0.0472 ms |   81.23 KB |
| QueryReduced  |      GinMergeFilter         | преключиться вдруг верный друг             |  0.1036 ms | 0.0012 ms | 0.0007 ms |  0.1025 ms |   10.77 KB |
| QueryReduced  |      GinMergeFilter         | приключится вдруг верный друг              |  0.1041 ms | 0.0006 ms | 0.0004 ms |  0.1033 ms |   10.77 KB |
| QueryReduced  |      GinMergeFilter         | приключится вдруг вот верный друг выручить |  0.1088 ms | 0.0008 ms | 0.0005 ms |  0.1080 ms |    5.44 KB |
| QueryReduced  |      GinFilter              | a b c d .,/#                               |  0.1113 ms | 0.0012 ms | 0.0007 ms |  0.1107 ms |   81.23 KB |
| QueryReduced  |      GinMerge               | приключится вдруг верный друг              |  0.1375 ms | 0.0009 ms | 0.0005 ms |  0.1368 ms |   10.66 KB |
| QueryReduced  |      GinMerge               | преключиться вдруг верный друг             |  0.1378 ms | 0.0012 ms | 0.0008 ms |  0.1360 ms |   10.66 KB |
| QueryReduced  |      GinMergeFilter         | чорт з ным зо сталом                       |  0.1555 ms | 0.0017 ms | 0.0011 ms |  0.1540 ms |   47.80 KB |
| QueryReduced  |      GinMerge               | пляшем на                                  |  0.2065 ms | 0.0036 ms | 0.0024 ms |  0.2025 ms |    5.14 KB |
| QueryReduced  |      GinOptimizedFilter     | приключится вдруг верный друг              |  0.2430 ms | 0.0023 ms | 0.0015 ms |  0.2411 ms |  193.40 KB |
| QueryReduced  |      GinOptimizedFilter     | чорт з ным зо сталом                       |  0.2430 ms | 0.0014 ms | 0.0008 ms |  0.2420 ms |  127.24 KB |
| QueryReduced  |      GinMerge               | приключится вдруг вот верный друг выручить |  0.2479 ms | 0.0068 ms | 0.0045 ms |  0.2390 ms |    5.47 KB |
| QueryReduced  |      GinOptimizedFilter     | преключиться вдруг верный друг             |  0.2482 ms | 0.0050 ms | 0.0033 ms |  0.2428 ms |  193.40 KB |
| QueryReduced  |      GinOptimizedFilter     | приключится вдруг вот верный друг выручить |  0.2561 ms | 0.0019 ms | 0.0013 ms |  0.2542 ms |  188.06 KB |
| QueryReduced  |      GinFastFilter          | приключится вдруг вот верный друг выручить |  0.2738 ms | 0.0065 ms | 0.0043 ms |  0.2680 ms |  188.07 KB |
| QueryReduced  |      GinFast                | преключиться вдруг верный друг             |  0.2762 ms | 0.0037 ms | 0.0024 ms |  0.2706 ms |  325.61 KB |
| QueryReduced  |      GinFast                | приключится вдруг верный друг              |  0.2883 ms | 0.0067 ms | 0.0044 ms |  0.2815 ms |  325.61 KB |
| QueryReduced  |      GinMerge               | чорт з ным зо сталом                       |  0.2976 ms | 0.0027 ms | 0.0018 ms |  0.2952 ms |   47.68 KB |
| QueryReduced  |      GinFast                | пляшем на                                  |  0.3096 ms | 0.0035 ms | 0.0021 ms |  0.3058 ms |    8.34 KB |
| QueryReduced  |      GinFilter              | чорт з ным зо сталом                       |  0.3151 ms | 0.0048 ms | 0.0032 ms |  0.3100 ms |  119.02 KB |
| QueryReduced  |      GinFastFilter          | чорт з ным зо сталом                       |  0.3161 ms | 0.0037 ms | 0.0024 ms |  0.3133 ms |  135.27 KB |
| QueryReduced  |      GinFastFilter          | преключиться вдруг верный друг             |  0.3687 ms | 0.0016 ms | 0.0010 ms |  0.3676 ms |  177.38 KB |
| QueryReduced  |      GinFastFilter          | приключится вдруг верный друг              |  0.3746 ms | 0.0016 ms | 0.0011 ms |  0.3734 ms |  177.38 KB |
| QueryReduced  |      GinOptimized           | преключиться вдруг верный друг             |  0.4775 ms | 0.0142 ms | 0.0094 ms |  0.4651 ms |  667.74 KB |
| QueryReduced  |      GinOptimized           | приключится вдруг верный друг              |  0.4793 ms | 0.0177 ms | 0.0117 ms |  0.4617 ms |  667.74 KB |
| QueryReduced  |      GinFilter              | приключится вдруг верный друг              |  0.4918 ms | 0.0040 ms | 0.0027 ms |  0.4880 ms |  161.13 KB |
| QueryReduced  |      GinFilter              | преключиться вдруг верный друг             |  0.4948 ms | 0.0030 ms | 0.0020 ms |  0.4923 ms |  161.13 KB |
| QueryReduced  |      GinFast                | приключится вдруг вот верный друг выручить |  0.5241 ms | 0.0146 ms | 0.0096 ms |  0.5107 ms |  662.50 KB |
| QueryReduced  |      GinMergeFilter         | ты шла по палубе в молчаний                |  0.5813 ms | 0.0051 ms | 0.0034 ms |  0.5739 ms |  100.74 KB |
| QueryReduced  |      GinFilter              | приключится вдруг вот верный друг выручить |  0.5856 ms | 0.0080 ms | 0.0053 ms |  0.5773 ms |  155.79 KB |
| QueryReduced  |      GinMergeFilter         | оно шла по палубе в молчаний               |  0.6733 ms | 0.0042 ms | 0.0028 ms |  0.6675 ms |  211.51 KB |
| QueryReduced  |      GinFast                | чорт з ным зо сталом                       |  0.7272 ms | 0.0126 ms | 0.0084 ms |  0.7128 ms |  704.86 KB |
| QueryReduced  |      GinOptimized           | приключится вдруг вот верный друг выручить |  0.8689 ms | 0.0171 ms | 0.0101 ms |  0.8535 ms | 1372.39 KB |
| QueryReduced  |      GinOptimizedFilter     | ты шла по палубе в молчаний                |  0.9251 ms | 0.0086 ms | 0.0057 ms |  0.9173 ms |  447.86 KB |
| QueryReduced  |      GinOptimizedFilter     | оно шла по палубе в молчаний               |  0.9937 ms | 0.0080 ms | 0.0053 ms |  0.9865 ms |  558.62 KB |
| QueryReduced  |      GinMerge               | ты шла по палубе в молчаний                |  0.9962 ms | 0.0065 ms | 0.0043 ms |  0.9898 ms |  100.78 KB |
| QueryReduced  |      GinFastFilter          | ты шла по палубе в молчаний                |  1.0231 ms | 0.0136 ms | 0.0090 ms |  1.0103 ms |  447.86 KB |
| QueryReduced  |      GinOptimized           | чорт з ным зо сталом                       |  1.0461 ms | 0.0334 ms | 0.0221 ms |  1.0168 ms | 1414.84 KB |
| QueryReduced  |      GinFastFilter          | оно шла по палубе в молчаний               |  1.0718 ms | 0.0099 ms | 0.0059 ms |  1.0635 ms |  558.62 KB |
| QueryReduced  |      GinFilter              | оно шла по палубе в молчаний               |  1.0916 ms | 0.0119 ms | 0.0071 ms |  1.0774 ms |  526.35 KB |
| QueryReduced  |      GinFilter              | ты шла по палубе в молчаний                |  1.1162 ms | 0.0289 ms | 0.0191 ms |  1.0871 ms |  415.58 KB |
| QueryReduced  |      GinMerge               | оно шла по палубе в молчаний               |  1.1701 ms | 0.0059 ms | 0.0035 ms |  1.1635 ms |  211.54 KB |
| QueryReduced  |      GinMerge               | удача с ними за столом                     |  1.4892 ms | 0.0288 ms | 0.0190 ms |  1.4507 ms |  441.80 KB |
| QueryReduced  |      GinMerge               | с ними за столом чёрт                      |  1.4952 ms | 0.0396 ms | 0.0262 ms |  1.4503 ms |  441.80 KB |
| QueryReduced  |      GinMerge               | чёрт с ними за столом                      |  1.4987 ms | 0.0332 ms | 0.0219 ms |  1.4761 ms |  441.80 KB |
| QueryReduced  |      GinOptimized           | пляшем на                                  |  1.5911 ms | 0.0290 ms | 0.0192 ms |  1.5547 ms | 2845.62 KB |
| QueryReduced  |      GinMergeFilter         | с ними за столом чёрт                      |  1.6922 ms | 0.0258 ms | 0.0170 ms |  1.6639 ms |  441.77 KB |
| QueryReduced  |      GinMergeFilter         | чёрт с ними за столом                      |  1.7045 ms | 0.0420 ms | 0.0278 ms |  1.6680 ms |  441.78 KB |
| QueryReduced  |      GinMergeFilter         | удача с ними за столом                     |  1.7151 ms | 0.0427 ms | 0.0282 ms |  1.6602 ms |  441.77 KB |
| QueryReduced  |      GinMerge               | пляшем на столе за детей                   |  1.7192 ms | 0.0246 ms | 0.0163 ms |  1.6839 ms |  441.80 KB |
| QueryReduced  |      GinMergeFilter         | пляшем на столе за детей                   |  1.8120 ms | 0.0325 ms | 0.0215 ms |  1.7757 ms |  441.77 KB |
| QueryReduced  |      GinOptimized           | ты шла по палубе в молчаний                |  2.1436 ms | 0.0782 ms | 0.0517 ms |  2.0289 ms | 2941.07 KB |
| QueryReduced  |      GinOptimized           | оно шла по палубе в молчаний               |  2.2165 ms | 0.0919 ms | 0.0608 ms |  2.1227 ms | 3051.81 KB |
| QueryReduced  |      GinFast                | чёрт с ними за столом                      |  2.2544 ms | 0.0954 ms | 0.0631 ms |  2.1403 ms | 1808.92 KB |
| QueryReduced  |      GinFast                | с ними за столом чёрт                      |  2.2601 ms | 0.0784 ms | 0.0519 ms |  2.1692 ms | 1808.90 KB |
| QueryReduced  |      GinFast                | удача с ними за столом                     |  2.2888 ms | 0.0799 ms | 0.0528 ms |  2.2095 ms | 1808.91 KB |
| QueryReduced  |      GinFast                | пляшем на столе за детей                   |  2.4264 ms | 0.0916 ms | 0.0606 ms |  2.3095 ms | 1808.90 KB |
| QueryReduced  |      GinFast                | ты шла по палубе в молчаний                |  2.4274 ms | 0.1039 ms | 0.0687 ms |  2.3267 ms | 2941.22 KB |
| QueryReduced  |      GinOptimized           | с ними за столом чёрт                      |  2.4430 ms | 0.0384 ms | 0.0201 ms |  2.4084 ms | 3282.03 KB |
| QueryReduced  |      GinFast                | оно шла по палубе в молчаний               |  2.4586 ms | 0.0844 ms | 0.0558 ms |  2.3891 ms | 3051.99 KB |
| QueryReduced  |      GinOptimized           | удача с ними за столом                     |  2.4917 ms | 0.0980 ms | 0.0648 ms |  2.3978 ms | 3282.04 KB |
| QueryReduced  |      GinOptimized           | чёрт с ними за столом                      |  2.5275 ms | 0.0480 ms | 0.0318 ms |  2.4790 ms | 3282.06 KB |
| QueryReduced  |      GinOptimized           | пляшем на столе за детей                   |  2.6359 ms | 0.0509 ms | 0.0336 ms |  2.5679 ms | 3282.04 KB |
| QueryReduced  |      GinOptimizedFilter     | пляшем на столе за детей                   |  2.7208 ms | 0.0160 ms | 0.0095 ms |  2.7016 ms | 1163.13 KB |
| QueryReduced  |      GinOptimizedFilter     | с ними за столом чёрт                      |  2.7962 ms | 0.0304 ms | 0.0201 ms |  2.7723 ms | 1163.14 KB |
| QueryReduced  |      GinOptimizedFilter     | удача с ними за столом                     |  2.8091 ms | 0.0365 ms | 0.0217 ms |  2.7624 ms | 1163.13 KB |
| QueryReduced  |      GinOptimizedFilter     | чёрт с ними за столом                      |  2.8148 ms | 0.0348 ms | 0.0207 ms |  2.7849 ms | 1163.13 KB |
| QueryReduced  |      GinFastFilter          | удача с ними за столом                     |  3.4937 ms | 0.0331 ms | 0.0219 ms |  3.4667 ms | 1227.17 KB |
| QueryReduced  |      GinFastFilter          | с ними за столом чёрт                      |  3.5078 ms | 0.0262 ms | 0.0173 ms |  3.4846 ms | 1227.17 KB |
| QueryReduced  |      GinFastFilter          | чёрт с ними за столом                      |  3.5162 ms | 0.0467 ms | 0.0309 ms |  3.4670 ms | 1227.15 KB |
| QueryReduced  |      GinFastFilter          | пляшем на столе за детей                   |  3.5235 ms | 0.0481 ms | 0.0318 ms |  3.4738 ms | 1227.17 KB |
| QueryReduced  |      GinFastFilter          | на                                         |  4.4458 ms | 0.1710 ms | 0.1131 ms |  4.3210 ms | 3977.15 KB |
| QueryReduced  |      GinMergeFilter         | на                                         |  4.4795 ms | 0.1292 ms | 0.0769 ms |  4.3597 ms | 3977.22 KB |
| QueryReduced  |      GinFilter              | пляшем на столе за детей                   |  4.5461 ms | 0.3619 ms | 0.2393 ms |  4.1589 ms | 1098.82 KB |
| QueryReduced  |      GinFast                | на                                         |  4.7471 ms | 0.3498 ms | 0.2081 ms |  4.4958 ms | 3977.16 KB |
| QueryReduced  |      GinMerge               | на                                         |  5.2311 ms | 0.4246 ms | 0.2809 ms |  4.8713 ms | 3977.16 KB |
| QueryReduced  |      GinMergeFilter         | я ты он она                                |  5.5631 ms | 0.2252 ms | 0.1178 ms |  5.3983 ms | 3977.41 KB |
| QueryReduced  |      GinMerge               | я ты он она                                |  5.5999 ms | 0.4026 ms | 0.2106 ms |  5.2811 ms | 3977.35 KB |
| QueryReduced  |      GinFilter              | удача с ними за столом                     |  5.9507 ms | 0.1733 ms | 0.1146 ms |  5.8079 ms | 1098.82 KB |
| QueryReduced  |      GinOptimized           | я ты он она                                |  6.3003 ms | 0.2710 ms | 0.1792 ms |  5.9744 ms | 6817.74 KB |
| QueryReduced  |      GinFilter              | чёрт с ними за столом                      |  6.4309 ms | 0.7914 ms | 0.5234 ms |  5.4887 ms | 1098.82 KB |
| QueryReduced  |      GinFilter              | с ними за столом чёрт                      |  6.4814 ms | 0.3357 ms | 0.1998 ms |  6.2363 ms | 1098.83 KB |
| QueryReduced  |      GinOptimizedFilter     | на                                         |  6.8719 ms | 0.3531 ms | 0.2335 ms |  6.5690 ms | 6817.86 KB |
| QueryReduced  |      GinOptimized           | на                                         |  6.8920 ms | 0.2369 ms | 0.1410 ms |  6.7488 ms | 6817.73 KB |
| QueryReduced  |      GinOptimizedFilter     | я ты он она                                |  7.6577 ms | 0.2081 ms | 0.1239 ms |  7.5015 ms | 6818.01 KB |
| QueryReduced  |      GinFast                | я ты он она                                |  8.2559 ms | 0.4488 ms | 0.2969 ms |  7.8864 ms | 6817.84 KB |
| QueryReduced  |      GinFastFilter          | я ты он она                                | 10.8448 ms | 0.6307 ms | 0.4172 ms | 10.3774 ms | 7330.38 KB |
| QueryReduced  |      Legacy                 | на                                         | 14.0636 ms | 0.3029 ms | 0.1584 ms | 13.8055 ms | 3977.13 KB |
| QueryReduced  |      Legacy                 | b b b b b                                  | 14.1392 ms | 0.6781 ms | 0.4485 ms | 13.4634 ms |   22.45 KB |
| QueryReduced  |      Legacy                 | b b b b b b                                | 14.4156 ms | 0.7010 ms | 0.4637 ms | 13.5453 ms |   22.47 KB |
| QueryReduced  |      Legacy                 | b b b b                                    | 14.8221 ms | 0.8894 ms | 0.5883 ms | 14.1732 ms |   22.46 KB |
| QueryReduced  |      Legacy                 | пляшем на                                  | 14.9526 ms | 1.4396 ms | 0.9522 ms | 14.1888 ms |    5.01 KB |
| QueryReduced  |      GinFilter              | я ты он она                                | 15.0422 ms | 0.3436 ms | 0.2044 ms | 14.6128 ms | 6818.01 KB |
| QueryReduced  |      GinFilter              | на                                         | 16.3849 ms | 1.0161 ms | 0.6721 ms | 15.5061 ms | 6817.90 KB |
| QueryReduced  |      Legacy                 | a b c d .,/#                               | 16.6259 ms | 1.2467 ms | 0.8246 ms | 15.6128 ms |   47.54 KB |
| QueryReduced  |      Legacy                 | b                                          | 17.5559 ms | 0.4401 ms | 0.2911 ms | 17.0182 ms |   22.27 KB |
| QueryReduced  |      Legacy                 | я ты он она                                | 17.5886 ms | 0.5138 ms | 0.3399 ms | 16.9767 ms | 3977.12 KB |
| QueryReduced  |      Legacy                 | пляшем на столе за детей                   | 17.7075 ms | 1.0004 ms | 0.6617 ms | 17.0235 ms |  441.43 KB |
| QueryReduced  |      Legacy                 | приключится вдруг верный друг              | 18.0119 ms | 0.3653 ms | 0.2174 ms | 17.7757 ms |   10.53 KB |
| QueryReduced  |      Legacy                 | преключиться вдруг верный друг             | 18.6767 ms | 0.7712 ms | 0.5101 ms | 17.5897 ms |   10.52 KB |
| QueryReduced  |      Legacy                 | чорт з ным зо сталом                       | 19.2241 ms | 1.0977 ms | 0.5741 ms | 17.8890 ms |   47.54 KB |
| QueryReduced  |      Legacy                 | оно шла по палубе в молчаний               | 19.2247 ms | 1.2373 ms | 0.8184 ms | 18.1139 ms |  211.17 KB |
| QueryReduced  |      Legacy                 | приключится вдруг вот верный друг выручить | 19.8615 ms | 0.8704 ms | 0.5757 ms | 19.0875 ms |    5.10 KB |
| QueryReduced  |      Legacy                 | чёрт с ними за столом                      | 19.8860 ms | 0.7840 ms | 0.5186 ms | 19.0715 ms |  441.42 KB |
| QueryReduced  |      Legacy                 | удача с ними за столом                     | 20.4329 ms | 0.7989 ms | 0.5285 ms | 19.7384 ms |  441.43 KB |
| QueryReduced  |      Legacy                 | ты шла по палубе в молчаний                | 20.4554 ms | 0.4053 ms | 0.2412 ms | 20.0100 ms |  100.41 KB |
| QueryReduced  |      Legacy                 | с ними за столом чёрт                      | 22.4536 ms | 0.5394 ms | 0.3568 ms | 21.7700 ms |  441.42 KB |
```

* коммит: ...
