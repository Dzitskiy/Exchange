using Cassandra;
using Polly;
using OrderProcessor.HostedServices;
using OrderProcessor.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting application...");

    var builder = WebApplication.CreateBuilder(args);

    // Добавляем Serilog с конфигурацией из appsettings.json
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            //.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            );

    // Конфигурация Kafka
    builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));

    //// Конфигурация Cassandra
    //var cassandraHost = builder.Configuration["CASSANDRA_HOST"] ?? "localhost";
    //var cassandraPort = int.Parse(builder.Configuration["CASSANDRA_PORT"] ?? "9042");
    //var keyspace = builder.Configuration["CASSANDRA_KEYSPACE"] ?? "order_keyspace";

    // Сервисы обработки
    builder.Services.AddHostedService<KafkaConsumerHostedService>();

    builder.Services.AddSingleton<IOrderProcessingService, OrderProcessingService>();
    builder.Services.AddSingleton<IOrderRepository, OrderRepository>();

    // Регистрация Cassandra

    var contactPoint = builder.Configuration.GetSection("Cassandra:ContactPoints").Value;
    Log.Information($"Cassandra contact point: {contactPoint}");

    var keyspace = "orders_keyspace";

    Log.Information("Connecting to Cassandra...");
    var cluster = Cluster.Builder()
        .AddContactPoint(contactPoint)
        .WithPort(9042)
        .WithCredentials("cassandra", "cassandra")
        .Build();

    // Политика повторных попыток
    var retryPolicy = Policy
        .Handle<NoHostAvailableException>()
        .WaitAndRetry(new[]
        {
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(3),
        TimeSpan.FromSeconds(5)
        });

    retryPolicy.Execute(() =>
    {
        var session = cluster.Connect();
        Log.Information($"Connected to Cassandra cluster: {cluster.Metadata.ClusterName}");

        session.Execute($"CREATE KEYSPACE IF NOT EXISTS {keyspace} " +
            "WITH REPLICATION = { 'class' : 'SimpleStrategy', 'replication_factor' : 1 };");
        
        Log.Information($"Using keyspace: {keyspace}");
        session.ChangeKeyspace(keyspace);

        builder.Services.AddSingleton<Cassandra.ISession>(session);

        //builder.Services.AddSingleton<Cassandra.ISession>(provider =>
        //{
        //    var cluster = provider.GetRequiredService<ICluster>();
        //    return cluster.Connect();
        //});
    });

    Log.Information("Application started successfully");
    var app = builder.Build();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}