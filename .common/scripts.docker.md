## Установка Docker Engine на Ubuntu
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
Далее будет доступна проверка:
```bash
sudo docker run hello-world
 ```

## Деплой
Пример скрипта доставки образа, не забудьте указать свой registry:
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
Также хэш контейнера можно получить из docker desktop или выполнив `docker ps --no-trunc`.

## IP Tables
Открытый наружу с хоста порт бд из докера можно закрыть, отредактировав цепочку правил DOCKER-USER:
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

docker run --env=MYSQL_USER=1 --env=MYSQL_PASSWORD=1 --env=MYSQL_DATABASE=tagit --env=MYSQL_ROOT_PASSWORD=1 --env=PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin --env=GOSU_VERSION=1.14 --env=MYSQL_MAJOR=8.0 --env=MYSQL_VERSION=8.0.31 --volume=src_mysql-volume:/var/lib/mysql:rw --volume=/var/lib/mysql -p 3306:3306 --name mysql_8 --runtime=runc -d mysql:8.0.31-debian