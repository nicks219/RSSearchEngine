## Разработка и оптимизация алгортимов поиска сервиса RSSE

### Идеи и варианты реализации: **[Viktor Loshmanov](https://github.com/ViktorLoshmanov)**

---
### Результаты бенчмарков различных алгоритмов в однопоточном запуске на наборе запросов
### Выполняется 1000 запросов в одном бенчмарке, результаты делить на 1000

* коммит: доработка поискового движка (добавлен direct index спользующий FrozenDictionary)
```
| Type                     | Method        | SearchType                 | Mean        | Error     | StdDev    | Min         | Allocated     |
|------------------------- |-------------- |--------------------------- |------------:|----------:|----------:|------------:|--------------:|
| StQueryBenchmarkExtended | QueryExtended | Pool GinOffsetFilter       |    354.7 ms |  29.26 ms |  19.36 ms |    329.7 ms |  144702.08 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinDirectOffsetFilter |    356.5 ms |  37.86 ms |  25.04 ms |    314.4 ms |  144506.30 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinMergeFilter        |    361.1 ms |  28.12 ms |  18.60 ms |    333.0 ms |  144507.70 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinFrozenDirectFilter |    387.2 ms | 106.63 ms |  70.53 ms |    318.2 ms |  144506.54 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinFrozenDirectFilter |    431.4 ms |  31.09 ms |  18.50 ms |    402.4 ms |  143903.04 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinDirectOffsetFilter |    436.5 ms |  38.80 ms |  25.66 ms |    404.6 ms |  143902.71 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinOffsetFilter       |    443.9 ms |  40.69 ms |  26.91 ms |    412.5 ms |  145087.40 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinMergeFilter        |    449.1 ms |  42.89 ms |  28.37 ms |    397.2 ms |  143901.53 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinOffset             |    712.9 ms |  66.75 ms |  44.15 ms |    670.8 ms |  143870.94 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinOffset             |    843.8 ms |  32.36 ms |  19.26 ms |    816.4 ms |  144250.71 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinFilter             |    871.5 ms |  14.67 ms |   9.70 ms |    856.1 ms |  251542.43 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinFilter             |    969.1 ms |  32.61 ms |  19.41 ms |    940.9 ms |  143902.19 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinFastFilter         |  1,056.8 ms |  36.08 ms |  23.86 ms |  1,018.9 ms |  264540.15 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinFastFilter         |  1,120.5 ms |  42.90 ms |  28.37 ms |  1,084.5 ms |  143902.43 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinMerge              |  3,471.5 ms | 478.60 ms | 316.56 ms |  3,091.2 ms |  144679.30 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinMerge              |  3,510.6 ms | 188.04 ms | 124.37 ms |  3,320.1 ms |  143905.02 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinDirectOffset       |  3,667.0 ms |  45.66 ms |  30.20 ms |  3,631.6 ms |  144678.69 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinDirectOffset       |  3,793.8 ms |  60.69 ms |  36.12 ms |  3,749.0 ms |  143901.53 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinFrozenDirect       |  3,812.4 ms |  61.97 ms |  36.88 ms |  3,753.7 ms |  144681.91 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinFrozenDirect       |  4,371.0 ms |  80.41 ms |  53.19 ms |  4,292.5 ms |  143907.36 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinFast               |  8,059.4 ms |  62.54 ms |  41.37 ms |  8,009.6 ms |  923326.17 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinOptimized          |  8,480.6 ms | 170.84 ms |  89.35 ms |  8,374.1 ms |  143870.61 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinFast               |  8,686.3 ms | 147.63 ms |  97.65 ms |  8,532.0 ms |  143870.28 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinOptimized          |  9,474.7 ms | 321.73 ms | 212.80 ms |  9,215.1 ms |  923317.75 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinSimple             | 14,225.9 ms | 199.78 ms | 132.14 ms | 14,074.5 ms |  143932.83 KB |
| StQueryBenchmarkExtended | QueryExtended |      Legacy                | 20,150.0 ms | 250.41 ms | 165.63 ms | 19,937.2 ms |  143935.05 KB |
|                          |               |                            |             |           |           |             |               |
| StQueryBenchmarkReduced  | QueryReduced  |      GinMergeFilter        |  1,120.9 ms |  36.89 ms |  21.95 ms |  1,089.4 ms |  565785.23 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinFast               |  1,154.1 ms |  28.86 ms |  19.09 ms |  1,119.8 ms |  565486.82 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinMerge              |  1,200.3 ms |  39.19 ms |  25.92 ms |  1,169.7 ms |  565755.08 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinOptimized          |  1,247.7 ms |  58.58 ms |  38.75 ms |  1,193.0 ms |  565488.52 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinMergeFilter        |  1,249.1 ms |  50.98 ms |  30.34 ms |  1,181.8 ms |  565486.79 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinMerge              |  1,362.3 ms |  91.56 ms |  60.56 ms |  1,287.6 ms |  565487.33 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinOptimizedFilter    |  1,516.2 ms |  75.24 ms |  49.77 ms |  1,452.1 ms |  565492.86 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinOptimizedFilter    |  1,844.7 ms | 117.20 ms |  77.52 ms |  1,727.8 ms | 1118958.53 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinFastFilter         |  1,880.3 ms | 162.55 ms |  96.73 ms |  1,694.0 ms |  565489.97 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinFast               |  1,943.4 ms |  26.00 ms |  17.20 ms |  1,922.5 ms | 1454398.00 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinOptimized          |  2,037.9 ms |  24.99 ms |  14.87 ms |  2,020.6 ms | 2222812.60 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinFastFilter         |  2,370.5 ms | 312.04 ms | 206.39 ms |  2,088.5 ms | 1160171.95 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinFilter             |  3,511.4 ms | 229.37 ms | 151.71 ms |  3,262.0 ms |  565491.80 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinFilter             |  3,525.6 ms |  19.97 ms |  11.89 ms |  3,510.3 ms | 1095082.74 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinSimple             |  9,268.3 ms |  68.14 ms |  35.64 ms |  9,205.8 ms |  565559.39 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      Legacy                | 17,649.9 ms | 930.34 ms | 615.37 ms | 16,934.6 ms |  565548.46 KB |
```

