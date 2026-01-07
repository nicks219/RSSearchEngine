# setup paths & filenames environments:
```bash
local deployment_rsse="deployment.yml"
local service_rsse="service.yml"
local ingress_traefik="traefik.yml"
local resource_mysql="mysql.yml"
local configmap_mysql="my-custom.cnf"
local ssh_public="test.pub"
local ssh_public_path="/mnt/c/Users/nick2/.ssh"
local nginx_bundle="nginx_bundle_a67352573a96.crt"
local nginx_bundle_path="~/.crt"
local user_direcory="/usr/share/ca-certificates/comodo"
```

# ssh & ssl crt copy, rsync копирование манифестов deployment, service, traefik, mysql: 
копирует отдельные файлы
```bash
mkdir ~/.ssh
ssh-copy-id -i /mnt/c/Users/nick2/.ssh/test/test.pub root@82.146.45.180
rsync -azP -e "ssh -i .ssh/test/test -p 22" .crt/crt-private root@82.146.45.180:~/.crt/crt-private
rsync -azP -e "ssh -i .ssh/test/test -p 22" .crt/nginx_bundle/nginx_bundle_* root@82.146.45.180:~/.crt/nginx_bundle/nginx_bundle_*

rsync -azP -e "ssh -i ~/.ssh/test/test -p 22" ~/deployment.yml root@82.146.45.180:~/deployment.yml
rsync -azP -e "ssh -i ~/.ssh/test/test -p 22" ~/service.yml root@82.146.45.180:~/service.yml
rsync -azP -e "ssh -i ~/.ssh/test/test -p 22" ~/traefik.yml root@82.146.45.180:~/traefik.yml
rsync -azP -e "ssh -i ~/.ssh/test/test -p 22" ~/mysql.yml root@82.146.45.180:~/mysql.yml

rsync -azP -e "ssh -i .ssh/test/test -p 22" ~/my-custom.cnf root@82.146.45.180:~/my-custom.cnf
```

# run/restart cluster: 
при проблемах авторизации на kubectl команда переустановит бинарь, сохранив все данные, и восстановит работу
```bash
curl -sfL https://get.k3s.io | INSTALL_K3S_EXEC="--tls-san 82.146.45.180" sh -
```

# проверь:
```bash
echo [mysqld]\nbind-address = 0.0.0.0\nport = 3306 > my-custom.cnf

kubectl create namespace mysql-server
kubectl create configmap mysql-config --from-file=main-config=my-custom.cnf -n default
kubectl apply -f mysql.yml

kubectl apply -f deployment.yml
kubectl apply -f service.yml
```

# команды деплоя секретов/ингресса:
```bash
kubectl create secret tls secret-tls --key=/usr/share/ca-certificates/comodo/crt-private --cert=/usr/share/ca-certificates/comodo/nginx_bundle/nginx_bundle_a67352573a96.crt
```
```bash
kubectl create secret tls secret-tls --key=~/.crt/crt-private --cert=~/.crt/nginx_bundle/nginx_bundle_a67352573a96.crt
kubectl apply -f traefik.yml
```

# обновляем k3s

1) бэкап конфигов: kubectl get all -A -o yaml > backup.yaml
kubectl get all,secrets,configmaps,ingress,service,serviceaccount,roles,rolebindings,persistentvolumeclaims,persistentvolumes -A -o yaml > cluster-backup.1.32.yaml
2) [скорее всего нет необходимости] sudo systemctl stop k3s
второй вариант проверен, ставит последнюю stable-версию и флаг для доступа по IP (пока всё на одном сервере):
[ ] curl -sfL https://get.k3s.io | INSTALL_K3S_VERSION=v1.35.0+k3s1 sh -
[+] curl -sfL https://get.k3s.io | INSTALL_K3S_EXEC="--tls-san 82.146.45.180" sh -
[скорее всего нет необходимости] sudo systemctl start k3s
k3s --version | kubectl get pods --all-namespaces | systemctl status k3s

---
было (пример): k3s version v1.32.3+k3s1 (079ffa8d)
go version go1.23.6
---
default       mysql-646cf4b546-qjcrk                    1/1     Running     3 (181d ago)     2y6d
default       otel-collector-858569986b-xhh95           1/1     Running     0                169d
default       postgres-86f4955749-d8lbw                 1/1     Running     2 (181d ago)     253d
default       rsse-app-deployment-74fc8f878b-qfr9t      1/1     Running     0                3d12h
kube-system   coredns-5ccd4dbd47-f7wqt                  1/1     Running     75 (181d ago)    219d
kube-system   helm-install-traefik-crd-g9vkd            0/1     Completed   1                253d
kube-system   helm-install-traefik-dv5jv                0/1     Completed   3                253d
kube-system   local-path-provisioner-774c6665dc-l8qn4   1/1     Running     2 (181d ago)     253d
kube-system   svclb-traefik-32bcbd67-8zf7t              2/2     Running     4 (181d ago)     253d
kube-system   traefik-67bfb46dcb-gjcp4                  1/1     Running     171 (181d ago)   253d
---
стало: k3s version v1.34.3+k3s1 (48ffa7b6)
go version go1.24.11
---