# Инструкция по запуску

## Сборка и запуск системы:

``` bash
docker-compose up -d --build
``` 

## Проверка работы сервисов:

``` bash
docker-compose ps
```

##  Отправка тестового запроса:

``` bash
curl --location 'http://localhost:5000/api/orders' \
--header 'Content-Type: application/json' \
--data '{
    "op": "create",
    "instId": "BTC-USDT",
    "clOrdId": "",
    "tdMode": "cash",
    "side": "buy",
    "ordType": "limit",
    "px": "2.15",
    "sz": "2"
}'```

## Просмотр логов:

``` bash
docker-compose logs -f orderapi
```

##  Мониторинг системы:

- Seq: http://localhost:8081
- Kafdrop: http://localhost:9000
- Cassandra Web UI: http://localhost:3000

## Нагрузочное тестирование:

``` bash
docker run -i --network=ordersystem_default loadimpact/k6 run - <load-test/order-load-test.js
```