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
| Method              | IndexType                        | Mean       | Error     | StdDev   | Min        | Allocated     |
|-------------------- |--------------------------------- |-----------:|----------:|---------:|-----------:|--------------:|
| InitializeTokenizer | GeneralDirect                    | 4,164.5 ms |  53.23 ms | 35.21 ms | 4,119.5 ms |  134652.04 KB |
| InitializeTokenizer | InvertedIndexReduced             | 4,536.1 ms |  16.73 ms | 11.07 ms | 4,512.0 ms |  197515.10 KB |
| InitializeTokenizer | InvertedOffsetIndexExtended      | 5,549.3 ms | 113.61 ms | 75.15 ms | 5,430.4 ms | 1569449.77 KB |
| InitializeTokenizer | ArrayDirectOffsetIndexExtended   | 5,916.1 ms |  28.90 ms | 19.12 ms | 5,889.2 ms | 2753313.47 KB |
| InitializeTokenizer | ArrayDirectOffsetIndexHsExtended | 7,243.6 ms |  99.51 ms | 65.82 ms | 7,152.6 ms | 6171444.98 KB |
```

* коммит: ...