* коммит: доработка поискового движка (оптимизация extended алгоритмов - добавлена проверка по дополнительному индексу, удалены simple лгоритмы)
```
| Type                     | Method        | SearchType                 | Mean         | Error      | StdDev     | Min          | Allocated     |
|--------------------------|---------------|--------------------------- |-------------:|-----------:|-----------:|-------------:|--------------:|
| StQueryBenchmarkExtended | QueryExtended | Pool GinDirectOffsetFilter |    123.95 ms |   2.073 ms |   1.371 ms |    120.81 ms |  144240.53 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinFrozenDirectFilter |    125.30 ms |   4.304 ms |   2.847 ms |    121.38 ms |  144841.55 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinDirectOffsetFilter |    127.02 ms |   4.076 ms |   2.696 ms |    122.76 ms |  144842.94 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinFrozenDirectFilter |    127.20 ms |   3.626 ms |   2.398 ms |    123.14 ms |  144241.03 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinOffsetFilter       |    176.40 ms |   4.987 ms |   3.299 ms |    170.61 ms |  145042.10 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinOffsetFilter       |    177.53 ms |   8.216 ms |   5.434 ms |    169.86 ms |  145421.85 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinMergeFilter        |    224.27 ms |   5.121 ms |   3.047 ms |    217.27 ms |  144244.10 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinMergeFilter        |    225.28 ms |   5.243 ms |   3.468 ms |    220.43 ms |  144844.15 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinOffset             |    423.01 ms |  16.229 ms |  10.734 ms |    409.77 ms |  144592.34 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinOffset             |    424.70 ms |  14.985 ms |   9.912 ms |    411.92 ms |  144215.79 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinFilter             |    794.21 ms |  19.925 ms |  13.179 ms |    776.37 ms |  144248.26 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinFilter             |    864.79 ms |  16.011 ms |  10.590 ms |    846.39 ms |  251884.41 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinFastFilter         |    887.32 ms |  17.415 ms |  10.363 ms |    869.74 ms |  144244.95 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinFastFilter         |    959.67 ms |  21.261 ms |  14.063 ms |    939.37 ms |  264882.77 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinMerge              |  2,611.35 ms |  48.002 ms |  31.750 ms |  2,554.61 ms |  145016.80 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinMerge              |  2,635.74 ms |  64.595 ms |  42.726 ms |  2,592.16 ms |  144241.52 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinDirectOffset       |  2,639.90 ms |  48.365 ms |  31.990 ms |  2,593.41 ms |  144249.60 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinDirectOffset       |  2,673.60 ms |  41.977 ms |  24.980 ms |  2,621.12 ms |  145016.52 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinFrozenDirect       |  3,034.20 ms |  39.302 ms |  25.995 ms |  2,985.54 ms |  145016.52 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinFrozenDirect       |  3,048.48 ms |  65.987 ms |  39.267 ms |  2,961.00 ms |  144241.80 KB |
| StQueryBenchmarkExtended | QueryExtended | Pool GinFast               |  6,923.96 ms | 117.295 ms |  69.800 ms |  6,832.42 ms |  144210.88 KB |
| StQueryBenchmarkExtended | QueryExtended |      GinFast               |  7,624.47 ms |  53.064 ms |  31.578 ms |  7,574.68 ms |  923660.23 KB |
| StQueryBenchmarkExtended | QueryExtended |      Legacy                | 17,154.53 ms | 307.159 ms | 203.166 ms | 16,881.79 ms |  144273.42 KB |
|                          |               |                            |              |            |            |              |               |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinOptimized          |  1,000.88 ms |  12.694 ms |   8.396 ms |    989.68 ms |  565889.14 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinMergeFilter        |  1,067.58 ms |  15.824 ms |  10.466 ms |  1,049.39 ms |  565894.38 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinMergeFilter        |  1,072.33 ms |  16.257 ms |  10.753 ms |  1,062.00 ms |  566185.85 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinMerge              |  1,124.60 ms |  18.140 ms |  11.998 ms |  1,107.63 ms |  566152.20 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinMerge              |  1,126.65 ms |  16.627 ms |   9.894 ms |  1,114.54 ms |  565894.26 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinFast               |  1,126.86 ms |  13.293 ms |   8.792 ms |  1,113.83 ms |  565894.20 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinOptimizedFilter    |  1,334.30 ms |  23.420 ms |  15.490 ms |  1,311.46 ms |  565884.95 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinFastFilter         |  1,603.09 ms |  15.791 ms |  10.445 ms |  1,593.28 ms |  565889.07 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinOptimizedFilter    |  1,650.20 ms |  32.876 ms |  21.745 ms |  1,623.68 ms | 1119366.27 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinFast               |  1,813.18 ms |  44.382 ms |  29.356 ms |  1,765.98 ms | 1454796.66 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinFastFilter         |  1,972.73 ms |  42.717 ms |  25.420 ms |  1,949.52 ms | 1160564.82 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinOptimized          |  1,977.91 ms |  18.275 ms |  10.875 ms |  1,962.24 ms | 2223214.92 KB |
| StQueryBenchmarkReduced  | QueryReduced  | Pool GinFilter             |  3,002.58 ms |  21.171 ms |  14.003 ms |  2,973.37 ms |  565897.52 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      GinFilter             |  3,340.39 ms |  27.167 ms |  16.167 ms |  3,321.66 ms | 1095483.23 KB |
| StQueryBenchmarkReduced  | QueryReduced  |      Legacy                | 15,721.16 ms | 248.325 ms | 164.252 ms | 15,488.19 ms |  565950.83 KB |
```

