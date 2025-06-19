using Cassandra;
using Microsoft.Extensions.Configuration;
using OrderServices.AppServices.Order.Repositories;
using OrderServices.AppServices.Order.Services;
using OrderServices.DataAccess.Orders;
using Serilog;
using Serilog.Events;
using System.Text;

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

#region AddServices

//OrderRegistrar.AddServices(builder.Services, builder.Configuration);
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

#endregion

#region Cassandra

// Регистрация Cassandra
builder.Services.AddSingleton<ICluster>(_ =>
    Cluster.Builder()
        .AddContactPoint("localhost")
        .WithPort(9042)
        .Build());

builder.Services.AddScoped<Cassandra.ISession>(provider =>
{
    var cluster = provider.GetRequiredService<ICluster>();
    return cluster.Connect();
});

#endregion

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
