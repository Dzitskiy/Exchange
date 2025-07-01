using Common.Contracts.Settings;
using Confluent.Kafka;
using External.Api.HostedServices;
using External.AppServices.Services;
using External.Infrastructure;
using Serilog;
using Serilog.Events;
using System.Text;
Console.OutputEncoding = Encoding.UTF8;

//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Debug()
//    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
//    .Enrich.FromLogContext()
//    .WriteTo.Console()
//    .WriteTo.Debug()
//    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Оптимизации для высокой нагрузки
//builder.WebHost.UseKestrel(opts => {
//    opts.Limits.MaxConcurrentConnections = 1000;
//    opts.Limits.MaxConcurrentUpgradedConnections = 1000;
//    opts.Limits.MaxRequestBodySize = 10 * 1024;
//});

#region Настройка Kafka

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
builder.Services.AddScoped<ICreateOrderService, CreateOrderService>();
builder.Services.AddSingleton<KafkaResponseConsumer>();

// Пул консьюмеров (настраиваем количество в зависимости от нагрузки)
builder.Services.AddHostedService<CreateOrderConsumer>();
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));

#endregion

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

