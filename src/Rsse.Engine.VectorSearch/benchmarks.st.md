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

* коммит: ...
