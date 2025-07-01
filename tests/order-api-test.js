// Запуск теста
//   # Быстрая проверка: k6 run--vus 10 --duration 30s order - api - test.js
// Полный тест по сценарию: k6 run order-api-test.js

import http from 'k6/http';
import { check, sleep } from 'k6';
import { SharedArray } from 'k6/data';
import { Trend, Rate, Counter } from 'k6/metrics';

// Кастомные метрики
const createOrderDuration = new Trend('create_order_duration');
const successRate = new Rate('success_rate');
const timeoutErrors = new Counter('timeout_errors');

// Конфигурация теста
export const options = {
    scenarios: {
        stress_test: {
            executor: 'ramping-vus',
            stages: [
                { duration: '30s', target: 50 },   // Ramp-up до 50 VU
                { duration: '1m', target: 50 },    // Стабильная нагрузка
                { duration: '30s', target: 100 },  // Увеличение до 100 VU
                { duration: '1m', target: 100 },
                { duration: '30s', target: 200 },  // Пиковая нагрузка
                { duration: '1m', target: 200 },
                { duration: '30s', target: 0 },    // Ramp-down
            ],
            gracefulRampDown: '30s',
        },
    },
    thresholds: {
        http_req_duration: ['p(95)<500'],  // 95% запросов < 500ms
        success_rate: ['rate>0.95'],       // >95% успешных запросов
        timeout_errors: ['count==0'],      // Ошибок таймаута быть не должно
    },
};

// Генерация тестовых данных
const products = new SharedArray('orders', function () {
    return JSON.parse(open('./orders.json'));
});

export default function () {
    const product = products[Math.floor(Math.random() * products.length)];
    const payload = JSON.stringify({
        ProductId: product.id,
        Quantity: Math.floor(Math.random() * 5) + 1
    });

    const params = {
        headers: {
            'Content-Type': 'application/json',
        },
        timeout: '30s',  // Таймаут ожидания ответа
    };

    const start = Date.now();
    const response = http.post('http://localhost:5278/api/Orders', payload, params);
    const end = Date.now();

    // Проверка результатов
    const checkResult = check(response, {
        'Status is 200': (r) => r.status === 200,
        'Has OrderId': (r) => {
            try {
                const body = JSON.parse(r.body);
                return body.orderId && body.orderId.length > 0;
            } catch {
                return false;
            }
        },
    });

    // Фиксация метрик
    createOrderDuration.add(end - start);
    successRate.add(checkResult);

    if (response.status === 504) {
        timeoutErrors.add(1);
    }

    // Имитация пользовательской задержки
    sleep(0.1);
}