* коммит: доработка поискового движка (добавлено хранение данных документа в одном массиве GinArrayDirect, удалены extended fast алгоритмы)
```
| Type                        | Method             | SearchType                   | Mean           | Error       | StdDev      | Min            | Allocated     |
|---------------------------- |------------------- |----------------------------- |---------------:|------------:|------------:|---------------:|--------------:|
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinDirectOffsetFilterLs |    121.3694 ms |   3.1992 ms |   2.1161 ms |    117.4590 ms |  144241.15 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinFrozenDirectFilter   |    123.2403 ms |   3.2067 ms |   2.1211 ms |    120.7666 ms |  144841.12 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinDirectOffsetFilterLs |    125.0225 ms |   4.5255 ms |   2.9933 ms |    118.9673 ms |  144842.23 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinFrozenDirectFilter   |    128.3337 ms |   4.6956 ms |   3.1058 ms |    123.4107 ms |  144242.69 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinDirectOffsetFilterBs |    129.9412 ms |   3.9188 ms |   2.5920 ms |    125.8431 ms |  144242.39 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinDirectOffsetFilterBs |    131.7192 ms |   2.5807 ms |   1.7070 ms |    129.7346 ms |  144842.39 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinOffsetFilter         |    170.7456 ms |   4.1087 ms |   2.7177 ms |    166.1949 ms |  145421.05 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinOffsetFilter         |    172.9589 ms |   1.5659 ms |   0.9319 ms |    170.9938 ms |  145041.62 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinMergeFilter          |    222.6505 ms |   4.3186 ms |   2.8565 ms |    217.1644 ms |  144843.20 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinMergeFilter          |    224.0194 ms |   3.0514 ms |   1.5960 ms |    220.8196 ms |  144243.05 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterLs  |    229.8614 ms |   4.1869 ms |   2.4916 ms |    227.4490 ms |  144241.29 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterBs  |    230.9331 ms |   3.7795 ms |   2.4999 ms |    227.5048 ms |  144843.66 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterBs  |    233.4713 ms |   5.5218 ms |   3.6523 ms |    227.1036 ms |  144241.20 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterLs  |    235.9166 ms |   4.1786 ms |   2.7639 ms |    231.9263 ms |  144842.64 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinOffset               |    407.4826 ms |   2.4520 ms |   1.6218 ms |    404.0698 ms |  144590.00 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinOffset               |    434.6524 ms |   3.5465 ms |   2.3458 ms |    430.6072 ms |  144210.22 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinFilter               |    790.8775 ms |  27.7554 ms |  18.3585 ms |    763.3268 ms |  144242.36 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinFilter               |    882.9788 ms |  15.3933 ms |  10.1817 ms |    863.3239 ms |  251878.84 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinMerge                |  2,630.6819 ms |  26.8937 ms |  14.0659 ms |  2,608.7873 ms |  144241.80 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinMerge                |  2,672.0121 ms |  47.2171 ms |  31.2312 ms |  2,624.5500 ms |  145016.80 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinDirectOffsetLs       |  2,725.6366 ms |  38.5168 ms |  25.4765 ms |  2,663.3953 ms |  144241.47 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinDirectOffsetLs       |  2,751.6710 ms |  36.3034 ms |  24.0124 ms |  2,719.3612 ms |  145016.52 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectLs        |  2,920.3582 ms |  48.8034 ms |  32.2804 ms |  2,884.4650 ms |  145017.08 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectLs        |  2,946.8379 ms |  54.1972 ms |  35.8481 ms |  2,867.7516 ms |  144241.47 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinFrozenDirect         |  2,985.9849 ms |  54.9533 ms |  36.3482 ms |  2,942.5613 ms |  145016.47 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinFrozenDirect         |  3,004.8625 ms |  43.7098 ms |  22.8611 ms |  2,967.4650 ms |  144242.08 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinDirectOffsetBs       |  3,171.9043 ms |  17.8263 ms |   9.3235 ms |  3,156.2192 ms |  144241.75 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinDirectOffsetBs       |  3,217.7566 ms |  36.8648 ms |  21.9376 ms |  3,186.6825 ms |  145016.80 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectBs        |  3,221.9086 ms |  91.9828 ms |  54.7375 ms |  3,162.9826 ms |  145016.84 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectBs        |  3,246.2850 ms |  76.4273 ms |  50.5519 ms |  3,178.7413 ms |  144242.08 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      Legacy                  | 18,098.5667 ms | 307.2139 ms | 203.2029 ms | 17,755.4124 ms |  144273.09 KB |
|                             |                    |                              |                |             |             |                |               |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimized            |    999.6078 ms |  11.6574 ms |   6.9371 ms |    987.1925 ms |  565891.43 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinMergeFilter          |  1,077.9936 ms |  36.0987 ms |  23.8771 ms |  1,042.2550 ms |  565884.83 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinMergeFilter          |  1,097.8791 ms |  19.0242 ms |   9.9500 ms |  1,086.5856 ms |  566183.82 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinMerge                |  1,115.7942 ms |  24.0695 ms |  15.9205 ms |  1,099.7108 ms |  566157.05 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinMerge                |  1,123.8209 ms |  14.5597 ms |   9.6304 ms |  1,106.9998 ms |  565884.66 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinFast                 |  1,184.5789 ms |  20.3487 ms |  13.4594 ms |  1,159.5382 ms |  565890.72 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimizedFilter      |  1,347.8022 ms |  13.2608 ms |   7.8913 ms |  1,330.3363 ms |  565892.34 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinFastFilter           |  1,556.9433 ms |  34.7460 ms |  22.9823 ms |  1,517.0265 ms |  565897.60 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinOptimizedFilter      |  1,645.6037 ms |  14.0535 ms |   9.2955 ms |  1,630.4418 ms | 1119359.13 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinFast                 |  1,818.0602 ms |  22.5270 ms |  14.9002 ms |  1,799.3743 ms | 1454803.59 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinFastFilter           |  1,873.5094 ms |  19.7283 ms |  13.0490 ms |  1,857.3733 ms | 1000874.51 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinOptimized            |  1,961.0973 ms |  30.0113 ms |  19.8506 ms |  1,931.8745 ms | 2223218.50 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinFilter               |  3,074.1684 ms |  41.7115 ms |  27.5896 ms |  3,039.7896 ms |  565897.52 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinFilter               |  3,330.1918 ms |  30.8656 ms |  20.4157 ms |  3,293.9583 ms | 1095489.13 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      Legacy                  | 16,002.4425 ms | 167.8237 ms | 111.0050 ms | 15,870.2716 ms |  565955.72 KB |
```

