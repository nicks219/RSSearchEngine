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

* коммит: ...
