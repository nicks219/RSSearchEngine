I. запуск с чистой "оптимистичной" 10% отсечкой, без использования RemoveMostFrequentTokens.
видно, что для SimpleLegacy память аллоцируется пропорционально объему запросов.

| Method            | SearchType        | Mean      | Error     | StdDev   | Min       | Allocated   |
|------------------ |------------------ |----------:|----------:|---------:|----------:|------------:|
| DuplicatesReduced | Pool SimpleLegacy |  47.22 ms |  1.724 ms | 0.448 ms |  46.62 ms |           - |
| DuplicatesReduced |      SimpleLegacy |  57.92 ms |  8.646 ms | 2.245 ms |  56.16 ms | 30756.65 KB |
| DuplicatesReduced | Pool Direct       | 252.91 ms |  3.059 ms | 0.794 ms | 251.89 ms |     0.02 KB |
| DuplicatesReduced |      Direct       | 261.76 ms | 20.218 ms | 3.129 ms | 258.29 ms |  5934.34 KB |

| Method       | SearchType        | Mean      | Error     | StdDev    | Min       | Allocated |
|------------- |------------------ |----------:|----------:|----------:|----------:|----------:|
| QueryReduced | Pool SimpleLegacy | 0.0811 ms | 0.0021 ms | 0.0003 ms | 0.0807 ms |         - |
| QueryReduced |      SimpleLegacy | 0.1928 ms | 0.0208 ms | 0.0032 ms | 0.1891 ms |  317.8 KB |
| QueryReduced |      Direct       | 0.2963 ms | 0.0064 ms | 0.0017 ms | 0.2940 ms |   0.53 KB |
| QueryReduced | Pool Direct       | 0.3640 ms | 0.0054 ms | 0.0008 ms | 0.3628 ms |         - |

| Method       | SearchType        | Mean        | Error      | StdDev    | Min         | Allocated     |
|------------- |------------------ |------------:|-----------:|----------:|------------:|--------------:|
| QueryReduced | Pool SimpleLegacy |    59.19 ms |   6.329 ms |  1.644 ms |    57.08 ms |     565.77 KB |
| QueryReduced |      Direct       |    99.99 ms |  50.499 ms |  7.815 ms |    88.31 ms |     884.44 KB |
| QueryReduced | Pool Direct       |   136.91 ms |   3.086 ms |  0.478 ms |   136.39 ms |     565.77 KB |
| QueryReduced |      SimpleLegacy |   249.13 ms |  34.794 ms |  9.036 ms |   233.44 ms | 1193960.09 KB |
| QueryReduced |      Legacy       | 1,611.88 ms | 350.496 ms | 54.240 ms | 1,532.09 ms |     569.49 KB |

II. ...
