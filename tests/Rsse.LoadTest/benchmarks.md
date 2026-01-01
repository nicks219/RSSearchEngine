#### Идеи для оптимизации
* [ ] Сериализация: настроить `System.Text.Json`
* [ ] Middleware: посмотреть пайплайн запроса ASP (SwaggerMiddleware | RL | DeveloperExceptionPage)
* [ ] OTLP: разобраться, как профилировать и настроить

---
**результаты нагрузочного тестирования на текущей версии кода**

* copies: 6 | warmup: 1 sec | during: 00:00:15

| ручка    | RPS | RAM (dotTrace)        |
|----------|-----|-----------------------|
| election | 190 | SQL:196MB/total:319MB |

* copies: 100 | warmup: 1 sec | during: 00:00:15

| ручка    | RPS | RAM |
|----------|-----|-----|
| election | 257 | ... |

---
