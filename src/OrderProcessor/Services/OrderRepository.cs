using Cassandra;
using Cassandra.Mapping;

namespace OrderProcessor.Services
{
    /// <inheritdoc cref="IOrderRepository"/>
    public class OrderRepository : IOrderRepository
    {
        private readonly Cassandra.ISession _session;
        private readonly IMapper _mapper;

        public OrderRepository(Cassandra.ISession session)
        {
            _session = session;
            _mapper = new Mapper(session);

            InitializeDatabase();
        }

        public async Task<Guid> CreateOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            //await _mapper.InsertAsync(order);
            
            await _session.ExecuteAsync(
                new SimpleStatement(
                    "INSERT INTO \"Order\" (id, description, createdat) VALUES (?, ?, ?)",
                    order.Id,
                    order.Description,
                    order.CreatedAt.ToUniversalTime()
                    ));

            return order.Id;
        }

        public async Task<Order> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var order = await _mapper.FirstOrDefaultAsync<Order>("SELECT * FROM \"Order\" WHERE id = ?", id); ;

            return order;
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeDatabase()
        {
            // Создание keyspace
            _session.Execute(@"
            CREATE KEYSPACE IF NOT EXISTS order_ks
            WITH replication = {
                'class': 'SimpleStrategy',
                'replication_factor': 1
            };");

            // Использование keyspace
            _session.ChangeKeyspace("order_ks");

            // Создание таблицы
            _session.Execute(@"
            CREATE TABLE IF NOT EXISTS ""Order"" (
                id uuid PRIMARY KEY,
                description text,
                createdat timestamp
            );");
        }
    }
}