* коммит: доработка поискового движка (оптимизация GinArrayDirect)
```
| Type                        | Method             | SearchType                  | Mean           | Error       | StdDev      | Min            | Allocated     |
|---------------------------- |------------------- |---------------------------- |---------------:|------------:|------------:|---------------:|--------------:|
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterLs |    121.1187 ms |   4.6239 ms |   3.0584 ms |    115.6396 ms |  144842.53 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterLs |    121.7447 ms |   3.4945 ms |   2.3114 ms |    118.2535 ms |  144241.44 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterBs |    126.8080 ms |   5.2383 ms |   3.4648 ms |    120.7802 ms |  144843.93 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterHs |    127.9769 ms |   4.3271 ms |   2.8621 ms |    123.7157 ms |  144843.76 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterBs |    128.1217 ms |   4.5479 ms |   3.0082 ms |    122.2570 ms |  144243.55 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterHs |    130.6821 ms |   3.8674 ms |   2.5580 ms |    127.2620 ms |  144242.41 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinOffsetFilter        |    181.7120 ms |   6.5580 ms |   4.3377 ms |    173.8805 ms |  145041.98 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinOffsetFilter        |    184.5947 ms |   6.2866 ms |   4.1582 ms |    177.8126 ms |  145422.47 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinOffset              |    417.0183 ms |   9.2763 ms |   5.5202 ms |    410.6166 ms |  144589.72 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinOffset              |    418.4994 ms |  15.5469 ms |  10.2833 ms |    402.4812 ms |  144216.84 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectLs       |  2,200.7577 ms |  57.5730 ms |  34.2608 ms |  2,142.3996 ms |  145016.47 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectLs       |  2,252.2124 ms |  78.5509 ms |  51.9566 ms |  2,185.5413 ms |  144241.52 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectBs       |  2,553.8222 ms |  34.1005 ms |  22.5554 ms |  2,527.0193 ms |  144241.80 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectBs       |  2,613.3482 ms |  41.0383 ms |  27.1443 ms |  2,563.8484 ms |  145016.52 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectHs       |  2,944.7004 ms |  41.2099 ms |  27.2578 ms |  2,893.0633 ms |  144242.13 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectHs       |  2,961.3500 ms |  91.9474 ms |  60.8175 ms |  2,884.0817 ms |  145016.80 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      Legacy                 | 17,607.3112 ms | 211.3234 ms | 139.7774 ms | 17,341.9480 ms |  144273.09 KB |
|                             |                    |                             |                |             |             |                |               |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimized           |  1,024.5202 ms |  20.0791 ms |  13.2811 ms |  1,004.9709 ms |  565895.79 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinMergeFilter         |  1,078.3478 ms |  17.9308 ms |  11.8601 ms |  1,062.5315 ms |  566182.78 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinMergeFilter         |  1,082.1056 ms |  30.4522 ms |  15.9271 ms |  1,058.5128 ms |  565888.49 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinMerge               |  1,115.7584 ms |  21.6829 ms |  14.3419 ms |  1,097.9293 ms |  565888.93 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinMerge               |  1,140.8270 ms |  12.5375 ms |   8.2928 ms |  1,131.8474 ms |  566151.42 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinFast                |  1,165.2859 ms |  17.6153 ms |  11.6514 ms |  1,147.9095 ms |  565894.15 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimizedFilter     |  1,335.0239 ms |  18.9445 ms |  12.5306 ms |  1,312.7198 ms |  565891.51 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinFastFilter          |  1,574.5885 ms |  30.6178 ms |  20.2518 ms |  1,535.7181 ms |  565889.20 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinOptimizedFilter     |  1,682.2323 ms |  14.9355 ms |   9.8789 ms |  1,664.7075 ms | 1119373.62 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinFast                |  1,804.3620 ms |  25.5400 ms |  16.8932 ms |  1,775.8738 ms | 1454801.07 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinFastFilter          |  1,894.1884 ms |  17.1503 ms |  11.3439 ms |  1,872.1697 ms | 1000873.41 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinOptimized           |  2,012.6525 ms |  36.6572 ms |  21.8141 ms |  1,992.6706 ms | 2223216.02 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinFilter              |  3,088.7533 ms |  27.9131 ms |  18.4628 ms |  3,059.6496 ms | 2824543.96 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinFilter              |  3,374.6788 ms |  38.6839 ms |  25.5870 ms |  3,337.0041 ms | 1095489.33 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      Legacy                 | 16,132.6477 ms | 133.6885 ms |  79.5559 ms | 16,013.1033 ms |  565949.26 KB |
```

