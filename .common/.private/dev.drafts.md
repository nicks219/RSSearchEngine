# КОМАНДЫ ПОДЪЁМА БД
--------------------
```bash
# копия из docker desktop
docker run --hostname=c88a099fa16b --env=PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin --env=GOSU_VERSION=1.14 --env=MYSQL_MAJOR=8.0 --env=MYSQL_VERSION=8.0.31 --env=MYSQL_USER=1 --env=MYSQL_PASSWORD=1 --env=MYSQL_DATABASE=tagit --env=MYSQL_ROOT_PASSWORD=1 --volume=src_mysql-volume:/var/lib/mysql:rw --volume=/var/lib/mysql --network=bridge -p 3306:3306 --restart=no --runtime=runc -d mysql:8.0.31-debian
docker run --hostname=2aeeb8d8884d --env=POSTGRES_PASSWORD=1 --env=POSTGRES_USER=1 --env=PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/[ingress.traefik.one.yml](..%2Fingress.traefik.one.yml)sbin:/bin --env=PG_VERSION=16.2 --env=PG_SHA256=446e88294dbc2c9085ab4b7061a646fa604b4bec03521d5ea671c2e5ad9b2952 --env=POSTGRES_DB=tagit --env=LANG=en_US.utf8 --env=PG_MAJOR=16 --env=DOCKER_PG_LLVM_DEPS=llvm15-dev --env=PGDATA=/var/lib/postgresql/data --env=GOSU_VERSION=1.17 --volume=/var/lib/postgresql/data --network=bridge -p 5432:5432 --restart=no --runtime=runc -d postgres:16-alpine

# использовал при разработке миграции на postgres (команды избыточны, скопированы из docker desktop)
MY8 : docker run --env=MYSQL_USER=1 --env=MYSQL_PASSWORD=1 --env=MYSQL_DATABASE=tagit --env=MYSQL_ROOT_PASSWORD=1 --env=PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin --env=GOSU_VERSION=1.14 --env=MYSQL_MAJOR=8.0 --env=MYSQL_VERSION=8.0.31 --volume=src_mysql-volume:/var/lib/mysql:rw --volume=/var/lib/mysql -p 3306:3306 --name mysql_8 --runtime=runc -d mysql:8.0.31-debian
PG16: docker run --name pg_16 --env=POSTGRES_PASSWORD=1 --env=POSTGRES_USER=1 --env=POSTGRES_DB=tagit --env=PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin --env=LANG=en_US.utf8 --env=PG_MAJOR=16 --env=PG_VERSION=16.2 --env=PG_SHA256=446e88294dbc2c9085ab4b7061a646fa604b4bec03521d5ea671c2e5ad9b2952 --env=DOCKER_PG_LLVM_DEPS=llvm15-dev --env=PGDATA=/var/lib/postgresql/data --volume=/var/lib/postgresql/data -p 5432:5432 --restart=no --runtime=runc -d postgres:16-alpine
PG17: docker run --name pg_17 --env=POSTGRES_PASSWORD=1 --env=POSTGRES_USER=1 --env=POSTGRES_DB=tagit --env=PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin --volume=/var/lib/postgresql/data -p 5432:5432 --restart=no --runtime=runc -d postgres:17.4-alpine3.21

PG17: docker run --name pg_17 -e POSTGRES_PASSWORD=1 -e POSTGRES_USER=1 -e POSTGRES_DB=tagit --volume=/var/lib/postgresql/data -p 5432:5432 -d postgres:17.4-alpine3.21
# psql в контейнере: psql -d tagit -U 1
```

# ПОФИКСИТЬ
-----------
* [x] глюки лаунчера dev сервера
* [x] уязвимость от dependabot в rollup (надо запустить фронт)
* [x] увеличить длину текста заметки в 1.5 - 2 раза
- а не зафиксирована ли она в дампе?? точно, зафиксирован на 4000 ! как менять будем?
- зайти на сервис, поправить backup_6.txt сверху: 4000 на 10.000 и сделать restore
- вот поэтому и надо схему отдельно от данных менять
- так, а как мы заходим на сервис? 82.146.45.180:443
  kubectl get pods
  kubecel exec -it

* [x] проверь, придется ли править имя в ClientApp/build/backup_9.txt (имя актуально)
* [x] сделай modal-окно для вопроса по удалению
* [x] поправь ингресс чтобы принимал http трафик

мёрж: находясь на мастере вмерживаешь в него develop (merge into branch develop в гитэкcтеншн)
сообщение коммита будет Merge branch 'develop'

# МИГРАЦИЯ НА Postgres
----------------------
* ручка copy: переносит данные с MySql на PG, учитывая Users
* PG: SQL-скрипт создаёт DDL drop/create на 3 таблицы (кроме Users) с FK, ограничениями и правилами генерациеи PK
* выделен функционал инициализации бд тестовыми данными (следует сделать его отдельной службой)
* до переезда: стоит подумать о связанном с бд функционале - дополнительные колонки, перенос векторов в бд, шардирование

# POSTGRESQL запросы
--------------------
1. Все таблицы в public:

SELECT table_name  
FROM information_schema.tables  
WHERE table_schema = 'public' AND table_type = 'BASE TABLE';

2. Поля таблицы users:

SELECT column_name, data_type, is_nullable  
FROM information_schema.columns  
WHERE table_name = 'users' AND table_schema = 'public';

3. Ограничения таблицы orders:

SELECT constraint_name, constraint_type  
FROM information_schema.table_constraints  
WHERE table_name = 'orders' AND table_schema = 'public';

4. Внешние ключи (orders → users):

SELECT tc.constraint_name, kcu.column_name, ccu.table_name AS referenced_table  
FROM information_schema.table_constraints tc  
JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name  
JOIN information_schema.constraint_column_usage ccu ON tc.constraint_name = ccu.constraint_name  
WHERE tc.table_name = 'orders' AND tc.constraint_type = 'FOREIGN KEY';


# k3s отладка DNS
---
перезапуск отеля: kubectl rollout restart deployment otel-collector -n default
курлпод: kubectl run alpine --rm -i -t --image=alpine -- sh
apk add curl

изменение конфигурации core dns
kubectl -n kube-system get configmap coredns -o yaml
kubectl -n kube-system edit configmap coredns
kubectl -n kube-system rollout restart deployment coredns
вариант dns кубера: 10.43.0.10:53