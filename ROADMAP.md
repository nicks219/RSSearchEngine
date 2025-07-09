# План развития сервиса:
**2025**
* ✅ Поставка диагностиков по OTLP в Grafana Cloud с otel-collector локально и в кластере
* Добавление профилирования через Pryroscope, настройка связей для observability в Grafana Cloud 
* Интеграция с Aspire для локальной разработки
* Отказ от MySql
* Автоматизация версионирования, для кластера использовать `kustomization.yaml`  
  https://kubernetes.io/docs/tasks/manage-kubernetes-objects/kustomization/

---
# Запланированные изменения:
**перенесено из changelog**
* **БД**
    * ✅ перейти с MySQL на Postgres,
    * ✅ функционал дампа дополнить раздельными миграциями схемы и данных
    * ✅ добавить k3s ресурсы для Postgres
* **Тесты**
    * юниты для JS
    * подготовить среду для E2E тестов, покрыть код автотестами
    * ✅ интеграционные в отдельном проекте, бд в контейнерах/services на ci/cd
* **React**:
    * ✅ упростить контейнер со стейтом
    * ~~подумать над применимостью в клиенте Flux/Redux~~
* **GitHub CI**:
    * соединить пайплайны, например, завязать публикацию образа на выпуск релиза
      и использовать для него артефакты билда сервиса и фронта
* **GitHub CD**:
    * подготовить варианты интеграции с высокодоступным кластером: сейчас сервис задеплоен на k3s
* **Permanent**:
    * обеспечить дополнительную сохранность данных при необратимых действиях пользователя
* **UI**:
    * ✅ разделить верстку для мобильных устройств и десктопа, добавить modal-окна
        * вынести общие для всех устройств стили в отдельный файл CSS
        * ✅ добавить специфичные для устройств стили в отдельный файл CSS
    * вынести системный функционал в отдельную админку, переписать компонент с авторизацией
    * добавить понятный пользователю вывод ошибок
    * ✅ на стороне клиента: увеличить длину списка похожих до 20
    * базовый функционал - нечеткий поиск, отображение тегов - находится под авторизацией,  
      для демонстрации возможностей (а не только факта деплоя) следует создать компонент поиска/отображения тегов
    * генерировать контракты для фронта по open api либо классам шарпа
    * поднять версию vite c 6.3.3 (придется поднимать версию Node.js)
* **Планы обновлений**:
    * ✅ поднять версии до NET9 | Pg 17.4 | React 19
        * ✅ актуализировать пайплайн [ci.dotnet.build.yml](.github/workflows/ci.dotnet.build.yml)
        * ✅ актуализировать [Dockerfile-net-react](src/Dockerfile-net-react) и проверить пайплайн деплоя [cd.deploy.k3s.yml](.github/workflows/cd.deploy.k3s.yml)
    * ✅ подключить pvc для сервиса (ворнинг по поводу ключей)
    * ✅ использовать на пайплайне services для интеграционных тестов
    * ✅ почистить сборку, убрать лишние локализации
    * актуализировать версии сервиса до `6.0.0` в документации:
        * ✅ в package.json
        * ✅ зафиксировать в документации и [README.md](README.md)
        * зафиксировать в system/version
        * создать релиз

---
# Ссылки на документацию:
  * Grafana Cloud
    - [Trace correlaction](https://grafana.com/docs/grafana/latest/datasources/tempo/traces-in-grafana/trace-correlations/)
    - [Trace to metrics](https://grafana.com/docs/grafana/next/datasources/tempo/configure-tempo-data-source/#trace-to-metrics)
    - [Derived fields](https://grafana.com/docs/grafana/next/datasources/loki/configure-loki-data-source/#derived-fields)
    - [.NET Pyroscope](https://grafana.com/docs/pyroscope/latest/configure-client/language-sdks/dotnet/?utm_source=chatgpt.com)
    - [Traces to profiles](https://grafana.com/docs/pyroscope/latest/configure-client/trace-span-profiles/dotnet-span-profiles/?utm_source=chatgpt.com)
  * Open Telemetry SDK
    - [информация про включение самодиагностики в OpenTelemetry SDK NET](https://www.nuget.org/packages/OpenTelemetry#self-diagnostics)
  * Внутренняя документация проекта
    - [информация по созданию k3s ресурсов для проекта](.common/scripts.k3s.bash)
