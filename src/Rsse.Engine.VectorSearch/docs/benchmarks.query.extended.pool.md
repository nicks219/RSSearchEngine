## Разработка и оптимизация алгортимов поиска сервиса RSSE

### Идеи и варианты реализации: **[Viktor Loshmanov](https://github.com/ViktorLoshmanov)**

---
### Результаты бенчмарков различных алгоритмов в однопоточном запуске на наборе запросов

* коммит: доработка поискового движка (добавлен direct index спользующий FrozenDictionary)
```
| Method        | SearchType                  | SearchQuery                                | Mean       | Error     | StdDev    | Min        | Allocated  |
|-------------- |---------------------------- |------------------------------------------- |-----------:|----------:|----------:|-----------:|-----------:|
| QueryExtended | Pool GinOffsetFilter        | пляшем на                                  |  0.0045 ms | 0.0000 ms | 0.0000 ms |  0.0044 ms |    5.29 KB |
| QueryExtended | Pool GinArrayDirectFilterBs | b                                          |  0.0057 ms | 0.0002 ms | 0.0001 ms |  0.0055 ms |   21.98 KB |
| QueryExtended | Pool GinArrayDirectHs       | b                                          |  0.0057 ms | 0.0002 ms | 0.0001 ms |  0.0056 ms |   21.98 KB |
| QueryExtended | Pool GinArrayDirectLs       | b                                          |  0.0058 ms | 0.0001 ms | 0.0001 ms |  0.0056 ms |   21.98 KB |
| QueryExtended | Pool GinArrayDirectFilterHs | b                                          |  0.0059 ms | 0.0002 ms | 0.0001 ms |  0.0057 ms |   21.98 KB |
| QueryExtended | Pool GinArrayDirectFilterLs | b                                          |  0.0059 ms | 0.0002 ms | 0.0001 ms |  0.0057 ms |   21.98 KB |
| QueryExtended | Pool GinArrayDirectBs       | b                                          |  0.0059 ms | 0.0002 ms | 0.0001 ms |  0.0057 ms |   21.98 KB |
| QueryExtended | Pool GinOffsetFilter        | b                                          |  0.0067 ms | 0.0001 ms | 0.0001 ms |  0.0066 ms |   22.03 KB |
| QueryExtended | Pool GinOffset              | b                                          |  0.0068 ms | 0.0002 ms | 0.0001 ms |  0.0065 ms |   21.95 KB |
| QueryExtended | Pool GinArrayDirectFilterLs | приключится вдруг верный друг              |  0.0071 ms | 0.0000 ms | 0.0000 ms |  0.0070 ms |    4.71 KB |
| QueryExtended | Pool GinArrayDirectFilterLs | ты шла по палубе в молчаний                |  0.0071 ms | 0.0000 ms | 0.0000 ms |  0.0071 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectFilterHs | приключится вдруг верный друг              |  0.0074 ms | 0.0000 ms | 0.0000 ms |  0.0073 ms |    4.71 KB |
| QueryExtended | Pool GinArrayDirectFilterHs | ты шла по палубе в молчаний                |  0.0074 ms | 0.0000 ms | 0.0000 ms |  0.0074 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectFilterBs | приключится вдруг верный друг              |  0.0076 ms | 0.0001 ms | 0.0000 ms |  0.0075 ms |    4.71 KB |
| QueryExtended | Pool GinArrayDirectFilterLs | a b c d .,/#                               |  0.0076 ms | 0.0000 ms | 0.0000 ms |  0.0076 ms |    4.76 KB |
| QueryExtended | Pool GinArrayDirectFilterBs | ты шла по палубе в молчаний                |  0.0077 ms | 0.0001 ms | 0.0001 ms |  0.0077 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectFilterHs | a b c d .,/#                               |  0.0079 ms | 0.0000 ms | 0.0000 ms |  0.0078 ms |    4.76 KB |
| QueryExtended | Pool GinArrayDirectFilterBs | a b c d .,/#                               |  0.0080 ms | 0.0001 ms | 0.0000 ms |  0.0079 ms |    4.76 KB |
| QueryExtended | Pool GinArrayDirectFilterLs | приключится вдруг вот верный друг выручить |  0.0088 ms | 0.0001 ms | 0.0001 ms |  0.0087 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectFilterHs | приключится вдруг вот верный друг выручить |  0.0093 ms | 0.0001 ms | 0.0000 ms |  0.0092 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectFilterBs | приключится вдруг вот верный друг выручить |  0.0095 ms | 0.0006 ms | 0.0004 ms |  0.0092 ms |    4.72 KB |
| QueryExtended | Pool GinOffset              | b b b b                                    |  0.0106 ms | 0.0002 ms | 0.0002 ms |  0.0104 ms |   10.12 KB |
| QueryExtended | Pool GinOffsetFilter        | b b b b                                    |  0.0106 ms | 0.0000 ms | 0.0000 ms |  0.0106 ms |   10.73 KB |
| QueryExtended | Pool GinArrayDirectFilterLs | b b b b                                    |  0.0119 ms | 0.0001 ms | 0.0001 ms |  0.0118 ms |   10.15 KB |
| QueryExtended | Pool GinOffsetFilter        | b b b b b                                  |  0.0120 ms | 0.0001 ms | 0.0000 ms |  0.0120 ms |   10.91 KB |
| QueryExtended | Pool GinArrayDirectLs       | b b b b                                    |  0.0121 ms | 0.0000 ms | 0.0000 ms |  0.0120 ms |   10.15 KB |
| QueryExtended | Pool GinArrayDirectHs       | b b b b                                    |  0.0125 ms | 0.0001 ms | 0.0001 ms |  0.0124 ms |   10.15 KB |
| QueryExtended | Pool GinArrayDirectFilterHs | b b b b                                    |  0.0127 ms | 0.0001 ms | 0.0000 ms |  0.0127 ms |   10.15 KB |
| QueryExtended | Pool GinOffset              | b b b b b                                  |  0.0128 ms | 0.0000 ms | 0.0000 ms |  0.0127 ms |   10.13 KB |
| QueryExtended | Pool GinArrayDirectLs       | b b b b b                                  |  0.0133 ms | 0.0001 ms | 0.0001 ms |  0.0132 ms |   10.16 KB |
| QueryExtended | Pool GinArrayDirectFilterLs | b b b b b                                  |  0.0136 ms | 0.0002 ms | 0.0001 ms |  0.0134 ms |   10.16 KB |
| QueryExtended | Pool GinArrayDirectHs       | b b b b b                                  |  0.0137 ms | 0.0000 ms | 0.0000 ms |  0.0137 ms |   10.16 KB |
| QueryExtended | Pool GinOffsetFilter        | b b b b b b                                |  0.0140 ms | 0.0006 ms | 0.0004 ms |  0.0136 ms |   10.91 KB |
| QueryExtended | Pool GinArrayDirectFilterLs | b b b b b b                                |  0.0140 ms | 0.0001 ms | 0.0000 ms |  0.0140 ms |   10.16 KB |
| QueryExtended | Pool GinArrayDirectFilterHs | b b b b b                                  |  0.0142 ms | 0.0001 ms | 0.0000 ms |  0.0141 ms |   10.16 KB |
| QueryExtended | Pool GinArrayDirectFilterBs | b b b b                                    |  0.0145 ms | 0.0001 ms | 0.0001 ms |  0.0144 ms |   10.15 KB |
| QueryExtended | Pool GinArrayDirectLs       | b b b b b b                                |  0.0148 ms | 0.0001 ms | 0.0001 ms |  0.0147 ms |   10.16 KB |
| QueryExtended | Pool GinArrayDirectFilterHs | b b b b b b                                |  0.0151 ms | 0.0001 ms | 0.0001 ms |  0.0150 ms |   10.16 KB |
| QueryExtended | Pool GinArrayDirectHs       | b b b b b b                                |  0.0152 ms | 0.0002 ms | 0.0001 ms |  0.0151 ms |   10.16 KB |
| QueryExtended | Pool GinOffsetFilter        | a b c d .,/#                               |  0.0153 ms | 0.0001 ms | 0.0001 ms |  0.0151 ms |    5.34 KB |
| QueryExtended | Pool GinOffsetFilter        | приключится вдруг верный друг              |  0.0154 ms | 0.0002 ms | 0.0001 ms |  0.0152 ms |    5.70 KB |
| QueryExtended | Pool GinArrayDirectBs       | b b b b                                    |  0.0155 ms | 0.0002 ms | 0.0001 ms |  0.0154 ms |   10.15 KB |
| QueryExtended | Pool GinOffset              | b b b b b b                                |  0.0155 ms | 0.0004 ms | 0.0003 ms |  0.0151 ms |   10.13 KB |
| QueryExtended | Pool GinArrayDirectFilterLs | удача с ними за столом                     |  0.0168 ms | 0.0002 ms | 0.0001 ms |  0.0167 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectFilterBs | удача с ними за столом                     |  0.0168 ms | 0.0002 ms | 0.0001 ms |  0.0167 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectBs       | b b b b b                                  |  0.0172 ms | 0.0004 ms | 0.0003 ms |  0.0169 ms |   10.16 KB |
| QueryExtended | Pool GinArrayDirectFilterBs | b b b b b                                  |  0.0172 ms | 0.0002 ms | 0.0002 ms |  0.0170 ms |   10.16 KB |
| QueryExtended | Pool GinArrayDirectFilterHs | удача с ними за столом                     |  0.0172 ms | 0.0001 ms | 0.0001 ms |  0.0171 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectFilterBs | с ними за столом чёрт                      |  0.0181 ms | 0.0001 ms | 0.0001 ms |  0.0180 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectFilterBs | чёрт с ними за столом                      |  0.0181 ms | 0.0002 ms | 0.0001 ms |  0.0180 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectFilterLs | чёрт с ними за столом                      |  0.0181 ms | 0.0001 ms | 0.0000 ms |  0.0181 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectFilterHs | чёрт с ними за столом                      |  0.0183 ms | 0.0001 ms | 0.0001 ms |  0.0182 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectFilterHs | с ними за столом чёрт                      |  0.0186 ms | 0.0001 ms | 0.0001 ms |  0.0184 ms |    4.72 KB |
| QueryExtended | Pool GinOffsetFilter        | приключится вдруг вот верный друг выручить |  0.0186 ms | 0.0002 ms | 0.0002 ms |  0.0183 ms |    5.88 KB |
| QueryExtended | Pool GinArrayDirectFilterLs | с ними за столом чёрт                      |  0.0189 ms | 0.0001 ms | 0.0001 ms |  0.0188 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectFilterBs | b b b b b b                                |  0.0191 ms | 0.0001 ms | 0.0001 ms |  0.0190 ms |   10.16 KB |
| QueryExtended | Pool GinOffsetFilter        | ты шла по палубе в молчаний                |  0.0194 ms | 0.0002 ms | 0.0001 ms |  0.0192 ms |    5.88 KB |
| QueryExtended | Pool GinArrayDirectBs       | b b b b b b                                |  0.0194 ms | 0.0001 ms | 0.0001 ms |  0.0192 ms |   10.16 KB |
| QueryExtended | Pool GinOffset              | a b c d .,/#                               |  0.0208 ms | 0.0002 ms | 0.0001 ms |  0.0207 ms |    4.73 KB |
| QueryExtended | Pool GinArrayDirectLs       | a b c d .,/#                               |  0.0378 ms | 0.0002 ms | 0.0001 ms |  0.0376 ms |    4.76 KB |
| QueryExtended | Pool GinArrayDirectBs       | a b c d .,/#                               |  0.0387 ms | 0.0003 ms | 0.0002 ms |  0.0384 ms |    4.76 KB |
| QueryExtended | Pool GinArrayDirectHs       | a b c d .,/#                               |  0.0456 ms | 0.0004 ms | 0.0003 ms |  0.0452 ms |    4.76 KB |
| QueryExtended | Pool GinOffsetFilter        | удача с ними за столом                     |  0.0664 ms | 0.0006 ms | 0.0003 ms |  0.0660 ms |    5.88 KB |
| QueryExtended | Pool GinArrayDirectFilterHs | пляшем на                                  |  0.0931 ms | 0.0003 ms | 0.0002 ms |  0.0928 ms |    4.70 KB |
| QueryExtended | Pool GinOffsetFilter        | чёрт с ними за столом                      |  0.0955 ms | 0.0005 ms | 0.0003 ms |  0.0951 ms |    5.88 KB |
| QueryExtended | Pool GinArrayDirectFilterLs | пляшем на                                  |  0.0963 ms | 0.0009 ms | 0.0006 ms |  0.0949 ms |    4.70 KB |
| QueryExtended | Pool GinArrayDirectFilterBs | пляшем на                                  |  0.1014 ms | 0.0036 ms | 0.0024 ms |  0.0992 ms |    4.70 KB |
| QueryExtended | Pool GinOffsetFilter        | с ними за столом чёрт                      |  0.1080 ms | 0.0003 ms | 0.0002 ms |  0.1077 ms |    5.88 KB |
| QueryExtended | Pool GinOffset              | приключится вдруг верный друг              |  0.1138 ms | 0.0010 ms | 0.0007 ms |  0.1128 ms |    4.68 KB |
| QueryExtended | Pool GinArrayDirectBs       | приключится вдруг верный друг              |  0.1153 ms | 0.0003 ms | 0.0002 ms |  0.1151 ms |    4.71 KB |
| QueryExtended | Pool GinArrayDirectLs       | приключится вдруг верный друг              |  0.1193 ms | 0.0007 ms | 0.0004 ms |  0.1185 ms |    4.71 KB |
| QueryExtended | Pool GinArrayDirectHs       | приключится вдруг верный друг              |  0.1210 ms | 0.0005 ms | 0.0003 ms |  0.1205 ms |    4.71 KB |
| QueryExtended | Pool GinArrayDirectHs       | пляшем на                                  |  0.2103 ms | 0.0032 ms | 0.0021 ms |  0.2049 ms |    4.70 KB |
| QueryExtended | Pool GinArrayDirectBs       | пляшем на                                  |  0.2108 ms | 0.0030 ms | 0.0016 ms |  0.2079 ms |    4.70 KB |
| QueryExtended | Pool GinArrayDirectLs       | пляшем на                                  |  0.2153 ms | 0.0006 ms | 0.0004 ms |  0.2146 ms |    4.70 KB |
| QueryExtended | Pool GinArrayDirectFilterBs | я ты он она                                |  0.2266 ms | 0.0027 ms | 0.0016 ms |  0.2244 ms |   21.98 KB |
| QueryExtended | Pool GinOffset              | приключится вдруг вот верный друг выручить |  0.2344 ms | 0.0033 ms | 0.0017 ms |  0.2316 ms |    4.69 KB |
| QueryExtended | Pool GinArrayDirectFilterLs | я ты он она                                |  0.2587 ms | 0.0062 ms | 0.0041 ms |  0.2487 ms |   21.98 KB |
| QueryExtended | Pool GinOffset              | пляшем на                                  |  0.2595 ms | 0.0016 ms | 0.0011 ms |  0.2578 ms |    4.67 KB |
| QueryExtended | Pool GinArrayDirectFilterHs | я ты он она                                |  0.2860 ms | 0.0100 ms | 0.0066 ms |  0.2737 ms |   21.98 KB |
| QueryExtended | Pool GinArrayDirectBs       | приключится вдруг вот верный друг выручить |  0.4757 ms | 0.0055 ms | 0.0036 ms |  0.4655 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectLs       | приключится вдруг вот верный друг выручить |  0.4979 ms | 0.0158 ms | 0.0105 ms |  0.4774 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectHs       | приключится вдруг вот верный друг выручить |  0.5377 ms | 0.0192 ms | 0.0114 ms |  0.5157 ms |    4.72 KB |
| QueryExtended | Pool GinOffset              | с ними за столом чёрт                      |  0.5528 ms | 0.0031 ms | 0.0020 ms |  0.5486 ms |    4.69 KB |
| QueryExtended | Pool GinOffset              | чёрт с ними за столом                      |  0.5588 ms | 0.0052 ms | 0.0035 ms |  0.5547 ms |    4.69 KB |
| QueryExtended | Pool GinOffset              | удача с ними за столом                     |  0.5588 ms | 0.0054 ms | 0.0035 ms |  0.5531 ms |    4.69 KB |
| QueryExtended | Pool GinOffsetFilter        | я ты он она                                |  0.7514 ms | 0.0065 ms | 0.0038 ms |  0.7446 ms |   22.98 KB |
| QueryExtended | Pool GinOffset              | я ты он она                                |  0.9369 ms | 0.0154 ms | 0.0080 ms |  0.9260 ms |   21.95 KB |
| QueryExtended | Pool GinOffset              | ты шла по палубе в молчаний                |  1.1155 ms | 0.0064 ms | 0.0042 ms |  1.1094 ms |    4.69 KB |
| QueryExtended | Pool GinArrayDirectHs       | на                                         |  1.1667 ms | 0.0716 ms | 0.0474 ms |  1.0949 ms | 1914.19 KB |
| QueryExtended | Pool GinArrayDirectFilterBs | на                                         |  1.1756 ms | 0.0651 ms | 0.0430 ms |  1.0963 ms | 1914.19 KB |
| QueryExtended | Pool GinArrayDirectLs       | на                                         |  1.1840 ms | 0.0489 ms | 0.0324 ms |  1.1345 ms | 1914.20 KB |
| QueryExtended | Pool GinArrayDirectFilterLs | на                                         |  1.1876 ms | 0.0574 ms | 0.0379 ms |  1.1426 ms | 1914.19 KB |
| QueryExtended | Pool GinArrayDirectFilterHs | на                                         |  1.2023 ms | 0.0403 ms | 0.0267 ms |  1.1694 ms | 1914.20 KB |
| QueryExtended | Pool GinArrayDirectBs       | на                                         |  1.2110 ms | 0.0540 ms | 0.0357 ms |  1.1548 ms | 1914.19 KB |
| QueryExtended | Pool GinOffset              | на                                         |  1.2759 ms | 0.0395 ms | 0.0261 ms |  1.2315 ms | 1914.16 KB |
| QueryExtended | Pool GinOffsetFilter        | на                                         |  1.2799 ms | 0.0667 ms | 0.0441 ms |  1.2023 ms | 1914.25 KB |
| QueryExtended | Pool GinArrayDirectHs       | удача с ними за столом                     |  2.2103 ms | 0.1345 ms | 0.0800 ms |  2.1170 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectLs       | удача с ними за столом                     |  2.2130 ms | 0.0837 ms | 0.0554 ms |  2.0938 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectLs       | чёрт с ними за столом                      |  2.2147 ms | 0.0952 ms | 0.0630 ms |  2.1188 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectHs       | чёрт с ними за столом                      |  2.2290 ms | 0.0738 ms | 0.0488 ms |  2.1387 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectBs       | удача с ними за столом                     |  2.5656 ms | 0.0525 ms | 0.0347 ms |  2.5111 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectHs       | с ними за столом чёрт                      |  2.6660 ms | 0.2032 ms | 0.1344 ms |  2.5273 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectLs       | с ними за столом чёрт                      |  2.6712 ms | 0.4512 ms | 0.2984 ms |  2.3676 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectBs       | чёрт с ними за столом                      |  2.6763 ms | 0.0409 ms | 0.0214 ms |  2.6359 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectBs       | с ними за столом чёрт                      |  2.9677 ms | 0.0933 ms | 0.0617 ms |  2.8474 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectBs       | я ты он она                                |  6.5279 ms | 0.2906 ms | 0.1520 ms |  6.2449 ms |   21.98 KB |
| QueryExtended | Pool GinArrayDirectHs       | я ты он она                                |  7.0135 ms | 0.6251 ms | 0.4135 ms |  6.5570 ms |   21.99 KB |
| QueryExtended | Pool GinArrayDirectLs       | я ты он она                                |  8.0365 ms | 0.3180 ms | 0.1892 ms |  7.8040 ms |   21.99 KB |
| QueryExtended | Pool GinArrayDirectBs       | ты шла по палубе в молчаний                | 11.3593 ms | 0.5844 ms | 0.3477 ms | 10.7933 ms |    4.72 KB |
| QueryExtended | Pool GinArrayDirectLs       | ты шла по палубе в молчаний                | 13.6486 ms | 0.2528 ms | 0.1672 ms | 13.3699 ms |    4.73 KB |
| QueryExtended | Pool GinArrayDirectHs       | ты шла по палубе в молчаний                | 15.4659 ms | 0.8041 ms | 0.5319 ms | 14.7014 ms |    4.72 KB |
| QueryExtended |      Legacy                 | b                                          | 15.8596 ms | 0.5334 ms | 0.3528 ms | 15.2040 ms |   22.01 KB |
| QueryExtended |      Legacy                 | на                                         | 16.1606 ms | 0.8138 ms | 0.5383 ms | 15.3963 ms | 1914.24 KB |
| QueryExtended |      Legacy                 | пляшем на                                  | 17.0826 ms | 0.5997 ms | 0.3966 ms | 16.6374 ms |    4.74 KB |
| QueryExtended |      Legacy                 | a b c d .,/#                               | 17.3053 ms | 0.5183 ms | 0.3428 ms | 16.8274 ms |    4.82 KB |
| QueryExtended |      Legacy                 | я ты он она                                | 18.0778 ms | 0.4599 ms | 0.2737 ms | 17.6923 ms |   22.03 KB |
| QueryExtended |      Legacy                 | b b b b                                    | 18.4552 ms | 0.9509 ms | 0.6290 ms | 17.2330 ms |   10.19 KB |
| QueryExtended |      Legacy                 | удача с ними за столом                     | 18.8245 ms | 1.1005 ms | 0.7279 ms | 17.7992 ms |    4.77 KB |
| QueryExtended |      Legacy                 | чёрт с ними за столом                      | 18.8562 ms | 0.8136 ms | 0.5381 ms | 18.2618 ms |    4.75 KB |
| QueryExtended |      Legacy                 | с ними за столом чёрт                      | 18.9877 ms | 0.9787 ms | 0.6473 ms | 17.7927 ms |    4.75 KB |
| QueryExtended |      Legacy                 | приключится вдруг верный друг              | 19.0332 ms | 1.0523 ms | 0.6960 ms | 17.9527 ms |    4.75 KB |
| QueryExtended |      Legacy                 | b b b b b                                  | 19.3731 ms | 0.4365 ms | 0.2887 ms | 18.9213 ms |   10.19 KB |
| QueryExtended |      Legacy                 | приключится вдруг вот верный друг выручить | 19.8971 ms | 0.3508 ms | 0.2087 ms | 19.5762 ms |    4.76 KB |
| QueryExtended |      Legacy                 | b b b b b b                                | 20.4012 ms | 1.6109 ms | 1.0655 ms | 19.4863 ms |   10.21 KB |
| QueryExtended |      Legacy                 | ты шла по палубе в молчаний                | 20.5738 ms | 0.5526 ms | 0.3655 ms | 19.8016 ms |    4.75 KB |
```

* коммит: ...
