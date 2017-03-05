using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BookStore.Entity;
using Dapper;

namespace BookStore.Repository
{
    internal class OrderRepository : AbstractRepository, IOrderRepository
    {
        public OrderRepository() : base() { }
        public async Task<Order> Get(long id)
        {
            return await connection.QueryFirstOrDefaultAsync<Order>("SELECT * FROM [Order] WHERE [Id]=@Id",
                new {Id = id});
        }

        public async Task<int> Add(Order newItem)
        {
            return
                await connection.ExecuteAsync(
                    "INSERT INTO [Order] ([Id], [UserId], [BookId], [Amount]) VALUES (@Id, @UserId, @BookId, @Amount)",
                    newItem);
        }

        public async Task Clear()
        {
            await connection.ExecuteAsync("Truncate Table [Order]");
        }

        protected override SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["Order"].ConnectionString);
        }

        public Task<IEnumerable<Order>> AllOrders()
        {
            return connection.QueryAsync<Order>("SELECT * FROM [Order]");
        }
    }
}