* коммит: доработка поискового движка (оптимизация выделения памяти для метрик)
```
| Type                        | Method             | SearchType                  | Mean           | Error       | StdDev      | Min            | Allocated     |
|---------------------------- |------------------- |---------------------------- |---------------:|------------:|------------:|---------------:|--------------:|
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterLs |     45.7906 ms |   1.0866 ms |   0.7187 ms |     44.4095 ms |     509.66 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterLs |     46.3460 ms |   1.1497 ms |   0.7605 ms |     44.9444 ms |    1109.54 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterBs |     48.2040 ms |   1.0115 ms |   0.6691 ms |     47.3062 ms |    1109.55 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterBs |     48.9619 ms |   0.9141 ms |   0.6046 ms |     47.9127 ms |     509.63 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterHs |     49.5507 ms |   1.1800 ms |   0.7805 ms |     48.0350 ms |    1109.50 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterHs |     49.6934 ms |   1.1428 ms |   0.7559 ms |     48.4197 ms |     509.61 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinOffsetFilter        |    103.6954 ms |   1.1225 ms |   0.7425 ms |    102.0196 ms |    1310.14 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinOffsetFilter        |    105.8261 ms |   0.8833 ms |   0.5257 ms |    105.0541 ms |    1689.41 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinOffset              |    342.3612 ms |   0.9364 ms |   0.6194 ms |    341.1846 ms |     478.71 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinOffset              |    366.6089 ms |   1.6087 ms |   1.0641 ms |    364.1219 ms |     858.21 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectLs       |  2,097.6720 ms |  48.4314 ms |  32.0344 ms |  2,066.7756 ms |    1284.63 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectLs       |  2,129.8796 ms |  39.4112 ms |  26.0681 ms |  2,096.0409 ms |     509.63 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectBs       |  2,453.9964 ms |  73.7492 ms |  48.7805 ms |  2,382.3900 ms |     509.68 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectBs       |  2,458.7109 ms |  29.8554 ms |  17.7664 ms |  2,428.0106 ms |    1284.96 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectHs       |  2,821.6148 ms |  45.8535 ms |  30.3292 ms |  2,788.0147 ms |     509.68 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectHs       |  2,857.9006 ms |  29.2127 ms |  19.3224 ms |  2,824.9773 ms |    1284.96 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      Legacy                 | 17,825.6888 ms | 283.3692 ms | 187.4312 ms | 17,501.6540 ms |     541.26 KB |
|                             |                    |                             |                |             |             |                |               |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimized           |    575.8160 ms |  16.7305 ms |   9.9561 ms |    557.4171 ms |     876.16 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinMergeFilter         |    612.6859 ms |  13.2516 ms |   8.7651 ms |    591.5271 ms |     875.84 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinMergeFilter         |    623.1215 ms |   8.1098 ms |   4.8260 ms |    613.7455 ms |    1166.33 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinMerge               |    679.9324 ms |  15.4966 ms |  10.2500 ms |    668.3381 ms |     876.21 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinFast                |    685.5837 ms |  13.9578 ms |   9.2322 ms |    671.5851 ms |     875.88 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinMerge               |    694.0782 ms |  19.3823 ms |  12.8202 ms |    672.6344 ms |    1139.13 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimizedFilter     |    887.9186 ms |  10.4095 ms |   6.8852 ms |    876.1541 ms |     875.55 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinFastFilter          |  1,070.5075 ms |  13.0752 ms |   8.6484 ms |  1,056.2615 ms |     875.84 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinOptimizedFilter     |  1,191.8186 ms |  32.1987 ms |  21.2974 ms |  1,167.1959 ms |  554356.24 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinFast                |  1,262.4006 ms |  25.1878 ms |  14.9889 ms |  1,243.3337 ms |  889790.55 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinFastFilter          |  1,336.1423 ms |  12.8693 ms |   8.5122 ms |  1,322.9258 ms |  435865.02 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinOptimized           |  1,532.6972 ms |  13.5796 ms |   8.0810 ms |  1,517.1545 ms | 1658212.27 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinFilter              |  2,443.6288 ms |  50.1374 ms |  29.8360 ms |  2,364.7664 ms |     876.16 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinFilter              |  2,723.1915 ms |  19.5854 ms |  10.2435 ms |  2,709.6987 ms |  530468.18 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      Legacy                 | 15,977.9031 ms | 329.4480 ms | 217.9094 ms | 15,647.0856 ms |     938.71 KB |
```

