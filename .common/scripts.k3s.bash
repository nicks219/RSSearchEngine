#!/bin/bash
## setup paths & filenames environments:

rsse_deployment="deployment.rsse.yml"
rsse_service="service.rsse.yml"
ingress_traefik="traefik.rsse.yml"
resource_mysql="resource.mysql.yml"
configmap_mysql="configmap.mysql.cnf"
ssh_public="test.pub"
ssh_public_path="/mnt/c/Users/nick2/.ssh"
nginx_bundle="nginx_bundle_a67352573a96.crt"
nginx_bundle_path="~/.crt"
user_directory="/usr/share/ca-certificates/comodo"

## rsync: ssh/ssl certificates & deployment - service - traefik - mysql manifests copy:

mkdir ~/.ssh
ssh-copy-id -i /mnt/c/Users/nick2/.ssh/test/test.pub root@82.146.45.180
rsync -azP -e "ssh -i .ssh/test/test -p 22" .crt/crt-private root@82.146.45.180:~/.crt/crt-private
rsync -azP -e "ssh -i .ssh/test/test -p 22" .crt/nginx_bundle/nginx_bundle_* root@82.146.45.180:~/.crt/nginx_bundle/nginx_bundle_*

rsync -azP -e "ssh -i ~/.ssh/test/test -p 22" ~/deployment.yml root@82.146.45.180:~/deployment.yml
rsync -azP -e "ssh -i ~/.ssh/test/test -p 22" ~/service.yml root@82.146.45.180:~/service.yml
rsync -azP -e "ssh -i ~/.ssh/test/test -p 22" ~/traefik.yml root@82.146.45.180:~/traefik.yml
rsync -azP -e "ssh -i ~/.ssh/test/test -p 22" ~/mysql.yml root@82.146.45.180:~/mysql.yml

## run k3s cluster:

curl -sfL https://get.k3s.io | INSTALL_K3S_EXEC="--tls-san 82.146.45.180" sh -
echo [mysqld]\nbind-address = 0.0.0.0\nport = 3306 > my-custom.cnf

kubectl create namespace mysql-server
kubectl create configmap mysql-config --from-file=main-config=my-custom.cnf -n default
kubectl apply -f mysql.yml

kubectl apply -f deployment.yml
kubectl apply -f service.yml

no: kubectl create secret tls secret-tls --key=/usr/share/ca-certificates/comodo/crt-private --cert=/usr/share/ca-certificates/comodo/nginx_bundle/nginx_bundle_a67352573a96.crt
kubectl create secret tls secret-tls --key=~/.crt/crt-private --cert=~/.crt/nginx_bundle/nginx_bundle_a67352573a96.crt
kubectl apply -f traefik.yml
