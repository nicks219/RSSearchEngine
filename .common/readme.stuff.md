# Информация из readme

* [Дополнительное описание функционала](#дополнительное-описание-функционала)
* [Локальная разработка](#локальная-разработка)
* [CI/CD](#пайплайны)
* [Дополнительная информация для разработки](#дополнительная-информация-для-разработки)
---------------------------------------------


## Дополнительное описание функционала
* Для вычисления поискового индекса используется проприентарный алгоритм токенизации
* Запрос на нечеткий текстовый поиск вернет массив с индексами релевантности для заметок:
  ```text
  /api/compliance/indices/{string} вернет json {res: [id, weight]}
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
* Версии библиотек зафиксированы в **[CHANGELOG.md](CHANGELOG.md)**
* Дополнительную полезную информацию можно найти в папке ```.common```

## Пайплайны
* В процессе разработки: добавлены отдельные конвейеры прогона сборки/тестирования проектов .NET/Node,
  билда и публикации докер-образов в GitHub Registry, а также запуска деплоя на удалённом хостинге
  и развертывания сервиса в кластере kubernetes k3s. Манифесты кубера находятся в папке ```.k3s```

## Дополнительная информация для разработки
* БД:
    - при первом запуске будет создана бд `tagit` с таблицей авторизации. При ошибке проверьте корректность строки подключения в `appsettings`.
    - для накатки базы из дампа скопируйте файл с дампом бд, именуемый `backup_9.txt`, в папку `Rsse.Service/ClientApp/build` перед сборкой docker-образа.
      После подъёма контейнера запустите команду **Каталог>Restore** в меню сервиса.
    - для демонстрации функционала в VCS закоммитан файл с каталогом из 915 песен.
    - для запуска интеграционных тестов поднимается тестовая SQLite бд (в файле)
* API
    - актуальная версия доступна онлайн на **[bumps.sh](https://bump.sh/nicks219/doc/rsse)**
    - в development доступен **Swagger**
    - swagger.json закоммитан в папке **.common** под именем **rsse.api.x.x.json**
* Фронт:
    - используется React Router, т.е. возможна навигация
* Kubernetes:
    - ингресс с сертификатом DV SSL в секретах должен выглядеть так:
      ```text
      NAME               CLASS     HOSTS           ADDRESS         PORTS     AGE
      rsse-app-ingress   traefik   maintainer.me   82.146.45.180   80, 443   46h
      ```
    - манифест mysql для k3s совместо с конфигурацией создаст примерно такой ресурс:
      ```text
      NAME    TYPE           CLUSTER-IP     EXTERNAL-IP     PORT(S)          AGE
      mysql   ClusterIP      10.43.74.35    <none>          3306/TCP         46h
      ```
--------------------------------------------------
## Разнообразные шпаргалки, пусть будут

* **коммит rsse образа из билда (залогиниться в Docker Desktop)**:
cd src =)  папка с ямлом, запуск из-под wsl
cd /mnt/c/Users/nick/source/repos/tagit/src && \
docker-compose down && \
docker image rm nick219nick/tagit:v5 && \
docker-compose -f docker-compose-build.yml build && \
docker-compose -f docker-compose-build.yml up -d && \
R_C_HASH=`docker ps | grep tagit | grep -E -o "^\S+"` && \
docker commit -a Nick219 -m v5 ${R_C_HASH} nick219nick/tagit:v5 && \
docker push nick219nick/tagit:v5;


* **подъем образа на хосте**:
cd scr =) папка с ямлом
docker compose down && \
docker image rm nick219nick/tagit:v5 && \
docker compose up -d;


* **тестовые сценарии**: адреса могут быть неактуальны:
curl http://45.95.202.39:5000/gc/ & curl http://45.95.202.39:5000/start/ & ab -n1000 -c100 http://45.95.202.39:5000/test/ & curl http://45.95.202.39:5000/stop/
ab -n10 -c1 http://45.95.202.39:5000/testmega?id=true


* **снять дамп**: выделит первое слово до пробела и создаст дамп на хосте по пути ./rsse: 
DB_HASH=`docker ps | grep mysql_t | grep -E -o "^\S+"` && \
docker exec -it $DB_HASH mysqldump --host=localhost --user=root --password=1 tagit > ./tagit.dump 


* **подъем локально**:
docker-compose -f docker-compose-up.yml up -d


* **накатить дамп**:
DB_HASH=`docker ps | grep mysql_t | grep -E -o "^\S+"` && \
docker exec -i $DB_HASH mysql --host=localhost --user=root --password=1 tagit < tagit.dump 


* **билд докер-образа**:
docker build -t nick219nick/tagit:v5 .


* **MySQL**:  
  GRANT ALL PRIVILEGES ON *.* TO '1'@'%';  
  SET PASSWORD FOR '1'@'%' = '1';  
  FLUSH PRIVILEGIES;  

* root@nick2192:~# kubectl apply -f resource.postgres.yml
persistentvolumeclaim/postgres-pvc created
service/postgres created
deployment.apps/postgres created
# kubectl get svc postgres -o yaml

* при проблемах с авторизацией kubectl переустановил бинарь, остальное не помогало:
curl -sfL https://get.k3s.io | INSTALL_K3S_EXEC="--tls-san 82.146.45.180" sh -
