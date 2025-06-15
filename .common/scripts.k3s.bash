#!/bin/bash
# черновик, чтоб сохранить идею. как скрипт не проверял.
# перенеси файлы в одну папку или напиши helm-чарт.

## setup paths & filenames environments:

rsse_deployment="deployment.rsse.yml"
rsse_service="service.rsse.yml"
ingress_traefik="ingress.traefik.yml"
resource_mysql="resource.mysql.yml"
configmap_mysql="configmap.mysql.cnf"

ssh_private=".ssh/test/test"
ssh_public="/mnt/c/Users/nick2/.ssh/test/test.pub"
ssh_port="22"
host_ip="82.146.45.180"
host="root@$host_ip"

ssl_private=".crt/crt-private"
ssl_cert=".crt/nginx_bundle/nginx_bundle_a67352573a96.crt"
ssl_cert_for_rsync=".crt/nginx_bundle/nginx_bundle_*"
# remote_crt_directory="/usr/share/ca-certificates/comodo"

## rsync: ssh/ssl certificates & deployment - service - traefik - mysql manifests copy:

mkdir ~/.ssh
ssh-copy-id -i $ssh_public $host
rsync -azP -e "ssh -i $ssh_private -p $ssh_port" ~/"$ssl_private" $host:~/"$ssl_private"
rsync -azP -e "ssh -i $ssh_private -p $ssh_port" ~/"$ssl_cert_for_rsync" $host:~/"$ssl_cert_for_rsync"

rsync -azP -e "ssh -i ~/$ssh_private -p $ssh_port" ~/"$rsse_deployment" $host:~/"$rsse_deployment"
rsync -azP -e "ssh -i ~/$ssh_private -p $ssh_port" ~/"$rsse_service" $host:~/"$rsse_service"
rsync -azP -e "ssh -i ~/$ssh_private -p $ssh_port" ~/"$ingress_traefik" $host:~/"$ingress_traefik"
rsync -azP -e "ssh -i ~/$ssh_private -p $ssh_port" ~/"$resource_mysql" $host:~/"$resource_mysql"
rsync -azP -e "ssh -i ~/$ssh_private -p $ssh_port" ~/"$configmap_mysql" $host:~/"$configmap_mysql"

## run k3s cluster:

curl -sfL https://get.k3s.io | INSTALL_K3S_EXEC="--tls-san $host_ip" sh -

kubectl create namespace mysql-server
kubectl create configmap mysql-config --from-file=main-config=$configmap_mysql -n default
kubectl apply -f $resource_mysql

kubectl apply -f $rsse_deployment
kubectl apply -f $rsse_service

# kubectl create secret tls secret-tls --key=/usr/share/ca-certificates/comodo/crt-private --cert=/usr/share/ca-certificates/comodo/nginx_bundle/nginx_bundle_a67352573a96.crt
kubectl create secret tls secret-tls --key=~/"$ssl_private" --cert=~/"$ssl_cert"
kubectl apply -f $ingress_traefik


# traefik по ip
kubectl apply -f ingress.traefik.ip.yml
# traefik для notefinder
kubectl apply -f ingress.traefik.ru.yml
# postgres
kubectl apply -f resource.postgres.yml
# pvc
kubectl apply -f pvc.k3s.yml
# сервис rsse
kubectl apply -f deployment.rsse.pvc.v2.yml

#################
# Grafana Cloud #
#################

# secret для otel.collector.metrics: auth.txt содержит строку `Basic base64(username:password)` из grafana cloud
kubectl create secret generic grafana-auth --from-file=authorization=./auth.txt
# otel со скрейпингом метрик в формате prometheus (выключен)
kubectl apply -f otel.collector.metrics.yml

# secret для otel.collector.otlp.v2: id и api-key создаются grafana cloud
kubectl create secret generic grafana-cloud-api-key --from-file=key=./grafana-cloud-api-key.txt
kubectl create secret generic grafana-cloud-instance-id --from-file=id=./grafana-cloud-instance-id.txt
# otel с передачей диагностиков по otlp (активен)
kubectl apply -f otel.collector.otlp.v2.yml

####################
# полезные команды #
####################

# exec to pod
kubectl get pods
kubectl exec -it rsse-app-deployment-c9ff5fbd4-fk88h -- /bin/sh

# copy to host
kubectl cp rsse-app-deployment-c9ff5fbd4-fk88h:/App/ClientApp/build/_db_last_dump_.txt /root/_db_last_dump_.txt

# install nano in alpine
apk update && apk add nano
вариант: sed -i 's/старый_текст/новый_текст/g' file_name.txt

# temporary pod for testing purpose
kubectl run curlpod --rm -i -t --image=curlimages/curl --namespace=default --restart=Never -- sh