* коммит: доработка поискового движка (оптимизация памяти выделяемой токенайзером)
```
| Type                        | Method             | SearchType                  | Mean           | Error       | StdDev      | Min            | Allocated     |
|---------------------------- |------------------- |---------------------------- |---------------:|------------:|------------:|---------------:|--------------:|
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterLs |     45.5868 ms |   0.6422 ms |   0.4248 ms |     44.8607 ms |     402.47 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterLs |     46.0043 ms |   0.8736 ms |   0.5778 ms |     45.3730 ms |    1002.39 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterBs |     48.7302 ms |   0.7792 ms |   0.5154 ms |     47.8595 ms |    1002.37 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterBs |     49.0825 ms |   0.9623 ms |   0.6365 ms |     47.9817 ms |     402.45 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterHs |     49.5218 ms |   1.2745 ms |   0.8430 ms |     48.0266 ms |     402.47 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterHs |     50.2555 ms |   0.6019 ms |   0.3981 ms |     49.6599 ms |    1002.40 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinOffsetFilter        |    100.5852 ms |   1.1172 ms |   0.7389 ms |     98.6193 ms |    1203.06 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinOffsetFilter        |    103.7744 ms |   1.2504 ms |   0.8271 ms |    102.0402 ms |    1582.40 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinOffset              |    344.6816 ms |   2.7914 ms |   1.6611 ms |    342.3503 ms |     751.08 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinOffset              |    350.5582 ms |   2.4531 ms |   1.6226 ms |    348.5001 ms |     371.86 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectLs       |  2,087.8992 ms |  34.7380 ms |  22.9770 ms |  2,058.9982 ms |    1177.78 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectLs       |  2,129.5787 ms |  70.0003 ms |  41.6560 ms |  2,068.7974 ms |     402.50 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectBs       |  2,422.6741 ms |  53.9101 ms |  35.6582 ms |  2,376.8696 ms |     402.50 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectBs       |  2,423.2217 ms |  61.4521 ms |  40.6468 ms |  2,367.2454 ms |    1177.83 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectHs       |  2,796.6713 ms |  58.9795 ms |  39.0113 ms |  2,733.7901 ms |     403.16 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectHs       |  2,816.9891 ms |  67.6979 ms |  44.7780 ms |  2,750.1244 ms |    1177.83 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      Legacy                 | 17,656.9097 ms | 331.1444 ms | 219.0315 ms | 17,284.6759 ms |     434.17 KB |
|                             |                    |                             |                |             |             |                |               |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimized           |    572.1296 ms |   9.9149 ms |   6.5581 ms |    562.4874 ms |     433.48 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinMergeFilter         |    613.7233 ms |  24.4087 ms |  16.1448 ms |    583.5439 ms |     723.37 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinMergeFilter         |    621.2907 ms |  14.4598 ms |   9.5643 ms |    603.3195 ms |     432.88 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinFast                |    663.4688 ms |  20.6296 ms |  13.6452 ms |    639.2318 ms |     433.81 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinMerge               |    683.2157 ms |  20.3972 ms |  13.4915 ms |    666.2511 ms |     432.88 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinMerge               |    700.7601 ms |  12.2833 ms |   8.1246 ms |    684.8466 ms |     695.84 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimizedFilter     |    895.8280 ms |   5.8994 ms |   3.5107 ms |    888.7437 ms |     433.48 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinFastFilter          |  1,065.2154 ms |  20.9014 ms |  10.9318 ms |  1,044.2423 ms |     433.20 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinOptimizedFilter     |  1,195.0755 ms |  10.3099 ms |   6.1353 ms |  1,188.8849 ms |  553907.48 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinFast                |  1,271.1651 ms |  21.9547 ms |  14.5217 ms |  1,238.7912 ms |  889349.52 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinFastFilter          |  1,306.6736 ms |  18.7289 ms |  12.3880 ms |  1,286.3920 ms |     435421 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinOptimized           |  1,560.4799 ms |  17.8685 ms |  11.8189 ms |  1,540.2339 ms | 1657766.43 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinFilter              |  2,472.9217 ms |  43.0202 ms |  28.4552 ms |  2,429.8102 ms |     433.25 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinFilter              |  2,735.7182 ms |  34.0542 ms |  22.5248 ms |  2,708.0777 ms |  530034.44 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      Legacy                 | 15,512.6171 ms | 290.2059 ms | 191.9532 ms | 15,196.8418 ms |     496.31 KB |
```

