using Serilog;
using Serilog.Events;
using System.Text;
using Confluent.Kafka;
using ExternalServices.AppServices.Services;

Console.OutputEncoding = Encoding.UTF8;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Debug()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрация сервисов приложения
//builder.Services.AddSingleton<KafkaEventService>();
builder.Services.AddScoped<IKafkaEventService, KafkaEventService>();

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