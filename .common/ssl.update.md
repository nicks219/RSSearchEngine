# настройка доступа по ssl-сертификату для нового домена notefinder.ru:

# требовалось решить задачи
-       I. восстановить доступ по ssh-сертификату при ssh-доступе только по логину и паролю    [+]
-       II. приобрести и настроить доменное имя и сертификат GlobalSign для данного домена     [+]
-       III. добавить и запустить ингрессы в сервис с новым сертификатом и доступом по IP      [+]
-       IV. обеспечить только https-доступ к сервису, редирект через traefik не получился      [-]

# сопутствующие проблемы и их решения
-    	I. приватный и публичный ssh-ключи были найдены локально, также публичный ssh-rsa присутствовал на сервисе в файле ~/.ssh/authorized_keys
		при этом не получалось использовать локально приватный ssh-ключ из-за некорректных прав доступа и невозможеости их замены в общей с win папке из-под wsl: 
		"It is required that your private key files are NOT accessible by others".
		Решение заключалось в переносе приватного ключа "внутрь" wsl и использовании его оттуда.
- 	    II. покупка и активация домена прошли гладко, новые DNS записи вносить не потребовалось.
		в сервисе я использовал nginx-бандл сертификатов для конфигурации traefik, в данном случае пришлось собирать его "вручную"
		изначально я собрал бандл из файлов: notefinder (мой) - gcc r6 alphassl CA (промежуточный, совпадение найдено на сайте поставщика) - GlobalSign (рутовый, совпадение найдено на сайте поставщика)
		последний GlobalSign именуется R6 GlobalSign Root Certificate на сайте поставщика и он онказался лишним, вызвав предупреждение: Chain issues: "contains anchor", после чего был удалён из бандля.
		сертификаты добавлялись в секреты k3s и использовались в примитиве ingress кубера (для traefik), также был опробован апдейт секрета без его удаления/создания. 

		остались нерешенные недочеты, их можно просмотреть на ресурсе проверки доступа по SSL: https://support.globalsign.com/?showcerts=yes
		- DNS CAA: https://blog.qualys.com/product-tech/2017/03/13/caa-mandated-by-cabrowser-forum?_ga=2.246236359.2144599009.1734453021-94359761.1734453021
		- ошибки traefik: не поддерживает Server Name Indication - No SNI
		  Alternative names	9178bd1ab2806a17803f401c79125d12.a06414eb63b580b2b6e11bd8acbd46ed.traefik.default   MISMATCH

-       III.	удаление/создание новых ингрессов прошла гладко, предыдущие ингрессы сохраняли свою работоспособность.
-       IV.	редирект http -> https не сработал, пробовал:
              - новую аннотацию при создании ingress: traefik.ingress.kubernetes.io/redirect-entry-point: "https"  # Редирект с HTTP на HTTPS
              - возможность изменить конфигурацию самого traefik не были задействованы из-за недостатка опыта.
		
# конкретные команды и примеры кода для решения вышеупомянутых задач
- I
```bash
ssh_port="22"
host_ip="82.146.45.180"
host="root@$host_ip"
ssh_private="/mnt/c/Users/nick/.ssh/test"
ssh_public="/mnt/c/Users/nick/.ssh/test.pub"
ingress_traefik="ingress.traefik.ip.yml"

# приватный ключ надо копировать из общей папки внутрь wsl
cp $ssh_private ~/test
chmod 600 ~/test
ssh_private="~/test"

# проверка работоспособности
ssh -i $ssh_private $host
rsync -azP -e "ssh -i $ssh_private -p $ssh_port" "$ingress_traefik" $host:~/"$ingress_traefik"
# применение конфигурации изнутри хостинга
kubectl apply -f ingress.traefik.ip.yml
```

- II
все файлы использовались из home папки, варианты записи: --key=~/"$ssl_private". notefinder.crt это SSL-цепочка, notefinder.key это приватный SSL-ключ.
```bash
# создание нового секрета
kubectl create secret tls secret-tls-ru --key=notefinder.key --cert=notefinder.crt
```

```bash
# замена сертификата, вызвала предупреждение от kubectl об автоматическом добавленнии недостающих аннотаций
kubectl create secret tls secret-tls-ru \
  --cert=notefinder.noroot.crt \
  --key=notefinder.key \
  --dry-run=client -o yaml | kubectl apply -f -
```

```bash
# откат мог бы выглядеть так
kubectl create secret tls secret-tls-ru --key=notefinder.key --cert=notefinder.crt --dry-run=client -o yaml | kubectl apply -f -
```

- III
```bash
# создание и удаление рксурса по описанию
kubectl apply -f ingress.traefik.ru.yml
kubectl delete -f ingress.traefik.ru.yml
```

описание новых ингрессов: 
```yaml
# файл ingress.traefik.ip.yml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: rsse-app-ingress-http
spec:
  rules:
    - http:
        paths:
          - backend:
              service:
                name: rsse-app-service
                port:
                  number: 5000
            path: /
            pathType: Prefix
---
```

```yaml
# файл ingress.traefik.ru.yml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: rsse-app-ingress-https-ru
  annotations:
    ingress.kubernetes.io/ssl-redirect: "true"
spec:
  rules:
    - host: notefinder.ru
      http:
        paths:
          - backend:
              service:
                name: rsse-app-service
                port:
                  number: 5000
            path: /
            pathType: Prefix
  tls:
    - hosts:
        - notefinder.ru
      secretName: secret-tls-ru
---
```

- IV research..
```bash
# попытка получить настройки traefik, в процессе исследования
kubectl get pods -n kube-system
kubectl describe pod traefik-...-... -n kube-system
```

GPT советует добавить настройку самого traefik, например в values.yaml если он установлен через helm:
```yaml
# values.yaml
ingressRoute:
  http:
    redirectTo: https # Это обеспечит автоматический редирект с HTTP на HTTPS для всех Ingress'ов
```

# итоги
- время выполнения задачи - один рабочий день: 11-18-2024
- основные задачи полностью выполнены, https-доступ с notefinder.ru обеспечен, получен новый опыт при решении возникших проблем
- стоит продолжить ресёрч настроек ингрессов и поиск/настройку конфигурации самого traefik для редиректа на данном уровне
- на будущее стоит разобраться в работе helm

---------------------------------------------------------------------------------------------------------------------------
