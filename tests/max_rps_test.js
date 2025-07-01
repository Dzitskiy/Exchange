// Запуск теста
// Базовый запуск: k6 run max_rps_test.js
// С кастомными параметрами: k6 run -e BASE_URL=http://your-api:8080 --no-thresholds max_rps_test.js

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';

// Кастомные метрики
const requestDuration = new Trend('request_duration');
const successRate = new Rate('success_rate');
const timeoutErrors = new Counter('timeout_errors');

// Конфигурация теста
export const options = {
  scenarios: {
    find_max_rps: {
      executor: 'ramping-arrival-rate',
      startRate: 50,      // Начальный RPS
      timeUnit: '1s',     // Запросов в секунду
      preAllocatedVUs: 10, // Начальное количество VU
      maxVUs: 1000,       // Максимальное количество VU
      stages: [
        { target: 100, duration: '30s' },  // +100 RPS каждые 30s
        { target: 200, duration: '30s' },
        { target: 300, duration: '30s' },
        { target: 400, duration: '30s' },
        { target: 500, duration: '30s' },
        { target: 600, duration: '30s' },
        { target: 700, duration: '30s' },
        { target: 800, duration: '30s' },
        { target: 900, duration: '30s' },
        { target: 1000, duration: '30s' },
      ],
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<500'],  // Порог для задержки
    success_rate: ['rate>0.95'],       // Минимальный успех
    'http_reqs{status:200}': ['rate>0'], // Только для мониторинга
  },
  discardResponseBodies: true, // Для экономии памяти
};

// Глобальная переменная для базового URL
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5278/';

export default function () {
  const payload = JSON.stringify({
    Description: `random-${Math.floor(Math.random() * 100)}`,
  });

  const params = {
    headers: { 'Content-Type': 'application/json' },
    timeout: '30s',  // Таймаут ожидания ответа
  };

  const start = Date.now();
  const response = http.post(`${BASE_URL}/api/orders`, payload, params);
  const end = Date.now();
  
  // Фиксация метрик
  requestDuration.add(end - start);
  
  // Проверка успешности
  const isSuccess = check(response, {
    'status is 200': (r) => r.status === 200,
    'response has orderId': (r) => {
      try {
        const body = JSON.parse(r.body);
        return body.orderId && body.orderId.length > 0;
      } catch {
        return false;
      }
    },
  });

  successRate.add(isSuccess);
  
  if (response.status === 0 || response.status === 504) {
    timeoutErrors.add(1);
  }
  
  // Без задержки между запросами для максимальной нагрузки
}