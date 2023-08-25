# TAG IT 
#### Организация и поиск небольших заметок (база знаний)
#### Разрабатывается для личной эксплуатации
#### Чтобы не утонуть в море мудрости и знаний =)

* Нечеткий тестковый поиск (запрос с ошибками): 
  ```bash
  /api/find/{string} вернет json {res: [id, weight]}
  ```
* Организация заметок "всё под рукой" по категориям
* Распознаёт ссылки 
* Для тэгов применяйте ```@``` (больший вес слова при поиске)
* По выбранным категориям идёт с round robin
* Работа с локальными поисковыми индексами учитывает необходимость 
синхронизации индексов на подах
* Для добавления тега - используйте в названии прямоугольные скобки

# Технологии
* ```bash
  .NET 6 | TypeScript React | MSTest | Docker
  ```    
* ```bash
  [MSSQL] | MySQL
  ```
# Запуск
* Сбилдите фронт: ```npm install && npm run build```  
* Для локального разработки запустите:  
  ```npm start``` из папки ```Rsse.Front/ClientApp/build```  
* В каталоге есть ```docker-compose.yml``` и ```Dockerfile```
* В папке Rsse.Data/Dump есть MySql-дамп на 389 песен
* Некоторая информация есть в папке ```docs```

# WELL KNOWN BUGS AND TRICKS
* Используй node.js версии 16.20.2. Последняя версия уронила билд с ошибкой SSL:
`error:0308010C:digital envelope routines::unsupported`. (8-21-2023)
* Закрыть на ubuntu бд из докера от доступа "снаружи" сервиса, дело в цепочке правил DOCKER-USER:
```json
iptables -I DOCKER-USER -s 172.19.0.3 -d 172.19.0.2 -p tcp --dport 3306 -j ACCEPT //выглядит лишним, но бог с ним
iptables -I DOCKER-USER -p tcp --dport 3306 -j DROP
iptables -I DOCKER-USER -s 172.19.0.3 -d 0.0.0.0/0 -p tcp --dport 3306 -j RETURN

ufw тож включен: 3306 и 3306/tcp заблокированы. результат выглядит так: netstat -ntlp или iptables -S

Chain DOCKER-USER (1 references)
num   pkts bytes target     prot opt in     out     source               destination
1       31  3136 RETURN     tcp  --  *      *       172.19.0.3           0.0.0.0/0            tcp dpt:3306
2       25  1460 DROP       tcp  --  *      *       0.0.0.0/0            0.0.0.0/0            tcp dpt:3306
3        3   183 ACCEPT     tcp  --  *      *       172.19.0.3           172.19.0.2           tcp dpt:3306
4      107 27639 RETURN     all  --  *      *       0.0.0.0/0            0.0.0.0/0
```
* При первом (в т.ч. локальном) запуске будет создана бд `tagit` с таблицей авторизации, при ошибке проверь корректность юзера и пароля в `appsettings`.
* При локальном запуске не забывай, что `localohost` != `127.0.0.1`, иначе не залогинишься.
* Если при запуске в `Rsse.Base/ClientApp/build` будет лежать файл `backup_9.txt` с дампом, то можно запустить команду **Catalog>Restore**.
  Для его появления в этой папке после деплоя скопируй файл в `Rsse.Base/ClientApp/build` перед сборкой docker-образа. В VCS, скорее всего, находится 
  файл с 883 песнями.

# Деплой
* Пример скрипта доставки (компоуз должен быть поднят), не забудьте указать свой registry:
```bash
cd /mnt/f/tagit/src && \
docker-compose down && \
docker image rm nick219nick/tagit:v4 && \
docker-compose -f docker-compose-build.yml build && \
docker-compose -f docker-compose-build.yml up -d && \
R_C_HASH=`docker ps | grep tagit | grep -E -o "^\S+"` && \
docker commit -a Nick219 -m v4 ${R_C_HASH} nick219nick/tagit:v4 && \
docker push nick219nick/tagit:v4;
```
* Также хэш контейнера можно получить из docker desktop или выполнив `docker ps --no-trunc`.

# Описание API

В development зайдите на /swagger/v1/swagger.json, также в Rsse.Base/Controller закоммитан api.v1.json

# Установка Docker Engine на Ubuntu

https://docs.docker.com/engine/install/ubuntu/  

```bash
sudo apt-get remove docker docker-engine docker.io containerd runc

sudo apt-get update

sudo apt-get install \
    ca-certificates \
    curl \
    gnupg \
    lsb-release

sudo mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

echo \
 "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
 $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt-get update
sudo apt-get install docker-ce docker-ce-cli containerd.io docker-compose-plugin
```
А теперь =)
```bash
sudo docker run hello-world
```