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

# WELL KNOWN BUGS
* Используй node.js версии 16.20.2. Последняя версия уронила билд с ошибкой SSL:
`error:0308010C:digital envelope routines::unsupported`. (8-21-2023)

# Деплой
* Пример скрипта доставки (компоуз должен быть поднят), не забудьте указать свой registry:
```bash
cd /mnt/f/tagit/src && \
docker-compose down && \
docker image rm nick219nick/tagit:v1 && \
docker-compose -f docker-compose-build.yml build && \
docker-compose -f docker-compose-build.yml up -d && \
R_C_HASH=`docker ps | grep tagit | grep -E -o "^\S+"` && \
docker commit -a Nick219 -m v1 ${R_C_HASH} nick219nick/tagit:v1 && \
docker push nick219nick/tagit:v1;
```

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