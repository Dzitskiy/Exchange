using Cassandra;
using Confluent.Kafka;
using OrderProcessor.Services;
using Prometheus;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация Kafka
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));

// Сервисы обработки
builder.Services.AddSingleton<OrderProcessorService>();
builder.Services.AddHostedService<KafkaOrderConsumer>();

builder.Services.AddSingleton<IOrderRepository, OrderRepository>();

// Регистрация Cassandra
builder.Services.AddSingleton<ICluster>(_ =>
    Cluster.Builder()
        .AddContactPoint("localhost")
        .WithPort(9042)
        .Build());

builder.Services.AddSingleton<Cassandra.ISession>(provider =>
{
    var cluster = provider.GetRequiredService<ICluster>();
    return cluster.Connect();
});

//builder.Services.AddControllers();

var app = builder.Build();

//app.MapControllers();
//app.UseHttpMetrics();
//app.MapMetrics();

app.Run();