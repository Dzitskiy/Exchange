using Common.Contracts.Settings;
using Confluent.Kafka;
using OrderApi.HostedServices;
using OrderApi.Infrastructure;
using OrderApi.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

Log.Information("Starting Order API application...");

// Конфигурация Kafka
var kafkaConfig = new ProducerConfig
{
    BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],
    EnableIdempotence = true,
    Acks = Acks.All,
    MessageSendMaxRetries = 3
};

Log.Information($"Kafka BootstrapServers: {kafkaConfig.BootstrapServers}");

builder.Services.AddSingleton(kafkaConfig);
builder.Services.AddSingleton<KafkaProducerFactory>();
builder.Services.AddSingleton<ResponseTracker>();
builder.Services.AddSingleton<KafkaResponseConsumer>();

// Регистрация сервисов
builder.Services.AddScoped<IOrderCreatingService, OrderCreatingService>();

// Пул консьюмеров (настраиваем количество в зависимости от нагрузки)
builder.Services.AddHostedService<KafkaConsumerHostedService>();

Log.Information("Configuring Kafka options from appsettings.json");

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));



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