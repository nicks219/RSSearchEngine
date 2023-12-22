# RS Search Engine: "Tag IT"
#### Веб-сервис с каталогом для организация и поиска небольших заметок в т.ч. по тегам
#### Главная фишка функционала - текстовый поиск, который выдерживает существенные синтаксические ошибки

# Технологии
* ```bash
  .NET 7 | TypeScript React | MSTest | Docker
  ```    
* ```bash
  MySQL | SQLite | EF
  ```
---------------------------------------------

## Дополнительное описание функционала
* Для вычисления поискового индекса используется проприентарный алгоритм токенизации
* Запрос на нечеткий текстовый поиск вернет массив с индексами релевантности для заметок: 
  ```bash
  /api/find/{string} вернет json {res: [id, weight]}
  ```
* Ссылки в заметках - кликабельны
* По выбранным тегам просмотр заметок идёт с round robin либо рандомно
* Для добавления нового тега используйте в названии заметки прямоугольные скобки
* Для увеличения веса слова применяйте ```@``` в заметках и при поиске (актуально при отсутствии ошибок)
* Синхронизация локальных поисковых индексов между разными подами сервиса будет доработана 
в будущих версиях
* Изначально сервис задумывался как каталог исключительно для песен, что повлияло на именования в коде и таблицах бд

## Локальная разработка
* Сборка фронта:
  - **npm install && npm run build** в папке ```Rsse.Front/ClientApp```
  - либо запустите скрипты **install** и **build**
* Запуск фронта:
  - выполните команду **npm start** в папке ```Rsse.Front/ClientApp/build```
  - либо запустите скрипт **start**
* Т.к. активирован CORS, используйте `127.0.0.1` вместо `localohost` для корректной работы авторизации
* Для билда и подъёма сервиса используйте ```docker-compose``` и ```Dockerfile``` файлы из папки с проектами
* Для разработки фронта использовались версии: Node 21.5.0 - npm 10.2.4 - React 17.0.0
* Дополнительная информация и шпаргалки по синтаксису докера можно найти в папке ```docs```

## Дополнительная информация для разработки

* БД:
  - при первом запуске будет создана бд `tagit` с таблицей авторизации. При ошибке проверьте корректность строки подключения в `appsettings`.
  - для накатки базы из дампа скопируйте файл с дампом бд, именуемый `backup_9.txt`, в папку `Rsse.Base/ClientApp/build` перед сборкой docker-образа.
    После подъёма контейнера запустите команду **Каталог>Restore** в меню сервиса.
  - для демонстрации функционала в VCS закоммитан файл с каталогом из 915 песен.
  - для запуска интеграционных тестов поднимается тестовая SQLite бд (в файле)
* API
  - под development доступна ручка **/swagger/v1/swagger.json**
  - также в **Rsse.Base/Controller** закоммитан **api.v5.json**

