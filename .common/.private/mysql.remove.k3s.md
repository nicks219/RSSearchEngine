Удалить развертывание MySQL из k3s-кластера:

1. Удалите Deployment:
```bash
kubectl delete deployment mysql -n default
```

2. Удалите Service:
```bash
kubectl delete service mysql -n default
```

3. Удалите PersistentVolumeClaim (PVC):
```bash
kubectl delete pvc mysql-pvc -n default
```

4. Удалите ConfigMap:
```bash
kubectl delete configmap mysql-config -n default
```

Если вы хотите удалить все одним махом, можно использовать:
```bash
kubectl delete deployment,service,pvc,configmap -l app=mysql -n default
```

Или если использован один файл для создания всех ресурсов (как у меня), можно удалить всё через тот же файл:
```bash
kubectl delete -f ваш_файл_ресурсов_mysql.yaml
```

Примечания:
1. Удаление PVC не удалит автоматически данные на PersistentVolume (PV). Если нужно удалить и данные, найти соответствующий PV и удалить его:
```bash
kubectl get pv
kubectl delete pv имя_вашего_pv
```

2. Если namespace создан специально для MySQL, его тоже можно удалить:
```bash
kubectl delete namespace mysql-server
```