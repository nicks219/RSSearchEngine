## Разработка и оптимизация алгортимов поиска сервиса RSSE

### Идеи и варианты реализации: **[Viktor Loshmanov](https://github.com/ViktorLoshmanov)**

---
### Результаты бенчмарков токенизации

* коммит: начальный (x100)
```
| Method              | IndexType                        | Mean       | Error    | StdDev   | Min        | Allocated     |
|-------------------- |--------------------------------- |-----------:|---------:|---------:|-----------:|--------------:|
| InitializeTokenizer | GeneralDirect                    | 4,515.5 ms | 15.22 ms |  9.05 ms | 4,497.3 ms |  375654.66 KB |
| InitializeTokenizer | InvertedIndexReduced             | 5,105.5 ms | 23.00 ms | 12.03 ms | 5,085.3 ms |  438517.12 KB |
| InitializeTokenizer | InvertedOffsetIndexExtended      | 5,728.3 ms | 32.48 ms | 21.48 ms | 5,700.2 ms | 1810451.63 KB |
| InitializeTokenizer | ArrayDirectOffsetIndexExtended   | 6,368.8 ms | 25.25 ms | 13.20 ms | 6,352.9 ms | 2994310.00 KB |
| InitializeTokenizer | ArrayDirectOffsetIndexHsExtended | 7,520.3 ms | 20.99 ms | 12.49 ms | 7,494.0 ms | 6412446.59 KB |
```

* коммит: доработка поискового движка (оптимизация памяти выделяемой токенайзером) (x100)
```
| Method              | IndexType                        | Mean       | Error    | StdDev   | Min        | Allocated     |
|-------------------- |--------------------------------- |-----------:|---------:|---------:|-----------:|--------------:|
| InitializeTokenizer | GeneralDirect                    | 2,936.1 ms | 22.67 ms | 14.99 ms | 2,905.6 ms |  134652.32 KB |
| InitializeTokenizer | InvertedIndexReduced             | 3,466.4 ms | 94.98 ms | 62.82 ms | 3,400.0 ms |  197515.10 KB |
| InitializeTokenizer | InvertedOffsetIndexExtended      | 4,102.9 ms | 28.34 ms | 18.75 ms | 4,081.0 ms | 1569449.90 KB |
| InitializeTokenizer | ArrayDirectOffsetIndexExtended   | 4,944.1 ms | 55.18 ms | 36.50 ms | 4,897.0 ms | 2753310.45 KB |
| InitializeTokenizer | ArrayDirectOffsetIndexHsExtended | 6,009.4 ms | 32.82 ms | 17.17 ms | 5,989.3 ms | 6171443.64 KB |
```

| Method              | IndexType                        | Mean       | Error     | StdDev   | Min        | Allocated     |
|-------------------- |--------------------------------- |-----------:|----------:|---------:|-----------:|--------------:|
| InitializeTokenizer | GeneralDirect                    | 2,976.8 ms |  75.26 ms | 49.78 ms | 2,882.7 ms |  134652.32 KB |
| InitializeTokenizer | InvertedIndexReduced             | 3,640.7 ms | 101.38 ms | 67.06 ms | 3,526.2 ms |  197514.77 KB |
| InitializeTokenizer | InvertedOffsetIndexExtended      | 4,260.9 ms |  65.99 ms | 34.51 ms | 4,180.8 ms | 1569448.87 KB |
| InitializeTokenizer | ArrayDirectOffsetIndexReduced    | 4,645.4 ms |  49.22 ms | 32.56 ms | 4,606.0 ms | 2425572.09 KB |
| InitializeTokenizer | ArrayDirectOffsetIndexExtended   | 4,932.9 ms |  61.55 ms | 36.63 ms | 4,863.2 ms | 2753308.78 KB |
| InitializeTokenizer | ArrayDirectOffsetIndexHsReduced  | 5,702.7 ms |  92.13 ms | 60.94 ms | 5,638.3 ms | 5258291.16 KB |
| InitializeTokenizer | ArrayDirectOffsetIndexHsExtended | 6,180.5 ms |  17.97 ms | 10.69 ms | 6,169.3 ms | 6171445.23 KB |

* коммит: ...
