using Confluent.Kafka;
using System.Collections.Concurrent;

namespace OrderApi.Infrastructure;

public class KafkaProducerFactory
{
    private readonly ProducerConfig _config;
    private readonly ConcurrentBag<IProducer<Null, string>> _producers = new();

    public KafkaProducerFactory(ProducerConfig config) => _config = config;

    public IProducer<Null, string> GetProducer()
    {
        if (_producers.TryTake(out var producer))
            return producer;

        return new ProducerBuilder<Null, string>(_config).Build();
    }

    public void ReturnProducer(IProducer<Null, string> producer) =>
        _producers.Add(producer);
}