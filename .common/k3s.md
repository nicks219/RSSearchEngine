# setup paths & filenames environments:
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

# rsync: ssh & ssl crt copy, deployment - service - traefik - mysql / yml: (точно копирует отдельные файлы)
mkdir ~/.ssh
ssh-copy-id -i /mnt/c/Users/nick2/.ssh/test/test.pub root@82.146.45.180
rsync -azP -e "ssh -i .ssh/test/test -p 22" .crt/crt-private root@82.146.45.180:~/.crt/crt-private
rsync -azP -e "ssh -i .ssh/test/test -p 22" .crt/nginx_bundle/nginx_bundle_* root@82.146.45.180:~/.crt/nginx_bundle/nginx_bundle_*

rsync -azP -e "ssh -i ~/.ssh/test/test -p 22" ~/deployment.yml root@82.146.45.180:~/deployment.yml
rsync -azP -e "ssh -i ~/.ssh/test/test -p 22" ~/service.yml root@82.146.45.180:~/service.yml
rsync -azP -e "ssh -i ~/.ssh/test/test -p 22" ~/traefik.yml root@82.146.45.180:~/traefik.yml
rsync -azP -e "ssh -i ~/.ssh/test/test -p 22" ~/mysql.yml root@82.146.45.180:~/mysql.yml

rsync -azP -e "ssh -i .ssh/test/test -p 22" ~/my-custom.cnf root@82.146.45.180:~/my-custom.cnf

# run cluster: при проблемах авторизации на kubectl эта команда переустановит бинарь сохранив все данные и восстановит работу
curl -sfL https://get.k3s.io | INSTALL_K3S_EXEC="--tls-san 82.146.45.180" sh -
# проверь:
echo [mysqld]\nbind-address = 0.0.0.0\nport = 3306 > my-custom.cnf

kubectl create namespace mysql-server
kubectl create configmap mysql-config --from-file=main-config=my-custom.cnf -n default
kubectl apply -f mysql.yml

kubectl apply -f deployment.yml
kubectl apply -f service.yml

# kubectl create secret tls secret-tls --key=/usr/share/ca-certificates/comodo/crt-private --cert=/usr/share/ca-certificates/comodo/nginx_bundle/nginx_bundle_a67352573a96.crt
kubectl create secret tls secret-tls --key=~/.crt/crt-private --cert=~/.crt/nginx_bundle/nginx_bundle_a67352573a96.crt
kubectl apply -f traefik.yml
