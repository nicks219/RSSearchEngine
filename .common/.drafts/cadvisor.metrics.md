#### Идеи вариантов сбора системных метрик кластера:

* Добавить в deployment otel `на этапе разработки`
```yaml
spec:
  serviceAccountName: metrics-reader  # привязывать сервис-аккаунт
  containers:
  - name: otel-collector
    ...
    volumeMounts:
    - name: token
      mountPath: /var/run/secrets/kubernetes.io/serviceaccount
      readOnly: true
  volumes:
  - name: token
    projected:
      sources:
      - serviceAccountToken:
          path: token
          expirationSeconds: 3600  # токен обновляется каждые 1 час
          audience: api
```

* Добавить сборщик для prometheus `ок, см. otel.collector.otlp.v3.yml`
```yaml
receivers:
  ...
  prometheus:
    config:
      scrape_configs:
        - job_name: 'kubelet-cadvisor'
          scheme: https
          metrics_path: /metrics/cadvisor
          tls_config:
            insecure_skip_verify: true
          bearer_token_file: /var/run/secrets/kubernetes.io/serviceaccount/token  # или путь к токену
          static_configs:
            - targets: ['localhost:10250']
```

* Добавить prometheus в пайплайн метрик `ok`
```yaml
	...
	metrics:
	  receivers: [otlp, hostmetrics, prometheus]
	  processors: ...
```

* Вариант - добавить процессор для лейбла infra
```yaml
    processors:
      ...
      transform/add_infra_label:
        error_mode: ignore
        metric_statements:
          - context: resource
            statements:
              - set(attributes["deployment.environment"], "infra")
```

* И создать дополнительный пайплайн для метрик cadvisor
```yaml
metrics/infrastructure:
      receivers: [prometheus]
      processors: [transform/add_infra_label, ..., batch]
      exporters: [otlphttp/grafana_cloud]
```

* Замена Deployment на DaemonSet `нет необходимости`
```
apiVersion: apps/v1
kind: DaemonSet
metadata:
  name: otel-collector
  namespace: default
  labels:
    app: otel-collector
spec:
  selector:
    matchLabels:
      app: otel-collector
  template:
    metadata:
      labels:
        app: otel-collector
    spec:
      hostNetwork: true  # 🔥 доступ к сетке хоста
      containers:
        - name: otel-collector
          env:
            - name: GRAFANA_CLOUD_OTLP_ENDPOINT
              value: 'https://otlp-gateway-prod-us-east-0.grafana.net/otlp'
            - name: GRAFANA_CLOUD_API_KEY
              valueFrom:
                secretKeyRef:
                  name: grafana-cloud-api-key
                  key: key
            - name: GRAFANA_CLOUD_INSTANCE_ID
              valueFrom:
                secretKeyRef:
                  name: grafana-cloud-instance-id
                  key: id
          image: otel/opentelemetry-collector-contrib:latest
          command: ['/otelcol-contrib']
          args: ['--config=/conf/otel-collector-config.yaml']
          volumeMounts:
            - name: config-volume
              mountPath: /conf
          ports:
            - name: otlp
              containerPort: 4317
            - name: otlp-http
              containerPort: 4318
      volumes:
        - name: config-volume
          configMap:
            name: otel-collector-conf
      serviceAccountName: otel-collector
```