* коммит: доработка поискового движка (добавлено хранение данных документа в одном массиве GinArrayDirect ддя reduced)
```
| Type                        | Method             | SearchType                  | Mean           | Error       | StdDev      | Median         | Min            | Allocated     |
|---------------------------- |------------------- |---------------------------- |---------------:|------------:|------------:|---------------:|---------------:|--------------:|
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterLs |     46.5984 ms |   1.0432 ms |   0.6900 ms |     46.3174 ms |     45.5906 ms |     371.29 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterLs |     48.5642 ms |   1.1499 ms |   0.7606 ms |     48.8540 ms |     47.2869 ms |     971.14 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterBs |     49.6261 ms |   0.8613 ms |   0.5697 ms |     49.6456 ms |     48.8572 ms |     971.14 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterBs |     49.7607 ms |   0.9435 ms |   0.6240 ms |     50.0601 ms |     48.7255 ms |     371.19 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectFilterHs |     50.1350 ms |   1.0554 ms |   0.6981 ms |     50.5760 ms |     48.7612 ms |     971.12 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectFilterHs |     50.6845 ms |   0.3722 ms |   0.2462 ms |     50.7195 ms |     50.0450 ms |     371.25 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinOffsetFilter        |    105.1378 ms |   1.6837 ms |   1.1137 ms |    105.5593 ms |    103.2864 ms |    1551.27 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinOffsetFilter        |    105.8225 ms |   0.8735 ms |   0.5778 ms |    105.8556 ms |    104.5900 ms |    1171.88 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinOffset              |    347.8696 ms |   3.3335 ms |   1.9837 ms |    347.4328 ms |    344.9579 ms |     719.45 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinOffset              |    358.1566 ms |   4.6498 ms |   3.0755 ms |    357.2450 ms |    354.7743 ms |     340.33 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectLs       |  2,147.5911 ms |  18.1321 ms |   9.4835 ms |  2,149.5577 ms |  2,128.3735 ms |     371.30 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectLs       |  2,184.6083 ms |  33.0572 ms |  21.8653 ms |  2,187.6274 ms |  2,156.2710 ms |    1146.30 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectBs       |  2,494.8562 ms |  31.8172 ms |  21.0451 ms |  2,501.7566 ms |  2,444.6426 ms |    1146.58 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectBs       |  2,676.6470 ms | 181.9195 ms | 108.2574 ms |  2,632.4158 ms |  2,580.7118 ms |     371.58 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      GinArrayDirectHs       |  2,888.0231 ms |  46.5453 ms |  30.7868 ms |  2,897.2575 ms |  2,805.8092 ms |    1146.25 KB |
| StQueryBenchmarkExtended    | QueryExtended      | Pool GinArrayDirectHs       |  2,903.9930 ms |  15.8391 ms |   9.4256 ms |  2,904.0922 ms |  2,889.1052 ms |     371.86 KB |
| StQueryBenchmarkExtended    | QueryExtended      |      Legacy                 | 18,505.9374 ms | 382.0943 ms | 252.7317 ms | 18,450.4320 ms | 18,183.6857 ms |     402.88 KB |
|                             |                    |                             |                |             |             |                |                |               |
| StQueryBenchmarkReduced     | QueryReduced       |      GinArrayMergeFilter    |    306.3877 ms |   1.1852 ms |   0.7053 ms |    306.6587 ms |    305.2047 ms |     747.40 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinArrayDirect         |    308.0957 ms |   3.6549 ms |   2.1750 ms |    308.7511 ms |    304.3222 ms |     720.20 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinArrayMergeFilter    |    313.2230 ms |   4.8503 ms |   2.8863 ms |    314.3398 ms |    308.0543 ms |     401.59 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinArrayDirect         |    328.6123 ms |   1.0124 ms |   0.6025 ms |    328.4560 ms |    327.7538 ms |     401.62 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinArrayDirectFilterLs |    439.5417 ms |  16.1484 ms |  10.6812 ms |    438.5344 ms |    422.8104 ms |     662.28 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinArrayDirectFilterLs |    441.2654 ms |  18.3577 ms |  12.1425 ms |    441.4719 ms |    422.1946 ms |     401.91 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimized           |    581.8839 ms |  15.1924 ms |  10.0488 ms |    582.5331 ms |    564.7412 ms |     402.00 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinArrayDirectFilterBs |    586.3139 ms |  23.0486 ms |  15.2452 ms |    586.6126 ms |    561.9505 ms |     662.52 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinArrayDirectFilterBs |    596.9789 ms |  23.5184 ms |  15.5560 ms |    598.9053 ms |    564.6530 ms |     402.23 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinMergeFilter         |    640.4180 ms |  20.6389 ms |  13.6514 ms |    644.0766 ms |    614.5675 ms |     401.91 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinMergeFilter         |    653.2734 ms |  15.6139 ms |  10.3276 ms |    658.4660 ms |    633.8979 ms |     691.55 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinFast                |    660.6310 ms |  19.0117 ms |  12.5750 ms |    664.8560 ms |    635.4691 ms |     401.91 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinMerge               |    694.3952 ms |  39.8596 ms |  23.7198 ms |    693.6929 ms |    668.5613 ms |     664.91 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinMerge               |    735.7934 ms |  14.2089 ms |   9.3983 ms |    740.4752 ms |    714.9846 ms |     401.63 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinArrayDirectFilterHs |    777.2861 ms |  34.2544 ms |  22.6572 ms |    780.2264 ms |    744.7514 ms |     662.84 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinArrayDirectFilterHs |    799.7542 ms |  25.6471 ms |  16.9640 ms |    805.8012 ms |    774.3026 ms |     401.91 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinOptimizedFilter     |    883.4020 ms |   8.2163 ms |   5.4345 ms |    885.8877 ms |    872.4965 ms |     402.23 KB |
| StQueryBenchmarkReduced     | QueryReduced       | Pool GinFastFilter          |  1,046.1938 ms |  16.8407 ms |  11.1391 ms |  1,046.4821 ms |  1,022.3485 ms |     402.23 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinOptimizedFilter     |  1,103.2213 ms |   7.6235 ms |   4.5366 ms |  1,102.7676 ms |  1,098.5333 ms |  394182.90 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinFastFilter          |  1,306.5152 ms |  16.3988 ms |  10.8468 ms |  1,308.8728 ms |  1,279.8571 ms |  435393.77 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinFast                |  1,400.7345 ms |  17.6191 ms |  10.4849 ms |  1,399.4869 ms |  1,386.1603 ms |  889307.11 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      GinOptimized           |  1,619.9241 ms |  27.0946 ms |  16.1236 ms |  1,620.7804 ms |  1,594.5559 ms | 1657729.98 KB |
| StQueryBenchmarkReduced     | QueryReduced       |      Legacy                 | 16,410.4443 ms | 232.3982 ms | 153.7170 ms | 16,467.0588 ms | 16,161.5365 ms |     464.55 KB |
```

* коммит: ...
