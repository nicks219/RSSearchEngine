# План развития сервиса:
**2025**
* ✅ Поставка диагностиков по OTLP в Grafana Cloud с otel-collector локально и в кластере
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
* **Планы обновлений**: `описание процесса` 
    * ✅ обновиться до актуальных версий на начало **2026** года
    * ✅ обновить Rider `2025.2.3` -> `2025.3.1` для поддержки **NET10** (2025.2.3 проблемы на билде)
    * ✅ поднять версии до **NET10** (перенес в [Directory.Build.props](src/Directory.Build.props))
    * ✅ обновить зависимости NET проекта
      * ждём релиза `Pomelo.EntityFrameworkCore.MySql` -> замена на `MySql.EntityFrameworkCore` | `MySqlConnector`
      * `MSTest.TestFramework` выше 3.11.1 отвалятся атрибуты `[ClassCleanup(ClassCleanupBehavior.EndOfClass)]`
    * ✅ поднять **React** до `19.2.3`
      * напоминалка: npm outdated | npm update | npm install <package>@latest | npm run build | npx npm-check-updates | npx npm-check-updates -u | npm install
      * обновить Node: nvm install 22.12.0 | nvm use 22.12.0 | 22.21.1 LTS с сайта
      * если ругается на eslint: npm install @typescript-eslint/eslint-plugin@8.51.0 @typescript-eslint/parser@8.51.0 --save-exact | npm install
      * поправить ошибки, закэшировался старый сертификат: chrome://settings/clearBrowserData | dotnet dev-certs https --clean | либо vite создаст при запуске
    * ✅ проверить зависимости по dependabot: vite ~> 6.3.6 | js-yaml ~> 4.1.1 | glob ~> 10.5.0 (High severity)
    * ✅ протестировать обновленный функционал
      * прогнать тесты, чиним:
        * Rsse.Tests: добавил wait warmup
        * перенес ответ 403 в IAuthorizationMiddlewareResultHandler
        * синхронизировал версии MSTest.TestAdapter и MSTest.TestFramework
        * Rsse.Integration.Tests: добавил нового провайдера в seed
        * проблема: в интеграционных тестах ActivatorService многократно запускается/падает (на Delay) по отмене, хотя stoppingToken = false
          * причина: очистка хоста на запуск каждого теста (а также создание/очистка хостов для прогрева), лог общий, хотя и выводится фрагментировано
          * решение: для не зависящих от состояния тестов оставлен один тестовый хост
          * для зависящих от состояния тестов убраны дополнительные хосты для прогрева, далее можно заменить на очистку индексов по требованию
      * подняться локально, накатить дамп:
        * при удалении заметки: Unable to cast object of type 'MySqlConnector.MySqlConnection' to type 'MySql.Data.MySqlClient.MySqlConnection'
          * решение: до замены провайдера на Pomelo подключаем UseMySql через строку подключения
        * токенайзер падает при сохранении записи "123" (запусти тесты на CheckIsProduction:false) System.IndexOutOfRangeException BucketHelper:396
          * решение: SearchEngineTests: фикс и правка от автора кода для обработки пустых коллекций
          * дополнительно добавлен выброс исключения при попытке запуска AlgorithmSelector на проде
    * ✅ в пайплайнах обновить версии образов для билда NET/React (если требуется)
      * ci.dotnet.build | ci.node.js.build | Dockerfile-net-react (билд для k3s, проверить локально)
    * ✅ обновить версию сервиса в ApplicationFullName и в README
    * `.` поднять версии сервисов в кластере: Pg 18.1 | MySql 8.4 -> 9.5
    * ✅ обновить зависимости кластера k3s
    * ✅ обновить SSL-сертификат, подумать о переходе на `cert-manager`
    * `.` переместить разработку поиска в независимый RD-проект, в сервисе оставить используемый в проде код
	* ✅ security fix: обновить react-router-dom до 17.12.0
    * ...


---
# техдолг

* `.` DestructiveTests: сервис (фабрика) хранит состояние (поисковые индексы), добавить метод очистки для тестов (сейчас на каждый тест новая фабрика)
* ✅ BucketHelper: добавлен quick fix на пустую запись, исправлен на фикс от автора (CheckIsProduction:false)
* `.` продумать, как тестировать сервис, учитывая production mode для индексов, вынеси все проверки среды в один метод, добавлять зависимости консистентно
* `.` тест Migration_Requests_ShouldApplyCorrectly локально бывает нестабилен (не находит таблицу), исследовать
* ✅ Проверить новый образ на **NET10/alpine:3.22.2** (сборка/функционал)
  * сборка: Directory.Build.props уровнем выше над контекстом, продублировал, добавил в докерфайл. Симлинк не помог: `ln -s ../Directory.Build.props .`
  * запуск: через compose, судя по логам начальные таблицы не инициализированы, также: Cannot load library libgssapi_krb5.so.2
    * при повторном запуске поднялся стабильно, т.е не инициализируется минимальный дамп, возможно стоит добавить: krb5-libs | libssl3 | icu-libs zlib
    * решение: добавил хелсчеки в compose
    * docker compose -f docker-compose-build.yml up --force-recreate
    * docker-compose -f docker-compose-build.yml down --rmi all --volumes --remove-orphans
    * docker compose -f docker-compose-build.yml up -d --wait
* ...

---
# подпись GPG для git

* следует редактировать строго через git bash 
  * gpg --quick-gen-key "имя@почта" rsa2048 sign 0
  * gpg --list-secret-keys --keyid-format SHORT "имя@почта"
  * git config --global user.signingkey КЛЮЧ
  * git config user.signingkey
  * gpg --armor --export КЛЮЧ
  * GitHub → Profile Settings → SSH and GPG keys → New GPG key → вставь
  * настройки GE: C:\Users\%USERNAME%\AppData\Roaming\GitExtensions\GitExtensions\GitExtensions.settings
  * ...
