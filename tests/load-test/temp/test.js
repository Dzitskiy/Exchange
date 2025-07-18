import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 100 },   // Постепенно увеличиваем нагрузку до 100 пользователей
    { duration: '1m', target: 100 },    // Держим нагрузку
    { duration: '30s', target: 200 },   // Увеличиваем до 200
    { duration: '1m', target: 200 },    // Держим
    { duration: '30s', target: 300 },   // Увеличиваем до 300
    { duration: '1m', target: 300 },    // Держим
    { duration: '10s', target: 0 },     // Постепенно уменьшаем нагрузку
  ],
  thresholds: {
    http_req_failed: ['rate<0.01'],     // Макс. 1% ошибок
    http_req_duration: ['p(95)<500'],   // 95% запросов должны быть быстрее 500ms
  },
};

export default function () {
  const url = 'http://localhost:5000/api/Orders';
  const payload = JSON.stringify({
      op: 'create',
      instId: 'BTC-USDT',
      tdMode: 'cash',
      side: 'buy',
      ordType: 'limit',
      px: '2.15',
      sz: '2'
  });
  
  const headers = {
    'accept': '*/*',
    'Content-Type': 'application/json',
  };
  
  const response = http.post(url, payload, { headers });
  
  // Проверяем корректность ответа
  check(response, {
    'is status 200': (r) => r.status === 200,
    'has orderId': (r) => {
      try {
        const body = r.json();
        return typeof body.orderId === 'string' && body.orderId.length > 0;
      } catch (e) {
        return false;
      }
    },
  });
  
  sleep(0.1); // Короткая пауза между запросами
}