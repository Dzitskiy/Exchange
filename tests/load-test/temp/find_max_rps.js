import exec from 'k6/execution';
import { sleep } from 'k6';

const config = {
  minRPS: 50,
  maxRPS: 4500,
  step: 100,
  duration: '30s',
  threshold: 0.95
};

export default function () {
  let currentRPS = config.minRPS;
  let maxStableRPS = 0;
  
  while (currentRPS <= config.maxRPS) {
    console.log(`Testing ${currentRPS} RPS...`);
    
    const scenario = {
      executor: 'constant-arrival-rate',
      rate: currentRPS,
      timeUnit: '1s',
      duration: config.duration,
      preAllocatedVUs: currentRPS,
      maxVUs: currentRPS * 2,
    };
    
    const metrics = exec.scenario(scenario, () => {

        const res = http.post(url, JSON.stringify(data), { headers });

        check(res, {
            'status was 2xx': (r) => r.status >= 200 && r.status < 300,
        });
a

        sleep(1); // Заглушка
    });
    
    const successRate = metrics.metrics.success_rate.values.rate;
    
    if (successRate >= config.threshold) {
      maxStableRPS = currentRPS;
      console.log(`? ${currentRPS} RPS stable (success: ${(successRate * 100).toFixed(1)}%)`);
      currentRPS += config.step;
    } else {
      console.log(`? ${currentRPS} RPS failed (success: ${(successRate * 100).toFixed(1)}%)`);
      break;
    }
  }
  
  console.log(`\nMaximum stable RPS: ${maxStableRPS}`);
}