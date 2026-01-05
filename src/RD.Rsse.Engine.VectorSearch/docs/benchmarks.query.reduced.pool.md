## Разработка и оптимизация алгортимов поиска сервиса RSSE

### Идеи и варианты реализации: **[Viktor Loshmanov](https://github.com/ViktorLoshmanov)**

---
### Результаты бенчмарков различных алгоритмов в однопоточном запуске на наборе запросов

* коммит: доработка поискового движка (оптимизация GinArrayDirect)
```
| Method       | SearchType              | SearchQuery                                | Mean       | Error     | StdDev    | Min        | Allocated  |
|------------- |------------------------ |------------------------------------------- |-----------:|----------:|----------:|-----------:|-----------:|
| QueryReduced | Pool GinFilter          | пляшем на                                  |  0.0038 ms | 0.0000 ms | 0.0000 ms |  0.0038 ms |    4.94 KB |
| QueryReduced | Pool GinMergeFilter     | пляшем на                                  |  0.0047 ms | 0.0000 ms | 0.0000 ms |  0.0047 ms |    4.94 KB |
| QueryReduced | Pool GinOptimizedFilter | пляшем на                                  |  0.0048 ms | 0.0001 ms | 0.0000 ms |  0.0047 ms |    4.94 KB |
| QueryReduced | Pool GinFastFilter      | пляшем на                                  |  0.0051 ms | 0.0000 ms | 0.0000 ms |  0.0051 ms |    4.94 KB |
| QueryReduced | Pool GinMergeFilter     | b b b b b b                                |  0.0086 ms | 0.0001 ms | 0.0001 ms |  0.0085 ms |   22.39 KB |
| QueryReduced | Pool GinMergeFilter     | b b b b b                                  |  0.0088 ms | 0.0003 ms | 0.0002 ms |  0.0086 ms |   87.25 KB |
| QueryReduced | Pool GinMergeFilter     | b                                          |  0.0089 ms | 0.0005 ms | 0.0003 ms |  0.0085 ms |   22.21 KB |
| QueryReduced | Pool GinMerge           | b b b b                                    |  0.0089 ms | 0.0002 ms | 0.0001 ms |  0.0087 ms |   22.38 KB |
| QueryReduced | Pool GinMerge           | b b b b b                                  |  0.0089 ms | 0.0002 ms | 0.0001 ms |  0.0088 ms |   22.39 KB |
| QueryReduced | Pool GinFast            | b                                          |  0.0091 ms | 0.0004 ms | 0.0002 ms |  0.0085 ms |   22.21 KB |
| QueryReduced | Pool GinFastFilter      | b b b b b                                  |  0.0091 ms | 0.0004 ms | 0.0002 ms |  0.0090 ms |   22.39 KB |
| QueryReduced | Pool GinMerge           | b                                          |  0.0093 ms | 0.0004 ms | 0.0003 ms |  0.0088 ms |   22.21 KB |
| QueryReduced | Pool GinFastFilter      | b                                          |  0.0093 ms | 0.0003 ms | 0.0002 ms |  0.0091 ms |   22.21 KB |
| QueryReduced | Pool GinFastFilter      | b b b b                                    |  0.0094 ms | 0.0005 ms | 0.0003 ms |  0.0090 ms |   22.38 KB |
| QueryReduced | Pool GinFast            | b b b b b                                  |  0.0094 ms | 0.0004 ms | 0.0002 ms |  0.0089 ms |   22.39 KB |
| QueryReduced | Pool GinMerge           | b b b b b b                                |  0.0094 ms | 0.0002 ms | 0.0001 ms |  0.0093 ms |   82.27 KB |
| QueryReduced | Pool GinFast            | b b b b                                    |  0.0094 ms | 0.0003 ms | 0.0002 ms |  0.0092 ms |   22.38 KB |
| QueryReduced | Pool GinFast            | b b b b b b                                |  0.0095 ms | 0.0002 ms | 0.0001 ms |  0.0093 ms |   22.39 KB |
| QueryReduced | Pool GinFastFilter      | b b b b b b                                |  0.0096 ms | 0.0003 ms | 0.0002 ms |  0.0092 ms |   22.39 KB |
| QueryReduced | Pool GinOptimizedFilter | b                                          |  0.0102 ms | 0.0002 ms | 0.0001 ms |  0.0101 ms |   81.12 KB |
| QueryReduced | Pool GinOptimizedFilter | b b b b b b                                |  0.0103 ms | 0.0002 ms | 0.0001 ms |  0.0101 ms |    5.23 KB |
| QueryReduced | Pool GinOptimizedFilter | b b b b b                                  |  0.0103 ms | 0.0003 ms | 0.0002 ms |  0.0101 ms |    5.23 KB |
| QueryReduced | Pool GinOptimizedFilter | b b b b                                    |  0.0105 ms | 0.0006 ms | 0.0004 ms |  0.0100 ms |   22.38 KB |
| QueryReduced | Pool GinOptimized       | b b b b b                                  |  0.0107 ms | 0.0005 ms | 0.0003 ms |  0.0103 ms |   22.39 KB |
| QueryReduced | Pool GinOptimized       | b b b b                                    |  0.0107 ms | 0.0004 ms | 0.0002 ms |  0.0104 ms |   22.38 KB |
| QueryReduced | Pool GinOptimized       | b                                          |  0.0109 ms | 0.0006 ms | 0.0003 ms |  0.0103 ms |   22.21 KB |
| QueryReduced | Pool GinOptimized       | b b b b b b                                |  0.0111 ms | 0.0007 ms | 0.0004 ms |  0.0104 ms |   22.39 KB |
| QueryReduced | Pool GinMergeFilter     | b b b b                                    |  0.0125 ms | 0.0001 ms | 0.0001 ms |  0.0123 ms |   22.38 KB |
| QueryReduced | Pool GinFilter          | b                                          |  0.0145 ms | 0.0001 ms | 0.0001 ms |  0.0143 ms |   16.79 KB |
| QueryReduced | Pool GinFilter          | b b b b b                                  |  0.0146 ms | 0.0001 ms | 0.0001 ms |  0.0144 ms |   22.39 KB |
| QueryReduced | Pool GinFilter          | b b b b                                    |  0.0147 ms | 0.0001 ms | 0.0001 ms |  0.0145 ms |   22.38 KB |
| QueryReduced | Pool GinFilter          | b b b b b b                                |  0.0153 ms | 0.0012 ms | 0.0008 ms |  0.0146 ms |   22.39 KB |
| QueryReduced | Pool GinOptimized       | a b c d .,/#                               |  0.0345 ms | 0.0003 ms | 0.0002 ms |  0.0341 ms |   47.45 KB |
| QueryReduced | Pool GinOptimizedFilter | a b c d .,/#                               |  0.0365 ms | 0.0006 ms | 0.0004 ms |  0.0358 ms |   47.45 KB |
| QueryReduced | Pool GinMergeFilter     | a b c d .,/#                               |  0.0380 ms | 0.0001 ms | 0.0001 ms |  0.0379 ms |   47.45 KB |
| QueryReduced | Pool GinMerge           | a b c d .,/#                               |  0.0410 ms | 0.0001 ms | 0.0001 ms |  0.0409 ms |   47.45 KB |
| QueryReduced | Pool GinFast            | a b c d .,/#                               |  0.0416 ms | 0.0004 ms | 0.0002 ms |  0.0414 ms |   47.45 KB |
| QueryReduced | Pool GinFastFilter      | a b c d .,/#                               |  0.0419 ms | 0.0001 ms | 0.0001 ms |  0.0418 ms |   47.45 KB |
| QueryReduced | Pool GinFilter          | a b c d .,/#                               |  0.0979 ms | 0.0008 ms | 0.0006 ms |  0.0974 ms |   47.45 KB |
| QueryReduced | Pool GinMergeFilter     | приключится вдруг верный друг              |  0.1026 ms | 0.0011 ms | 0.0007 ms |  0.1018 ms |   10.45 KB |
| QueryReduced | Pool GinMergeFilter     | преключиться вдруг верный друг             |  0.1027 ms | 0.0008 ms | 0.0005 ms |  0.1019 ms |   10.45 KB |
| QueryReduced | Pool GinMergeFilter     | приключится вдруг вот верный друг выручить |  0.1097 ms | 0.0016 ms | 0.0011 ms |  0.1082 ms |    5.03 KB |
| QueryReduced | Pool GinMerge           | приключится вдруг верный друг              |  0.1415 ms | 0.0027 ms | 0.0016 ms |  0.1389 ms |   10.45 KB |
| QueryReduced | Pool GinMerge           | преключиться вдруг верный друг             |  0.1445 ms | 0.0023 ms | 0.0015 ms |  0.1409 ms |   10.45 KB |
| QueryReduced | Pool GinMergeFilter     | чорт з ным зо сталом                       |  0.1529 ms | 0.0009 ms | 0.0006 ms |  0.1520 ms |   47.48 KB |
| QueryReduced | Pool GinOptimized       | приключится вдруг верный друг              |  0.1649 ms | 0.0013 ms | 0.0009 ms |  0.1631 ms |   10.45 KB |
| QueryReduced | Pool GinOptimized       | преключиться вдруг верный друг             |  0.1656 ms | 0.0010 ms | 0.0007 ms |  0.1647 ms |   10.45 KB |
| QueryReduced | Pool GinFast            | преключиться вдруг верный друг             |  0.1765 ms | 0.0007 ms | 0.0004 ms |  0.1759 ms |   10.45 KB |
| QueryReduced | Pool GinFast            | приключится вдруг верный друг              |  0.1774 ms | 0.0039 ms | 0.0026 ms |  0.1728 ms |   10.45 KB |
| QueryReduced | Pool GinMerge           | пляшем на                                  |  0.2026 ms | 0.0016 ms | 0.0011 ms |  0.2008 ms |    4.94 KB |
| QueryReduced | Pool GinOptimizedFilter | преключиться вдруг верный друг             |  0.2107 ms | 0.0014 ms | 0.0009 ms |  0.2094 ms |   10.45 KB |
| QueryReduced | Pool GinOptimizedFilter | приключится вдруг верный друг              |  0.2140 ms | 0.0029 ms | 0.0019 ms |  0.2115 ms |   10.45 KB |
| QueryReduced | Pool GinOptimized       | приключится вдруг вот верный друг выручить |  0.2173 ms | 0.0019 ms | 0.0012 ms |  0.2146 ms |    5.03 KB |
| QueryReduced | Pool GinOptimizedFilter | приключится вдруг вот верный друг выручить |  0.2239 ms | 0.0022 ms | 0.0015 ms |  0.2214 ms |    5.03 KB |
| QueryReduced | Pool GinOptimizedFilter | чорт з ным зо сталом                       |  0.2364 ms | 0.0036 ms | 0.0024 ms |  0.2339 ms |   47.48 KB |
| QueryReduced | Pool GinFastFilter      | приключится вдруг вот верный друг выручить |  0.2388 ms | 0.0027 ms | 0.0016 ms |  0.2349 ms |    5.03 KB |
| QueryReduced | Pool GinFast            | приключится вдруг вот верный друг выручить |  0.2452 ms | 0.0090 ms | 0.0059 ms |  0.2348 ms |    5.03 KB |
| QueryReduced | Pool GinMerge           | приключится вдруг вот верный друг выручить |  0.2530 ms | 0.0013 ms | 0.0008 ms |  0.2519 ms |    5.03 KB |
| QueryReduced | Pool GinFilter          | чорт з ным зо сталом                       |  0.2912 ms | 0.0096 ms | 0.0063 ms |  0.2815 ms |   47.48 KB |
| QueryReduced | Pool GinFastFilter      | чорт з ным зо сталом                       |  0.2930 ms | 0.0017 ms | 0.0011 ms |  0.2910 ms |   47.48 KB |
| QueryReduced | Pool GinMerge           | чорт з ным зо сталом                       |  0.3035 ms | 0.0037 ms | 0.0019 ms |  0.3002 ms |   47.48 KB |
| QueryReduced | Pool GinFast            | пляшем на                                  |  0.3066 ms | 0.0062 ms | 0.0041 ms |  0.3028 ms |    4.94 KB |
| QueryReduced | Pool GinFastFilter      | преключиться вдруг верный друг             |  0.3478 ms | 0.0046 ms | 0.0031 ms |  0.3437 ms |   10.45 KB |
| QueryReduced | Pool GinFastFilter      | приключится вдруг верный друг              |  0.3537 ms | 0.0047 ms | 0.0031 ms |  0.3485 ms |   10.45 KB |
| QueryReduced | Pool GinOptimized       | чорт з ным зо сталом                       |  0.3738 ms | 0.0016 ms | 0.0011 ms |  0.3725 ms |   47.48 KB |
| QueryReduced | Pool GinOptimized       | пляшем на                                  |  0.3765 ms | 0.0015 ms | 0.0010 ms |  0.3748 ms |    4.94 KB |
| QueryReduced | Pool GinFast            | чорт з ным зо сталом                       |  0.3839 ms | 0.0037 ms | 0.0024 ms |  0.3794 ms |   47.48 KB |
| QueryReduced | Pool GinFilter          | приключится вдруг верный друг              |  0.4784 ms | 0.0137 ms | 0.0091 ms |  0.4564 ms |   10.45 KB |
| QueryReduced | Pool GinFilter          | преключиться вдруг верный друг             |  0.4813 ms | 0.0163 ms | 0.0108 ms |  0.4580 ms |   10.45 KB |
| QueryReduced | Pool GinFilter          | приключится вдруг вот верный друг выручить |  0.5433 ms | 0.0186 ms | 0.0111 ms |  0.5291 ms |    5.03 KB |
| QueryReduced | Pool GinMergeFilter     | ты шла по палубе в молчаний                |  0.5801 ms | 0.0022 ms | 0.0014 ms |  0.5779 ms |  100.34 KB |
| QueryReduced | Pool GinMergeFilter     | оно шла по палубе в молчаний               |  0.6781 ms | 0.0076 ms | 0.0045 ms |  0.6718 ms |  211.10 KB |
| QueryReduced | Pool GinOptimizedFilter | ты шла по палубе в молчаний                |  0.8228 ms | 0.0060 ms | 0.0036 ms |  0.8177 ms |  100.34 KB |
| QueryReduced | Pool GinOptimized       | ты шла по палубе в молчаний                |  0.8947 ms | 0.0197 ms | 0.0130 ms |  0.8781 ms |  100.34 KB |
| QueryReduced | Pool GinOptimizedFilter | оно шла по палубе в молчаний               |  0.8995 ms | 0.0050 ms | 0.0033 ms |  0.8952 ms |  211.10 KB |
| QueryReduced | Pool GinFastFilter      | ты шла по палубе в молчаний                |  0.9160 ms | 0.0077 ms | 0.0051 ms |  0.9056 ms |  100.34 KB |
| QueryReduced | Pool GinOptimized       | оно шла по палубе в молчаний               |  0.9556 ms | 0.0054 ms | 0.0036 ms |  0.9472 ms |  211.10 KB |
| QueryReduced | Pool GinFast            | ты шла по палубе в молчаний                |  0.9799 ms | 0.0124 ms | 0.0082 ms |  0.9594 ms |  100.34 KB |
| QueryReduced | Pool GinFastFilter      | оно шла по палубе в молчаний               |  0.9901 ms | 0.0167 ms | 0.0110 ms |  0.9785 ms |  211.10 KB |
| QueryReduced | Pool GinFilter          | ты шла по палубе в молчаний                |  0.9951 ms | 0.0223 ms | 0.0148 ms |  0.9703 ms |  100.34 KB |
| QueryReduced | Pool GinFilter          | оно шла по палубе в молчаний               |  1.0165 ms | 0.0284 ms | 0.0169 ms |  0.9740 ms |  211.10 KB |
| QueryReduced | Pool GinMerge           | ты шла по палубе в молчаний                |  1.0898 ms | 0.0142 ms | 0.0094 ms |  1.0754 ms |  100.34 KB |
| QueryReduced | Pool GinFast            | оно шла по палубе в молчаний               |  1.1508 ms | 0.0186 ms | 0.0123 ms |  1.1391 ms |  211.10 KB |
| QueryReduced | Pool GinMerge           | оно шла по палубе в молчаний               |  1.1707 ms | 0.0090 ms | 0.0059 ms |  1.1563 ms |  211.10 KB |
| QueryReduced | Pool GinOptimized       | с ними за столом чёрт                      |  1.3007 ms | 0.0217 ms | 0.0143 ms |  1.2776 ms |  441.36 KB |
| QueryReduced | Pool GinOptimized       | удача с ними за столом                     |  1.3187 ms | 0.0689 ms | 0.0456 ms |  1.2546 ms |  441.36 KB |
| QueryReduced | Pool GinOptimized       | чёрт с ними за столом                      |  1.3249 ms | 0.0236 ms | 0.0156 ms |  1.2962 ms |  441.36 KB |
| QueryReduced | Pool GinMerge           | с ними за столом чёрт                      |  1.4519 ms | 0.0397 ms | 0.0263 ms |  1.4051 ms |  441.36 KB |
| QueryReduced | Pool GinOptimized       | пляшем на столе за детей                   |  1.4845 ms | 0.0502 ms | 0.0332 ms |  1.4377 ms |  441.36 KB |
| QueryReduced | Pool GinMerge           | чёрт с ними за столом                      |  1.4963 ms | 0.0245 ms | 0.0162 ms |  1.4651 ms |  441.38 KB |
| QueryReduced | Pool GinMerge           | удача с ними за столом                     |  1.5109 ms | 0.0449 ms | 0.0297 ms |  1.4563 ms |  441.36 KB |
| QueryReduced | Pool GinFast            | с ними за столом чёрт                      |  1.5177 ms | 0.0437 ms | 0.0289 ms |  1.4829 ms |  441.36 KB |
| QueryReduced | Pool GinFast            | удача с ними за столом                     |  1.5211 ms | 0.0360 ms | 0.0238 ms |  1.4771 ms |  441.36 KB |
| QueryReduced | Pool GinFast            | чёрт с ними за столом                      |  1.5836 ms | 0.0450 ms | 0.0298 ms |  1.5379 ms |  441.36 KB |
| QueryReduced | Pool GinMerge           | пляшем на столе за детей                   |  1.6873 ms | 0.0460 ms | 0.0274 ms |  1.6453 ms |  441.36 KB |
| QueryReduced | Pool GinMergeFilter     | чёрт с ними за столом                      |  1.7079 ms | 0.0410 ms | 0.0271 ms |  1.6781 ms |  441.36 KB |
| QueryReduced | Pool GinMergeFilter     | удача с ними за столом                     |  1.7224 ms | 0.0431 ms | 0.0285 ms |  1.6829 ms |  441.36 KB |
| QueryReduced | Pool GinFast            | пляшем на столе за детей                   |  1.7505 ms | 0.0300 ms | 0.0157 ms |  1.7311 ms |  441.36 KB |
| QueryReduced | Pool GinMergeFilter     | с ними за столом чёрт                      |  1.7520 ms | 0.0272 ms | 0.0180 ms |  1.7192 ms |  441.36 KB |
| QueryReduced | Pool GinMergeFilter     | пляшем на столе за детей                   |  1.8407 ms | 0.0281 ms | 0.0186 ms |  1.8142 ms |  441.36 KB |
| QueryReduced | Pool GinOptimizedFilter | пляшем на столе за детей                   |  2.4530 ms | 0.0308 ms | 0.0204 ms |  2.4207 ms |  441.36 KB |
| QueryReduced | Pool GinOptimizedFilter | чёрт с ними за столом                      |  2.5012 ms | 0.0305 ms | 0.0202 ms |  2.4592 ms |  441.36 KB |
| QueryReduced | Pool GinOptimizedFilter | удача с ними за столом                     |  2.5137 ms | 0.0268 ms | 0.0177 ms |  2.4800 ms |  441.36 KB |
| QueryReduced | Pool GinOptimizedFilter | с ними за столом чёрт                      |  2.5617 ms | 0.0493 ms | 0.0293 ms |  2.5272 ms |  441.36 KB |
| QueryReduced | Pool GinFastFilter      | чёрт с ними за столом                      |  3.1082 ms | 0.0647 ms | 0.0428 ms |  3.0487 ms |  441.36 KB |
| QueryReduced | Pool GinFastFilter      | с ними за столом чёрт                      |  3.1626 ms | 0.0565 ms | 0.0336 ms |  3.0981 ms |  441.36 KB |
| QueryReduced | Pool GinFastFilter      | удача с ними за столом                     |  3.1694 ms | 0.0376 ms | 0.0249 ms |  3.1255 ms |  441.36 KB |
| QueryReduced | Pool GinFastFilter      | пляшем на столе за детей                   |  3.2193 ms | 0.1005 ms | 0.0598 ms |  3.1081 ms |  441.36 KB |
| QueryReduced | Pool GinMergeFilter     | на                                         |  4.3539 ms | 0.1230 ms | 0.0814 ms |  4.2245 ms | 3977.08 KB |
| QueryReduced | Pool GinFilter          | чёрт с ними за столом                      |  4.4711 ms | 0.1483 ms | 0.0981 ms |  4.2703 ms |  441.36 KB |
| QueryReduced | Pool GinOptimized       | я ты он она                                |  4.5523 ms | 0.2142 ms | 0.1417 ms |  4.2448 ms | 3977.07 KB |
| QueryReduced | Pool GinFast            | на                                         |  4.5566 ms | 0.3217 ms | 0.2128 ms |  4.2902 ms | 3977.07 KB |
| QueryReduced | Pool GinFilter          | пляшем на столе за детей                   |  4.6608 ms | 0.2169 ms | 0.1434 ms |  4.4166 ms |  441.36 KB |
| QueryReduced | Pool GinFilter          | с ними за столом чёрт                      |  4.6712 ms | 0.1925 ms | 0.1145 ms |  4.4483 ms |  441.36 KB |
| QueryReduced | Pool GinOptimized       | на                                         |  4.8847 ms | 0.2203 ms | 0.1457 ms |  4.6794 ms | 3977.07 KB |
| QueryReduced | Pool GinFilter          | удача с ними за столом                     |  5.0455 ms | 0.7377 ms | 0.4390 ms |  4.6422 ms |  441.36 KB |
| QueryReduced | Pool GinFast            | я ты он она                                |  5.5011 ms | 0.3839 ms | 0.2539 ms |  5.2050 ms | 3977.09 KB |
| QueryReduced | Pool GinMerge           | на                                         |  5.7364 ms | 1.1485 ms | 0.7597 ms |  4.8991 ms | 3977.11 KB |
| QueryReduced | Pool GinMergeFilter     | я ты он она                                |  6.0072 ms | 0.9302 ms | 0.6153 ms |  5.4638 ms | 3977.12 KB |
| QueryReduced | Pool GinOptimizedFilter | на                                         |  6.2350 ms | 0.9404 ms | 0.4918 ms |  5.6439 ms | 3977.13 KB |
| QueryReduced | Pool GinFastFilter      | на                                         |  6.4078 ms | 1.7883 ms | 1.1828 ms |  4.7631 ms | 3977.06 KB |
| QueryReduced | Pool GinMerge           | я ты он она                                |  6.5253 ms | 0.2466 ms | 0.1467 ms |  6.2484 ms | 3977.07 KB |
| QueryReduced | Pool GinOptimizedFilter | я ты он она                                |  7.5135 ms | 1.3258 ms | 0.8769 ms |  6.6084 ms | 3977.08 KB |
| QueryReduced | Pool GinFastFilter      | я ты он она                                |  9.5136 ms | 0.4296 ms | 0.2556 ms |  9.0092 ms | 3977.08 KB |
| QueryReduced | Pool GinFilter          | я ты он она                                | 13.0029 ms | 0.6569 ms | 0.3909 ms | 12.2837 ms | 3977.08 KB |
| QueryReduced |      Legacy             | b b b b b b                                | 14.6038 ms | 1.0060 ms | 0.6654 ms | 13.6333 ms |   22.46 KB |
| QueryReduced | Pool GinFilter          | на                                         | 14.9551 ms | 0.2768 ms | 0.1647 ms | 14.6777 ms | 3977.07 KB |
| QueryReduced |      Legacy             | на                                         | 15.0628 ms | 0.7513 ms | 0.4471 ms | 14.5105 ms | 3977.14 KB |
| QueryReduced |      Legacy             | b b b b                                    | 15.9900 ms | 0.4836 ms | 0.3198 ms | 15.5161 ms |   22.46 KB |
| QueryReduced |      Legacy             | b                                          | 16.1060 ms | 0.5399 ms | 0.3571 ms | 15.6826 ms |   22.27 KB |
| QueryReduced |      Legacy             | b b b b b                                  | 16.2220 ms | 0.4878 ms | 0.3226 ms | 15.6677 ms |   22.47 KB |
| QueryReduced |      Legacy             | пляшем на                                  | 16.4935 ms | 0.6156 ms | 0.3663 ms | 15.9783 ms |    5.02 KB |
| QueryReduced |      Legacy             | удача с ними за столом                     | 17.9665 ms | 1.2576 ms | 0.7484 ms | 17.0116 ms |  441.43 KB |
| QueryReduced |      Legacy             | я ты он она                                | 18.6504 ms | 0.5626 ms | 0.3722 ms | 18.1702 ms | 3977.40 KB |
| QueryReduced |      Legacy             | a b c d .,/#                               | 19.1216 ms | 1.3682 ms | 0.9050 ms | 17.7027 ms |   47.54 KB |
| QueryReduced |      Legacy             | преключиться вдруг верный друг             | 19.3685 ms | 0.4703 ms | 0.3111 ms | 18.8401 ms |   10.52 KB |
| QueryReduced |      Legacy             | приключится вдруг верный друг              | 19.5656 ms | 1.1063 ms | 0.7318 ms | 18.5844 ms |   10.54 KB |
| QueryReduced |      Legacy             | оно шла по палубе в молчаний               | 20.1478 ms | 1.1015 ms | 0.7285 ms | 19.1828 ms |  211.19 KB |
| QueryReduced |      Legacy             | чорт з ным зо сталом                       | 20.2412 ms | 1.0339 ms | 0.6838 ms | 19.1288 ms |   47.55 KB |
| QueryReduced |      Legacy             | приключится вдруг вот верный друг выручить | 20.7218 ms | 0.6503 ms | 0.4301 ms | 20.0046 ms |    5.12 KB |
| QueryReduced |      Legacy             | с ними за столом чёрт                      | 20.8597 ms | 0.7223 ms | 0.4299 ms | 20.3508 ms |  441.42 KB |
| QueryReduced |      Legacy             | ты шла по палубе в молчаний                | 21.0108 ms | 1.5429 ms | 1.0205 ms | 19.3930 ms |  100.40 KB |
| QueryReduced |      Legacy             | пляшем на столе за детей                   | 21.6979 ms | 0.2441 ms | 0.1615 ms | 21.4409 ms |  441.42 KB |
| QueryReduced |      Legacy             | чёрт с ними за столом                      | 21.9834 ms | 0.4382 ms | 0.2899 ms | 21.4547 ms |  441.43 KB |
```

* коммит: ...
