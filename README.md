# RS Search Engine: "Tag IT"
#### Веб-сервис для организация и поиска небольших заметок в т.ч. по тегам
#### Поисковый запрос выдерживает существенные синтаксические ошибки
#### Для вычисления поискового индекса используется проприентарный алгоритм токенизации

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
* Изначально сервис задумывался как каталог песен, что повлияло на именования в коде и таблицах бд

# Технологии
* ```bash
  .NET 7 | TypeScript React | MSTest | Docker
  ```    
* ```bash
  MySQL
  ```
# Запуск
* Проект фронта редуцирован до файлов, которые находятся в физической папке Rsse.Front, 
все *.tsx и скрипты продублированы в виртуальных папках src/front
* Соберите фронт:  
  **npm install && npm run build** в папке ```Rsse.Front/ClientApp```, или запустите скрипты **install** и **build**
* Для старта при локальной разработке:  
  команду **npm start** из папки ```Rsse.Front/ClientApp/build```, либо запустите скрипт **start**
* В папке **src** расположены два ```docker-compose``` файла и ```Dockerfile```
* Дополнительная информация есть в папке ```docs```

# Дополнительная информация
* Используй node.js версии 16.20.2. Последняя версия уронила билд с ошибкой SSL:
`error:0308010C:digital envelope routines::unsupported`. (8-21-2023)
* Открытый наружу с хоста порт бд из докера можно закрыть, отредактировав цепочку правил DOCKER-USER:
```bash
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
* При локальном запуске не забывайте, что `localohost` != `127.0.0.1`, иначе не залогинишься.
* **Накатка базы из дампа**: если при запуске в `Rsse.Base/ClientApp/build` будет лежать файл `backup_9.txt` с дампом, то можно запустить команду **Catalog>Restore**.
  Скопируйте этот файл в `Rsse.Base/ClientApp/build` перед сборкой docker-образа. В VCS находится файл с 915 песнями.

# Деплой
* Пример скрипта доставки образа - не забудьте указать свой registry:
```bash
cd /mnt/f/tagit/src && \
docker-compose down && \
docker image rm nick219nick/tagit:v5 && \
docker-compose -f docker-compose-build.yml build && \
docker-compose -f docker-compose-build.yml up -d && \
R_C_HASH=`docker ps | grep tagit | grep -E -o "^\S+"` && \
docker commit -a Nick219 -m v5 ${R_C_HASH} nick219nick/tagit:v5 && \
docker push nick219nick/tagit:v5;
```
* Также хэш контейнера можно получить из docker desktop или выполнив `docker ps --no-trunc`.

# Описание API
В development доступна ручка /swagger/v1/swagger.json, также в Rsse.Base/Controller закоммитан api.v1.json

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

# История изменений:
#### v5:
* fix: закрыт доступ к бд снаружи докера
* fix: исправлены несколько багов на фронте и в логике работы с дампами
* upd: обновлены версии зависимостей
* upd: изменена структура проекта - добавлены скрипты для npm, проект фронта убран из решения и разобран по виртуальным папкам
