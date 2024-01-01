![cover.png](docs%2Fcover.png)
---------------------------------------------
[![Deploy to K3S](https://github.com/nicks219/RSSearchEngine/actions/workflows/k3s-deploy.yml/badge.svg)](https://github.com/nicks219/RSSearchEngine/actions/workflows/k3s-deploy.yml)
#### Веб-сервис с каталогом для организации, случайного выбора и поиска небольших заметок в т.ч. по тегам
#### Главная фишка функционала - нечеткий текстовый поиск, выдерживающий существенные синтаксические ошибки
#### Архитектура оптимизирована для работы контейнеров на бюджетном хостинге (1vCPU/1Gb RAM)

## Технологии
* ```text
  .NET 7 | MSTest | TS React | Router | Vite
  ```    
* ```text
  MySQL | SQLite | EF
  ```
* ```text
  GitHub CI/CD | Docker | K3S ready | DV SSL ready
  ```
---------------------------------------------
## Информация

* [Дополнительное описание функционала](#дополнительное-описание-функционала)
* [Локальная разработка](#локальная-разработка)
* [CI/CD](#ci-cd)
* [Дополнительная информация для разработки](#дополнительная-информация-для-разработки)
---------------------------------------------


## Дополнительное описание функционала
* Для вычисления поискового индекса используется проприентарный алгоритм токенизации
* Запрос на нечеткий текстовый поиск вернет массив с индексами релевантности для заметок: 
  ```text
  /api/find/{string} вернет json {res: [id, weight]}
  ```
* Ссылки в заметках - кликабельны
* По выбранным тегам просмотр заметок идёт с round robin либо рандомно
* Для добавления нового тега используйте в названии заметки прямоугольные скобки
* Для увеличения веса слова применяйте ```@``` в заметках и при поиске (актуально при отсутствии ошибок)
* Синхронизация локальных поисковых индексов между разными подами сервиса будет доработана 
в будущих версиях
* Изначально сервис задумывался как каталог с рандомным выводом по тегам исключительно для песен, что повлияло на именования в коде и таблицах бд

## Локальная разработка
* Для разработки и сборки фронта используется Vite, 
запуск/остановка dev-сервера JS в связке со стартом/остановкой основного сервиса доступны в Development
и на данный момент реализованы только для Windows
* Для совместного и раздельного запуска основного сервиса и фронта используйте профили из launchSettings.json
* Для самостоятельного запуска фронта также можно использовать скрипты из package.json
* Для билда и подъёма сервиса в контейнере используйте файлы ```docker-compose``` и ```Dockerfile``` из папки с проектами
* Версии библиотек зафиксированы в [CHANGELOG.md](CHANGELOG.md), использовалась Node 21.5.0 (npm 10.2.4)
* Дополнительная информация и шпаргалки по синтаксису докера можно найти в папке ```docs```

## CI CD
* В процессе разработки: добавлены пробные пайплайны прогона сборки/тестирования проектов .NET/Node, 
  билда и публикации докер-образов в GitHub Registry, а также запуска деплоя на удалённом хостинге 
  и развертывания в кластере kubernetes k3s (манифесты находятся в папке .k3s)

## Дополнительная информация для разработки
* БД:
  - при первом запуске будет создана бд `tagit` с таблицей авторизации. При ошибке проверьте корректность строки подключения в `appsettings`.
  - для накатки базы из дампа скопируйте файл с дампом бд, именуемый `backup_9.txt`, в папку `Rsse.Service/ClientApp/build` перед сборкой docker-образа.
    После подъёма контейнера запустите команду **Каталог>Restore** в меню сервиса.
  - для демонстрации функционала в VCS закоммитан файл с каталогом из 915 песен.
  - для запуска интеграционных тестов поднимается тестовая SQLite бд (в файле)
* API
  - под development доступна ручка **/swagger/v1/swagger.json**
  - также в **Rsse.Service/Controller** закоммитан **api.v5.json**
* Фронт:
  - используется React Router, т.е. возможна навигация
* Kubernetes:
    - ингресс использует DV SSL сертификат от Comodo
    - манифест mysql для k3s совместо с конфигурацией создаст примерно такой ресурс:
      ```text
      NAME    TYPE           CLUSTER-IP     EXTERNAL-IP     PORT(S)          AGE
      mysql   LoadBalancer   10.43.153.59   82.146.45.180   3306:30532/TCP   2m55s
      ```
--------------------------------------------------

