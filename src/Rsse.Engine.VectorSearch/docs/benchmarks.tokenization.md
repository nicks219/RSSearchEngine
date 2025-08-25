## Разработка и оптимизация алгортимов поиска сервиса RSSE

### Идеи и варианты реализации: **[Viktor Loshmanov](https://github.com/ViktorLoshmanov)**

---
### Результаты бенчмарков токенизации

* коммит: начальный (x100)
```
| Method              | IndexType                   | Mean       | Error    | StdDev   | Min        | Allocated     |
|-------------------- |---------------------------- |-----------:|---------:|---------:|-----------:|--------------:|
| InitializeTokenizer | GeneralDirect               | 4,515.5 ms | 15.22 ms |  9.05 ms | 4,497.3 ms |  375654.66 KB |
| InitializeTokenizer | InvertedOffsetIndexExtended | 5,728.3 ms | 32.48 ms | 21.48 ms | 5,700.2 ms | 1810451.63 KB |
| InitializeTokenizer | InvertedIndexExtended       | 6,368.8 ms | 25.25 ms | 13.20 ms | 6,352.9 ms | 2994310.00 KB |
| InitializeTokenizer | InvertedIndexHsExtended     | 7,520.3 ms | 20.99 ms | 12.49 ms | 7,494.0 ms | 6412446.59 KB |
```

* коммит: доработка поискового движка (оптимизация памяти выделяемой токенайзером) (x100)
```
| Method              | IndexType                   | Mean       | Error    | StdDev   | Min        | Allocated     |
|-------------------- |---------------------------- |-----------:|---------:|---------:|-----------:|--------------:|
| InitializeTokenizer | GeneralDirect               | 2,936.1 ms | 22.67 ms | 14.99 ms | 2,905.6 ms |  134652.32 KB |
| InitializeTokenizer | InvertedOffsetIndexExtended | 4,102.9 ms | 28.34 ms | 18.75 ms | 4,081.0 ms | 1569449.90 KB |
| InitializeTokenizer | InvertedIndexExtended       | 4,944.1 ms | 55.18 ms | 36.50 ms | 4,897.0 ms | 2753310.45 KB |
| InitializeTokenizer | InvertedIndexHsExtended     | 6,009.4 ms | 32.82 ms | 17.17 ms | 5,989.3 ms | 6171443.64 KB |
```

* коммит: доработка поискового движка (добавлено хранение данных документа в одном массиве GinArrayDirect ддя reduced) (x100)
```
| Method              | IndexType                   | Mean       | Error    | StdDev   | Min        | Allocated     |
|-------------------- |---------------------------- |-----------:|---------:|---------:|-----------:|--------------:|
| InitializeTokenizer | GeneralDirect               | 2,881.2 ms | 31.92 ms | 16.70 ms | 2,840.5 ms |  134652.32 KB |
| InitializeTokenizer | InvertedOffsetIndexExtended | 4,023.9 ms | 17.33 ms | 10.31 ms | 4,004.4 ms | 1569442.99 KB |
| InitializeTokenizer | InvertedIndexReduced        | 4,537.1 ms | 34.54 ms | 22.85 ms | 4,515.9 ms | 2425572.09 KB |
| InitializeTokenizer | InvertedIndexExtended       | 4,768.9 ms | 43.08 ms | 28.49 ms | 4,712.8 ms | 2753314.00 KB |
| InitializeTokenizer | InvertedIndexHsReduced      | 5,435.5 ms | 25.41 ms | 16.81 ms | 5,419.4 ms | 5258286.80 KB |
| InitializeTokenizer | InvertedIndexHsExtended     | 5,927.3 ms | 13.25 ms |  6.93 ms | 5,921.2 ms | 6171444.79 KB |
```

* коммит: доработка поискового движка (оптимизация индексов, добавлено партицирование индексов) (x100)
```
| Method              | IndexType                   | Mean       | Error    | StdDev    | Min        | Allocated     |
|-------------------- |---------------------------- |-----------:|---------:|----------:|-----------:|--------------:|
| InitializeTokenizer | GeneralDirect               | 2,857.5 ms | 287.0 ms | 189.83 ms | 2,574.7 ms |  134652.37 KB |
| InitializeTokenizer | InvertedOffsetIndexExtended | 4,228.1 ms | 188.7 ms | 124.80 ms | 4,059.4 ms | 1559829.59 KB |
| InitializeTokenizer | InvertedIndexReduced        | 4,402.2 ms | 157.2 ms | 104.01 ms | 4,261.3 ms | 2401119.41 KB |
| InitializeTokenizer | InvertedIndexExtended       | 4,681.1 ms | 181.5 ms | 120.06 ms | 4,526.2 ms | 2728469.50 KB |
| InitializeTokenizer | InvertedIndexHsReduced      | 5,470.0 ms | 165.3 ms | 109.35 ms | 5,326.0 ms | 5233832.69 KB |
| InitializeTokenizer | InvertedIndexHsExtended     | 5,795.5 ms | 167.3 ms | 110.66 ms | 5,608.4 ms | 6146603.63 KB |
```

* коммит: ...
