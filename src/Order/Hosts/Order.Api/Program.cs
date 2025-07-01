using Common.Contracts.Settings;
using OrderProcessing.Api.HostedServices;
using OrderProcessing.AppServices.Order.Services;
using OrderProcessing.WebApi;
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

// Add services to the container.
//builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();


// Конфигурация Kafka
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));

// Сервисы обработки
builder.Services.AddSingleton<IOrderProcessingService, OrderProcessingService>();
builder.Services.AddHostedService<OrderProcessingConsumer>();

builder.Services.AddControllers();
var app = builder.Build();

app.MapControllers();
//app.UseHttpMetrics();
//app.MapMetrics();

app.Run();

/*

// Регистрация сервисов
builder.Services.AddServices(builder.Configuration);


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

*/