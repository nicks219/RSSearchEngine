Манифест для развертывания PostgreSQL в k3s, аналогичный моему MySQL-развертыванию:

1. Сначала создадим ConfigMap с базовой конфигурацией PostgreSQL:

```bash
kubectl create configmap postgres-config --from-file=main-config=postgresql.conf -n default
```

Содержимое файла `postgresql.conf`:
```conf
listen_addresses = '*'
port = 5432
max_connections = 100
```

2. Манифест для развертывания PostgreSQL (`postgres-resource.yaml`):

```yaml
---
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: postgres-pvc
  namespace: default
spec:
  accessModes:
    - ReadWriteOnce
  storageClassName: local-path
  resources:
    requests:
      storage: 1Gi

---
apiVersion: v1
kind: Service
metadata:
  name: postgres
  namespace: default
spec:
  selector:
    app: postgres
  type: ClusterIP
  ports:
    - name: postgres-port
      protocol: TCP
      port: 5432
      targetPort: 5432

---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: postgres
  namespace: default
spec:
  replicas: 1
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
        name: postgres
    spec:
      nodeSelector:
        kubernetes.io/hostname: nick2192.fvds.ru
      containers:
        - name: postgres
          image: postgres:15-alpine
          imagePullPolicy: Always
          ports:
            - name: postgres
              containerPort: 5432
          volumeMounts:
            - name: postgres-config-volume
              mountPath: /etc/postgresql/postgresql.conf
              subPath: postgresql.conf
            - name: postgres-storage
              mountPath: /var/lib/postgresql/data
          env:
            - name: POSTGRES_USER
              value: "1"
            - name: POSTGRES_PASSWORD
              value: "1"
            - name: POSTGRES_DB
              value: "tagit"
            - name: PGDATA
              value: "/var/lib/postgresql/data/pgdata"
      volumes:
        - name: postgres-config-volume
          configMap:
            name: postgres-config
            items:
              - key: main-config
                path: postgresql.conf
        - name: postgres-storage
          persistentVolumeClaim:
            claimName: postgres-pvc
```

3. Применяем манифест:
```bash
kubectl apply -f postgres-resource.yaml
```

Основные отличия от MySQL-развертывания:
1. Используется образ `postgres:15-alpine` (можно заменить на другой тег)
2. Порт изменен на 5432 (стандартный для PostgreSQL)
3. Переменные окружения используют префикс POSTGRES_ вместо MYSQL_
4. Добавлена переменная PGDATA для указания пути к данным
5. Конфигурационный файл монтируется в другое место

После развертывания PostgreSQL будет доступен по адресу `postgres.default.svc.cluster.local` (или просто `postgres` внутри кластера) на порту 5432.

Для подключения используем строку подключения:
```
Host=postgres;Port=5432;Database=tagit;Username=1;Password=1
```