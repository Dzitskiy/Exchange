using Serilog;
using Serilog.Events;
using System.Text;
using ExternalServices.AppServices.Services;
using Common.Contracts.Settings;

Console.OutputEncoding = Encoding.UTF8;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Debug()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Оптимизации для высокой нагрузки
//builder.WebHost.UseKestrel(opts => {
//    opts.Limits.MaxConcurrentConnections = 1000;
//    opts.Limits.MaxConcurrentUpgradedConnections = 1000;
//    opts.Limits.MaxRequestBodySize = 10 * 1024;
//});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddSingleton<IOrderProcessingService, OrderProcessingService>();
//builder.Services.AddHostedService<OrderIdConsumerService>();

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));

// Регистрация сервисов приложения
//builder.Services.AddSingleton<KafkaEventService>();
//builder.Services.AddScoped<IKafkaEventService, KafkaEventService>();

//OrderRegistrar.AddServices(builder.Services, builder.Configuration);

//// Регистрация Kafka Producer
//builder.Services.AddSingleton<IProducer<string, string>>(sp =>
//{
//    var config = new ProducerConfig
//    {
//        BootstrapServers = "localhost:9092", //Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS"),
//        ClientId = "order-producer-api"
//    };
//    return new ProducerBuilder<string, string>(config).Build();
//});

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