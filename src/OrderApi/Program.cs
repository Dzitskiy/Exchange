using Confluent.Kafka;
using OrderApi.HostedServices;
using OrderApi.Infrastructure;
using OrderApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация Kafka
var kafkaConfig = new ProducerConfig
{
    BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],
    EnableIdempotence = true,
    Acks = Acks.All,
    MessageSendMaxRetries = 3
};

builder.Services.AddSingleton(kafkaConfig);
builder.Services.AddSingleton<KafkaProducerFactory>();
builder.Services.AddSingleton<ResponseTracker>();

// Регистрация сервисов
builder.Services.AddScoped<IKafkaOrderService, KafkaOrderService>();
builder.Services.AddSingleton<KafkaResponseConsumer>();

// Пул консьюмеров (настраиваем количество в зависимости от нагрузки)
builder.Services.AddHostedService<KafkaConsumerHostedService>();
builder.Services.Configure<KafkaConsumerOptions>(builder.Configuration.GetSection("Kafka"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();