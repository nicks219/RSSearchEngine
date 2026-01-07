## Разработка и оптимизация алгортимов поиска сервиса RSSE

### Идеи и варианты реализации: **[Viktor Loshmanov](https://github.com/ViktorLoshmanov)**

---
### Результаты бенчмарков различных алгоритмов в однопоточном запуске на наборе запросов

* коммит: доработка поискового движка (добавлен direct index спользующий FrozenDictionary)
```
| Method        | SearchType                  | SearchQuery                                | Mean       | Error     | StdDev    | Min        | Allocated  |
|-------------- |---------------------------- |------------------------------------------- |-----------:|----------:|----------:|-----------:|-----------:|
| QueryExtended |      GinOffsetFilter        | пляшем на                                  |  0.0045 ms | 0.0000 ms | 0.0000 ms |  0.0045 ms |    5.50 KB |
| QueryExtended |      GinArrayDirectLs       | b                                          |  0.0057 ms | 0.0001 ms | 0.0001 ms |  0.0055 ms |   22.06 KB |
| QueryExtended |      GinArrayDirectHs       | b                                          |  0.0057 ms | 0.0001 ms | 0.0001 ms |  0.0055 ms |   22.06 KB |
| QueryExtended |      GinArrayDirectBs       | b                                          |  0.0057 ms | 0.0002 ms | 0.0001 ms |  0.0055 ms |   22.06 KB |
| QueryExtended |      GinArrayDirectFilterBs | b                                          |  0.0057 ms | 0.0002 ms | 0.0001 ms |  0.0056 ms |   22.15 KB |
| QueryExtended |      GinArrayDirectFilterLs | b                                          |  0.0058 ms | 0.0002 ms | 0.0001 ms |  0.0056 ms |   22.15 KB |
| QueryExtended |      GinArrayDirectFilterHs | b                                          |  0.0059 ms | 0.0002 ms | 0.0001 ms |  0.0057 ms |   22.15 KB |
| QueryExtended |      GinOffset              | b                                          |  0.0068 ms | 0.0001 ms | 0.0001 ms |  0.0066 ms |   22.16 KB |
| QueryExtended |      GinArrayDirectFilterLs | приключится вдруг верный друг              |  0.0068 ms | 0.0001 ms | 0.0000 ms |  0.0068 ms |    5.28 KB |
| QueryExtended |      GinOffsetFilter        | b                                          |  0.0069 ms | 0.0002 ms | 0.0001 ms |  0.0067 ms |   22.24 KB |
| QueryExtended |      GinArrayDirectFilterLs | ты шла по палубе в молчаний                |  0.0070 ms | 0.0002 ms | 0.0001 ms |  0.0069 ms |    5.46 KB |
| QueryExtended |      GinArrayDirectFilterHs | приключится вдруг верный друг              |  0.0072 ms | 0.0001 ms | 0.0001 ms |  0.0071 ms |    5.28 KB |
| QueryExtended |      GinArrayDirectFilterHs | ты шла по палубе в молчаний                |  0.0074 ms | 0.0001 ms | 0.0001 ms |  0.0072 ms |    5.46 KB |
| QueryExtended |      GinArrayDirectFilterLs | a b c d .,/#                               |  0.0074 ms | 0.0001 ms | 0.0001 ms |  0.0073 ms |    5.33 KB |
| QueryExtended |      GinArrayDirectFilterBs | приключится вдруг верный друг              |  0.0075 ms | 0.0000 ms | 0.0000 ms |  0.0074 ms |    5.28 KB |
| QueryExtended |      GinArrayDirectFilterHs | a b c d .,/#                               |  0.0076 ms | 0.0001 ms | 0.0000 ms |  0.0076 ms |    5.33 KB |
| QueryExtended |      GinArrayDirectFilterBs | ты шла по палубе в молчаний                |  0.0078 ms | 0.0001 ms | 0.0001 ms |  0.0077 ms |    5.46 KB |
| QueryExtended |      GinArrayDirectFilterBs | a b c d .,/#                               |  0.0083 ms | 0.0005 ms | 0.0004 ms |  0.0079 ms |    5.33 KB |
| QueryExtended |      GinArrayDirectFilterLs | приключится вдруг вот верный друг выручить |  0.0088 ms | 0.0001 ms | 0.0001 ms |  0.0086 ms |    5.46 KB |
| QueryExtended |      GinArrayDirectFilterHs | приключится вдруг вот верный друг выручить |  0.0090 ms | 0.0002 ms | 0.0001 ms |  0.0088 ms |    5.46 KB |
| QueryExtended |      GinArrayDirectFilterBs | приключится вдруг вот верный друг выручить |  0.0093 ms | 0.0000 ms | 0.0000 ms |  0.0092 ms |    5.46 KB |
| QueryExtended |      GinOffset              | b b b b                                    |  0.0106 ms | 0.0001 ms | 0.0000 ms |  0.0105 ms |   10.33 KB |
| QueryExtended |      GinOffsetFilter        | b b b b                                    |  0.0107 ms | 0.0001 ms | 0.0001 ms |  0.0106 ms |   10.95 KB |
| QueryExtended |      GinArrayDirectFilterLs | b b b b                                    |  0.0117 ms | 0.0002 ms | 0.0001 ms |  0.0115 ms |   10.72 KB |
| QueryExtended |      GinArrayDirectLs       | b b b b                                    |  0.0117 ms | 0.0001 ms | 0.0001 ms |  0.0116 ms |   10.70 KB |
| QueryExtended |      GinArrayDirectHs       | b b b b                                    |  0.0125 ms | 0.0002 ms | 0.0001 ms |  0.0123 ms |   10.70 KB |
| QueryExtended |      GinOffsetFilter        | b b b b b                                  |  0.0125 ms | 0.0002 ms | 0.0001 ms |  0.0124 ms |   11.46 KB |
| QueryExtended |      GinArrayDirectFilterHs | b b b b                                    |  0.0126 ms | 0.0004 ms | 0.0002 ms |  0.0123 ms |   10.72 KB |
| QueryExtended |      GinOffset              | b b b b b                                  |  0.0131 ms | 0.0002 ms | 0.0001 ms |  0.0130 ms |   10.67 KB |
| QueryExtended |      GinArrayDirectLs       | b b b b b                                  |  0.0133 ms | 0.0001 ms | 0.0000 ms |  0.0133 ms |   11.00 KB |
| QueryExtended |      GinArrayDirectFilterLs | b b b b b                                  |  0.0137 ms | 0.0002 ms | 0.0001 ms |  0.0134 ms |   10.90 KB |
| QueryExtended |      GinArrayDirectHs       | b b b b b                                  |  0.0139 ms | 0.0002 ms | 0.0001 ms |  0.0138 ms |   11.00 KB |
| QueryExtended |      GinArrayDirectFilterLs | b b b b b b                                |  0.0143 ms | 0.0002 ms | 0.0001 ms |  0.0141 ms |   10.90 KB |
| QueryExtended |      GinOffsetFilter        | b b b b b b                                |  0.0143 ms | 0.0003 ms | 0.0002 ms |  0.0141 ms |   11.46 KB |
| QueryExtended |      GinArrayDirectFilterHs | b b b b b                                  |  0.0145 ms | 0.0003 ms | 0.0002 ms |  0.0143 ms |   10.90 KB |
| QueryExtended |      GinArrayDirectFilterBs | b b b b                                    |  0.0146 ms | 0.0002 ms | 0.0002 ms |  0.0144 ms |   10.72 KB |
| QueryExtended |      GinArrayDirectBs       | b b b b                                    |  0.0148 ms | 0.0001 ms | 0.0000 ms |  0.0147 ms |   10.70 KB |
| QueryExtended |      GinArrayDirectFilterHs | b b b b b b                                |  0.0148 ms | 0.0002 ms | 0.0001 ms |  0.0147 ms |   10.90 KB |
| QueryExtended |      GinArrayDirectLs       | b b b b b b                                |  0.0149 ms | 0.0001 ms | 0.0000 ms |  0.0148 ms |   11.00 KB |
| QueryExtended |      GinOffsetFilter        | a b c d .,/#                               |  0.0151 ms | 0.0003 ms | 0.0002 ms |  0.0149 ms |    5.55 KB |
| QueryExtended |      GinArrayDirectHs       | b b b b b b                                |  0.0153 ms | 0.0002 ms | 0.0001 ms |  0.0151 ms |   11.00 KB |
| QueryExtended |      GinOffset              | b b b b b b                                |  0.0154 ms | 0.0002 ms | 0.0001 ms |  0.0152 ms |   10.67 KB |
| QueryExtended |      GinOffsetFilter        | приключится вдруг верный друг              |  0.0161 ms | 0.0002 ms | 0.0001 ms |  0.0160 ms |    5.91 KB |
| QueryExtended |      GinArrayDirectFilterHs | удача с ними за столом                     |  0.0164 ms | 0.0003 ms | 0.0002 ms |  0.0162 ms |    5.46 KB |
| QueryExtended |      GinArrayDirectFilterBs | удача с ними за столом                     |  0.0169 ms | 0.0001 ms | 0.0001 ms |  0.0168 ms |    5.46 KB |
| QueryExtended |      GinArrayDirectFilterLs | удача с ними за столом                     |  0.0170 ms | 0.0003 ms | 0.0002 ms |  0.0168 ms |    5.46 KB |
| QueryExtended |      GinArrayDirectBs       | b b b b b                                  |  0.0171 ms | 0.0004 ms | 0.0002 ms |  0.0169 ms |   11.00 KB |
| QueryExtended |      GinArrayDirectFilterBs | b b b b b                                  |  0.0175 ms | 0.0004 ms | 0.0002 ms |  0.0173 ms |   10.90 KB |
| QueryExtended |      GinArrayDirectFilterLs | чёрт с ними за столом                      |  0.0181 ms | 0.0003 ms | 0.0002 ms |  0.0178 ms |    5.46 KB |
| QueryExtended |      GinArrayDirectFilterBs | с ними за столом чёрт                      |  0.0182 ms | 0.0001 ms | 0.0001 ms |  0.0180 ms |    5.46 KB |
| QueryExtended |      GinArrayDirectFilterBs | чёрт с ними за столом                      |  0.0183 ms | 0.0004 ms | 0.0003 ms |  0.0180 ms |    5.46 KB |
| QueryExtended |      GinArrayDirectFilterHs | чёрт с ними за столом                      |  0.0184 ms | 0.0005 ms | 0.0003 ms |  0.0180 ms |    5.46 KB |
| QueryExtended |      GinArrayDirectFilterLs | с ними за столом чёрт                      |  0.0186 ms | 0.0006 ms | 0.0004 ms |  0.0181 ms |    5.46 KB |
| QueryExtended |      GinArrayDirectFilterHs | с ними за столом чёрт                      |  0.0187 ms | 0.0002 ms | 0.0002 ms |  0.0185 ms |    5.46 KB |
| QueryExtended |      GinArrayDirectFilterBs | b b b b b b                                |  0.0190 ms | 0.0003 ms | 0.0002 ms |  0.0188 ms |   10.90 KB |
| QueryExtended |      GinOffsetFilter        | приключится вдруг вот верный друг выручить |  0.0192 ms | 0.0004 ms | 0.0003 ms |  0.0188 ms |    6.43 KB |
| QueryExtended |      GinArrayDirectBs       | b b b b b b                                |  0.0193 ms | 0.0001 ms | 0.0001 ms |  0.0192 ms |   11.00 KB |
| QueryExtended |      GinOffsetFilter        | ты шла по палубе в молчаний                |  0.0200 ms | 0.0002 ms | 0.0001 ms |  0.0197 ms |    6.43 KB |
| QueryExtended |      GinOffset              | a b c d .,/#                               |  0.0212 ms | 0.0002 ms | 0.0001 ms |  0.0209 ms |    4.94 KB |
| QueryExtended |      GinArrayDirectLs       | a b c d .,/#                               |  0.0364 ms | 0.0001 ms | 0.0001 ms |  0.0363 ms |    5.31 KB |
| QueryExtended |      GinArrayDirectBs       | a b c d .,/#                               |  0.0386 ms | 0.0004 ms | 0.0002 ms |  0.0382 ms |    5.31 KB |
| QueryExtended |      GinArrayDirectHs       | a b c d .,/#                               |  0.0470 ms | 0.0002 ms | 0.0001 ms |  0.0468 ms |    5.31 KB |
| QueryExtended |      GinOffsetFilter        | удача с ними за столом                     |  0.0676 ms | 0.0006 ms | 0.0004 ms |  0.0670 ms |    6.43 KB |
| QueryExtended |      GinArrayDirectFilterHs | пляшем на                                  |  0.0921 ms | 0.0026 ms | 0.0017 ms |  0.0900 ms |    5.27 KB |
| QueryExtended |      GinArrayDirectFilterLs | пляшем на                                  |  0.0960 ms | 0.0007 ms | 0.0004 ms |  0.0953 ms |    5.27 KB |
| QueryExtended |      GinOffsetFilter        | чёрт с ними за столом                      |  0.0984 ms | 0.0005 ms | 0.0003 ms |  0.0981 ms |    6.43 KB |
| QueryExtended |      GinArrayDirectFilterBs | пляшем на                                  |  0.1002 ms | 0.0005 ms | 0.0003 ms |  0.0998 ms |    5.27 KB |
| QueryExtended |      GinOffsetFilter        | с ними за столом чёрт                      |  0.1133 ms | 0.0004 ms | 0.0003 ms |  0.1128 ms |    6.43 KB |
| QueryExtended |      GinOffset              | приключится вдруг верный друг              |  0.1163 ms | 0.0015 ms | 0.0010 ms |  0.1150 ms |    4.89 KB |
| QueryExtended |      GinArrayDirectBs       | приключится вдруг верный друг              |  0.1176 ms | 0.0003 ms | 0.0002 ms |  0.1172 ms |    5.51 KB |
| QueryExtended |      GinArrayDirectLs       | приключится вдруг верный друг              |  0.1194 ms | 0.0005 ms | 0.0004 ms |  0.1188 ms |    5.51 KB |
| QueryExtended |      GinArrayDirectHs       | приключится вдруг верный друг              |  0.1267 ms | 0.0029 ms | 0.0019 ms |  0.1241 ms |    5.51 KB |
| QueryExtended |      GinArrayDirectLs       | пляшем на                                  |  0.2112 ms | 0.0010 ms | 0.0006 ms |  0.2103 ms |    5.26 KB |
| QueryExtended |      GinArrayDirectBs       | пляшем на                                  |  0.2126 ms | 0.0006 ms | 0.0004 ms |  0.2123 ms |    5.26 KB |
| QueryExtended |      GinArrayDirectHs       | пляшем на                                  |  0.2144 ms | 0.0018 ms | 0.0009 ms |  0.2127 ms |    5.26 KB |
| QueryExtended |      GinArrayDirectFilterBs | я ты он она                                |  0.2304 ms | 0.0020 ms | 0.0012 ms |  0.2280 ms |   22.56 KB |
| QueryExtended |      GinOffset              | приключится вдруг вот верный друг выручить |  0.2397 ms | 0.0024 ms | 0.0016 ms |  0.2378 ms |    5.23 KB |
| QueryExtended |      GinArrayDirectFilterLs | я ты он она                                |  0.2534 ms | 0.0079 ms | 0.0052 ms |  0.2457 ms |   22.55 KB |
| QueryExtended |      GinOffset              | пляшем на                                  |  0.2727 ms | 0.0023 ms | 0.0015 ms |  0.2708 ms |    4.88 KB |
| QueryExtended |      GinArrayDirectFilterHs | я ты он она                                |  0.2787 ms | 0.0090 ms | 0.0059 ms |  0.2694 ms |   22.55 KB |
| QueryExtended |      GinArrayDirectBs       | приключится вдруг вот верный друг выручить |  0.4736 ms | 0.0086 ms | 0.0057 ms |  0.4597 ms |    5.86 KB |
| QueryExtended |      GinArrayDirectLs       | приключится вдруг вот верный друг выручить |  0.4800 ms | 0.0121 ms | 0.0080 ms |  0.4636 ms |    5.86 KB |
| QueryExtended |      GinArrayDirectHs       | приключится вдруг вот верный друг выручить |  0.5344 ms | 0.0130 ms | 0.0086 ms |  0.5151 ms |    5.86 KB |
| QueryExtended |      GinOffset              | с ними за столом чёрт                      |  0.5593 ms | 0.0088 ms | 0.0058 ms |  0.5472 ms |    5.24 KB |
| QueryExtended |      GinOffset              | удача с ними за столом                     |  0.5618 ms | 0.0058 ms | 0.0038 ms |  0.5522 ms |    5.24 KB |
| QueryExtended |      GinOffset              | чёрт с ними за столом                      |  0.5817 ms | 0.0130 ms | 0.0086 ms |  0.5702 ms |    5.24 KB |
| QueryExtended |      GinOffsetFilter        | я ты он она                                |  0.7646 ms | 0.0050 ms | 0.0033 ms |  0.7565 ms |   23.19 KB |
| QueryExtended |      GinOffset              | я ты он она                                |  0.7840 ms | 0.0054 ms | 0.0036 ms |  0.7759 ms |   22.17 KB |
| QueryExtended |      GinOffset              | ты шла по палубе в молчаний                |  1.1049 ms | 0.0047 ms | 0.0031 ms |  1.1012 ms |    5.24 KB |
| QueryExtended |      GinArrayDirectLs       | на                                         |  1.2037 ms | 0.0339 ms | 0.0224 ms |  1.1711 ms | 1914.29 KB |
| QueryExtended |      GinArrayDirectBs       | на                                         |  1.2150 ms | 0.0530 ms | 0.0351 ms |  1.1618 ms | 1914.27 KB |
| QueryExtended |      GinArrayDirectFilterBs | на                                         |  1.2199 ms | 0.0413 ms | 0.0273 ms |  1.1781 ms | 1914.37 KB |
| QueryExtended |      GinArrayDirectFilterHs | на                                         |  1.2291 ms | 0.0494 ms | 0.0327 ms |  1.1832 ms | 1914.36 KB |
| QueryExtended |      GinArrayDirectHs       | на                                         |  1.2339 ms | 0.0322 ms | 0.0168 ms |  1.2092 ms | 1914.28 KB |
| QueryExtended |      GinArrayDirectFilterLs | на                                         |  1.2481 ms | 0.0433 ms | 0.0286 ms |  1.2032 ms | 1914.36 KB |
| QueryExtended |      GinOffsetFilter        | на                                         |  1.3028 ms | 0.0305 ms | 0.0182 ms |  1.2782 ms | 1914.48 KB |
| QueryExtended |      GinOffset              | на                                         |  1.3271 ms | 0.0387 ms | 0.0256 ms |  1.2868 ms | 1914.38 KB |
| QueryExtended |      GinArrayDirectLs       | удача с ними за столом                     |  2.1210 ms | 0.0726 ms | 0.0480 ms |  2.0357 ms |    5.87 KB |
| QueryExtended |      GinArrayDirectLs       | чёрт с ними за столом                      |  2.1452 ms | 0.1060 ms | 0.0701 ms |  2.0178 ms |    5.86 KB |
| QueryExtended |      GinArrayDirectHs       | чёрт с ними за столом                      |  2.2560 ms | 0.1152 ms | 0.0762 ms |  2.1611 ms |    5.86 KB |
| QueryExtended |      GinArrayDirectHs       | удача с ними за столом                     |  2.2983 ms | 0.0911 ms | 0.0542 ms |  2.2201 ms |    5.86 KB |
| QueryExtended |      GinArrayDirectLs       | с ними за столом чёрт                      |  2.3340 ms | 0.1142 ms | 0.0756 ms |  2.2008 ms |    5.87 KB |
| QueryExtended |      GinArrayDirectBs       | удача с ними за столом                     |  2.5299 ms | 0.0632 ms | 0.0376 ms |  2.4480 ms |    5.86 KB |
| QueryExtended |      GinArrayDirectHs       | с ними за столом чёрт                      |  2.6650 ms | 0.3205 ms | 0.2120 ms |  2.4576 ms |    5.86 KB |
| QueryExtended |      GinArrayDirectBs       | чёрт с ними за столом                      |  2.7173 ms | 0.0486 ms | 0.0321 ms |  2.6635 ms |    5.87 KB |
| QueryExtended |      GinArrayDirectBs       | с ними за столом чёрт                      |  2.8490 ms | 0.0527 ms | 0.0348 ms |  2.7628 ms |    5.86 KB |
| QueryExtended |      GinArrayDirectBs       | я ты он она                                |  6.6583 ms | 0.1413 ms | 0.0841 ms |  6.5361 ms |   22.79 KB |
| QueryExtended |      GinArrayDirectLs       | я ты он она                                |  6.8524 ms | 0.1183 ms | 0.0619 ms |  6.7590 ms |   22.79 KB |
| QueryExtended |      GinArrayDirectHs       | я ты он она                                |  7.1183 ms | 0.1968 ms | 0.1171 ms |  6.9327 ms |   22.79 KB |
| QueryExtended |      GinArrayDirectLs       | ты шла по палубе в молчаний                | 11.3758 ms | 0.2865 ms | 0.1705 ms | 11.1803 ms |    5.88 KB |
| QueryExtended |      GinArrayDirectBs       | ты шла по палубе в молчаний                | 11.7222 ms | 0.3728 ms | 0.2218 ms | 11.3507 ms |    5.88 KB |
| QueryExtended |      GinArrayDirectHs       | ты шла по палубе в молчаний                | 15.5435 ms | 0.9366 ms | 0.6195 ms | 14.7284 ms |    5.90 KB |
| QueryExtended |      Legacy                 | b                                          | 15.8908 ms | 1.0524 ms | 0.6961 ms | 15.0639 ms |   22.01 KB |
| QueryExtended |      Legacy                 | на                                         | 16.3756 ms | 0.5630 ms | 0.3724 ms | 15.7800 ms | 1914.23 KB |
| QueryExtended |      Legacy                 | a b c d .,/#                               | 17.4293 ms | 1.7758 ms | 0.9288 ms | 15.9301 ms |    4.79 KB |
| QueryExtended |      Legacy                 | пляшем на                                  | 17.5495 ms | 0.6650 ms | 0.4399 ms | 16.8391 ms |    4.76 KB |
| QueryExtended |      Legacy                 | я ты он она                                | 17.7053 ms | 0.5958 ms | 0.3941 ms | 17.1594 ms |   22.03 KB |
| QueryExtended |      Legacy                 | b b b b                                    | 17.9066 ms | 1.3303 ms | 0.8799 ms | 16.7262 ms |   10.20 KB |
| QueryExtended |      Legacy                 | b b b b b                                  | 18.5156 ms | 1.0523 ms | 0.6262 ms | 17.3050 ms |   10.19 KB |
| QueryExtended |      Legacy                 | приключится вдруг верный друг              | 19.0173 ms | 0.8596 ms | 0.5686 ms | 17.9859 ms |    4.75 KB |
| QueryExtended |      Legacy                 | b b b b b b                                | 19.4935 ms | 1.3718 ms | 0.9074 ms | 18.2798 ms |   10.21 KB |
| QueryExtended |      Legacy                 | чёрт с ними за столом                      | 19.6643 ms | 1.0389 ms | 0.6871 ms | 18.7491 ms |    4.76 KB |
| QueryExtended |      Legacy                 | удача с ними за столом                     | 20.1792 ms | 0.7082 ms | 0.4684 ms | 19.3545 ms |    4.75 KB |
| QueryExtended |      Legacy                 | с ними за столом чёрт                      | 20.5276 ms | 0.9179 ms | 0.6071 ms | 19.6567 ms |    4.77 KB |
| QueryExtended |      Legacy                 | приключится вдруг вот верный друг выручить | 20.8807 ms | 0.3618 ms | 0.2153 ms | 20.6117 ms |    4.77 KB |
| QueryExtended |      Legacy                 | ты шла по палубе в молчаний                | 21.0562 ms | 0.8927 ms | 0.5905 ms | 19.9311 ms |    4.76 KB |
```

* коммит: ...
