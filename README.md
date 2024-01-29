# RS Search Engine (демо)
[![Deploy.K3S](https://github.com/nicks219/RSSearchEngine/actions/workflows/cd.deploy.k3s.yml/badge.svg)](https://github.com/nicks219/RSSearchEngine/actions/workflows/cd.deploy.k3s.yml)

#### Веб-сервис с каталогом для организации, случайного выбора и поиска небольших заметок, в т.ч. по тегам.
#### Фишка функционала: нечеткий текстовый поиск, выдерживающий существенные синтаксические ошибки.
#### Архитектура оптимизирована для работы контейнеров на бюджетном хостинге 1vCPU/1Gb RAM.

## Технологии
* ```text
  .NET 8 | MSTest | TS React | Router | Vite | Vitest
  ```    
* ```text
  MySQL | SQLite | EF
  ```
* ```text
  GitHub CI/CD | Docker | K3S ready | DV SSL ready
  ```

![cover.png](.common/cover.png)

## I. Информация..
* **API**: актуальная версия доступна онлайн на **[bumps.sh](https://bump.sh/nicks219/doc/rsse)**
* **Версии**: разработка фиксируется в **[CHANGELOG.md](CHANGELOG.md)**
* **K3S**: деплой на [**maintiner.me**](https://maintainer.me), манифесты в папке [**.k8s**](https://github.com/nicks219/RSSearchEngine/tree/master/.k3s)
* **CI/CD**: пайплайны [**GitHub Action**](https://github.com/nicks219/RSSearchEngine/actions)
* **DV SSL**: выпуск от [**Comodo**](https://www.comodo.com/), ингресс сконфигурирован

## II. Далее..
* **Поиск**: проприентарный алгоритм токенизации по запросу вернет массив с индексами релевантности:
  ```text
  /api/compliance/indices/{string} вернет json {res: [id, weight]}
  ```
* **Локальная разработка**: для win доступен подъём/остановка среды для **React** и файлы докера
* **Тесты**: для интеграционных тестов используется **SQLite**
