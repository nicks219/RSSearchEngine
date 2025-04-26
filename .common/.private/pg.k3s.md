### манифест для развертывания postgres в k3s, аналогичный моему развертыванию mysql:

1. cоздадим ConfigMap с базовой конфигурацией postgres:

```bash
kubectl create configmap postgres-config --from-file=main-config=postgresql.conf -n default
```

содержимое файла `postgresql.conf`:
```conf
listen_addresses = '*'
port = 5432
max_connections = 100
```

2. манифест для развертывания postgres (`postgres-resource.yaml`):

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
          image: postgres:17.4-alpine3.21
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
            - name: PGDATA # задаётся в докерфайле образа: https://github.com/docker-library/postgres/blob/cc254e85ed86e1f8c9052f9cbf0e3320324f0421/17/alpine3.21/Dockerfile#L197
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

3. применим манифест:
```bash
kubectl apply -f postgres-resource.yaml
```

основные отличия от mysql-развертывания, разберись в актуальности:
1. добавлена переменная PGDATA для указания пути к данным
2конфигурационный файл монтируется в другое место

соответственно, после развертывания postgres будет доступен по адресу `postgres.default.svc.cluster.local` - 
или просто `postgres` внутри кластера - на порту 5432.

для подключения используем строку подключения:
```
Host=postgres;Port=5432;Database=tagit;Username=1;Password=1
```
