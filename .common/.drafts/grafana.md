## Pyroscope
#### ветка **release/6.0.3** `исследование` `неудовлетворительнo`

* upd: настроить сбор профилей с помощью Pyroscope `0.9.4` (файлы `*.v2`)
    - [x] проверить сборку образа с pyroscope `dp6.0.3` (тег `dp*.*.*`)
    - [x] проверить локально отправку профилей
        - загрузка образа `docker pull ghcr.io/nicks219/rssearchengine:dp6.0.3`
        - запуск контейнера `docker run --env-file ./v2.env.pwd -p 5100:5000 -e ASPNETCORE_ENVIRONMENT=Development --name dp6.0.3 ghcr.io/nicks219/rssearchengine:dp6.0.3`
    - [x] настроить datasource в Grafana Cloud и данные для авторизации
        - user на Pyroscope совпадает с OTLP, при этом создание токена на Pyroscope не аффектит OTLP-токен
    - [x] определиться с эндпоинтом
        - актуален эндпоинт датасорса https://profiles-prod-001.grafana.net
        - эндпоинт OLTP в данной конфигурации не актуален (возможно из-за неправильного токена, принадлежащего датасорсу)
    - [x] проверить отправку профилей с пода деплоя `kp6.0.3` (тег `kp*.*.*`)
        - необходим актуальный список переменных с проверенными значениями
        - можно включить режимы профилирования [по документации](https://grafana.com/docs/pyroscope/next/configure-client/language-sdks/dotnet/#send-data-to-pyroscope-oss-or-grafana-cloud-profiles)
        - см. отчет ниже по тексту
    * todo: попробовать убрать зависимости профилирования (NET-докерфайл-манифест-пайплайны) под переменную
    * todo: попробовать [апгрейд до `1.x`](https://grafana.com/docs/pyroscope/next/upgrade-guide/#upgrade-to-pyroscope-10)
        - настроить отдачу профилей по OTLP
        - настроить otel для приема/отправки профилей


* с локального запуска профили поставляются, переменные правильные
* **результат на деплое неудовлетворительный, раскатка на 1vCPU 2Gb прошла c трудом, функционал фризит, откатился на `k6.0.2`**
* профили с пода поставлялись, связь с профилями присутствовала в трассах и вела на запрос:
`{service_name="rsse-app", service_namespace="rsse-group"}`, который давал пустой ответ по временному диапазону, 
запрос `{service_name="rsse-app"}` отдал диапазон профиля.
* **пример логов от pyroscope с пода на выкладке**
  ```aiignore
  The ongoing StackSampleCollection duration crossed the threshold. A deadlock intervention was performed.
  ```
---

## Exemplars
#### ветка **release/6.0.2** `ok`
#### процесс поиска решения
* upd: связь метрик и трейсов через exemplar (на проверке)
    * выборка из exemplar в атрибут traceID с помощью процессоров otel невозможна (проверено)
    * добавление кастомного лейбла traceID приведет к высокой кардинальности,
      но на метрике при PromQL запросе `histogram_quantile(0.95, sum(rate(http_server_duration_with_trace_bucket{}[$__rate_interval])) by (le))`
      в Grafana Cloud появится "синяя кнопка": `Query with grafanacloud-...-traces` (проверено)
    * попробовать отправить метрики в формате open metrics (идея)
  ```yaml
  exporters:
  prometheusremotewrite/grafana_cloud:
  endpoint: https://prometheus-blocks-prod-us-central1.grafana.net/api/prom/push
  headers:
  Authorization: Basic <base64(api_key)>
  send_exemplars: true
  ```
  ```yaml
  exporters: [prometheusremotewrite/grafana_cloud]
  ```
* почитать про фичи Grafana по связке датасорсов, например:
    - [Trace correlaction](https://grafana.com/docs/grafana/latest/datasources/tempo/traces-in-grafana/trace-correlations/)
    - [Trace to metrics](https://grafana.com/docs/grafana/next/datasources/tempo/configure-tempo-data-source/#trace-to-metrics)
    - [Derived fields](https://grafana.com/docs/grafana/next/datasources/loki/configure-loki-data-source/#derived-fields)
* **проверенное решение: создать кастомный датасорс, разобраться с авторизацией и заменить поле экземпляра traceID на trace_id**
---
