import http from 'k6/http';
import { sleep, check } from 'k6';
 function generateClOrdId() {
   return 'clOrdId-' + Math.random().toString(36).substr(2, 9);
 }

const url = 'http://localhost:5000/api/Orders';
const payload = {
   op: "create",
   instId: "BTC-USDT",
   tdMode: "cash",
   side: "buy",
   ordType: "limit",
   px: "2.15",
   sz: "2"
};

const headers = {
  'Content-Type': 'application/json',
  // 'Authorization': 'Bearer ваш_токен'
};

export let options = {
  stages: [
    { duration: '30s', target: 8000 },
    { duration: '60s', target: 8000 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    'http_reqs': ['rate>=5000'], 
    'http_req_duration': ['p(95)<=1000']
  }
};

export default function () {
  const data = { ...payload
   , clOrdId: generateClOrdId() 
  };
  const res = http.post(url, JSON.stringify(data), { headers });
  
  check(res, {
    'status was 2xx': (r) => r.status >= 200 && r.status < 300,
  });
  
  sleep(0);
}