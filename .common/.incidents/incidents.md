#### Инциденты


* **[07-06-25](07-06-25/07-06-25.md) деградация кластера** `решено`



#### Команды

* сохранить все ямлы:
```
kubectl get all -A -o yaml > cluster.yaml
kubectl get pvc,pv,secrets,configmaps -A -o yaml >> cluster.yaml
```

* создать бэкапы kine и k3s-server:
```
tar czf ~/k3s-full-backup.tar.gz /var/lib/rancher/k3s/server
cp /var/lib/rancher/k3s/server/db/state.db ~/kine-backup.db
```

* восстановить из бэкапов (не проверял):
```
systemctl stop k3s
mv /var/lib/rancher/k3s/server /var/lib/rancher/k3s/server.bak
tar xzf ~/k3s-full-backup.tar.gz -C /
systemctl start k3s
```

* получить логи:
```
journalctl -u k3s --since "30 min ago" > k3s-recent.log
```

* обновить серты:
```
k3s certificate rotate
systemctl restart k3s
```

* добавить своп:
```
fallocate -l 1G /swapfile
chmod 600 /swapfile
mkswap /swapfile
swapon /swapfile
echo '/swapfile none swap sw 0 0' >> /etc/fstab
```

* почистить бд kine:
```
sudo systemctl stop k3s
sqlite3 /var/lib/rancher/k3s/server/db/state.db "VACUUM;"
sudo systemctl start k3s
```

* ...