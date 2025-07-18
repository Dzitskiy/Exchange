using Cassandra;
using Cassandra.Mapping;
using OrderProcessor.Entities;
using Serilog;
using System.Diagnostics.Metrics;
using System.Drawing;

namespace OrderProcessor.Services
{
    /// <inheritdoc cref="IOrderRepository"/>
    public class OrderRepository : IOrderRepository
    {
        private readonly Cassandra.ISession _session;
        private readonly IMapper _mapper;
        private readonly Serilog.ILogger _logger;

        public OrderRepository(Cassandra.ISession session)
        {
            _session = session;
            _mapper = new Mapper(session);

            _logger = Log.ForContext<OrderRepository>();

            InitializeDatabase();
        }

        public async Task<Guid> CreateOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            try
            {
                //await _mapper.InsertAsync(order);

                await _session.ExecuteAsync(
                    new SimpleStatement(
                        "INSERT INTO \"order\" (id, createdat, operation, instrumentid, trademode, clientorderid, side, ordertype, price, size) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        order.Id,
                        order.CreatedAt.ToUniversalTime(),
                        order.Operation,
                        order.InstrumentId,
                        order.TradeMode,
                        order.ClientOrderId,
                        order.Side,
                        order.OrderType,
                        order.Price,
                        order.Size
                        ));

                _logger.Information("Order created with ID: {OrderId}", order.Id);

                return order.Id;

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating order with ID: {OrderId}", order.Id);
                throw;
            }
        }

        public async Task<Order> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var order = await _mapper.FirstOrDefaultAsync<Order>("SELECT * FROM \"order\" WHERE id = ?", id); ;

            return order;
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeDatabase()
        {
            _logger.Information("Cassandra repository initialized");

            // Создание keyspace
            _session.Execute(@"
            CREATE KEYSPACE IF NOT EXISTS orders_keyspace
            WITH replication = {
                'class': 'SimpleStrategy',
                'replication_factor': 1
            };");

            // Использование keyspace
            _session.ChangeKeyspace("orders_keyspace");

            // Создание таблицы
            _session.Execute(@"
            CREATE TABLE IF NOT EXISTS ""order"" (
                id uuid PRIMARY KEY,
                createdat timestamp,                
                operation text,
                instrumentid text,
                trademode text,
                clientorderid text,
                side text,
                ordertype text,
                price text,
                size text
            );");
        }
    }
}