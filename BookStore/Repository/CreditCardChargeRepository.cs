using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BookStore.Entity;
using Dapper;

namespace BookStore.Repository
{
    internal class CreditCardChargeRepository : AbstractRepository, ICreditCardChargeRepository
    {
        public CreditCardChargeRepository() : base() { }
        public async Task<CreditCardCharge> Get(long id)
        {
            return
                await connection.QueryFirstOrDefaultAsync<CreditCardCharge>(
                    "SELECT * FROM [CreditCardCharge] WHERE [Id]=@Id", new {Id = id});
        }

        public async Task<int> Add(CreditCardCharge newItem)
        {
            return
                await connection.ExecuteAsync(
                    "INSERT INTO [CreditCardCharge] ([Id], [UserId], [Amount]) VALUES (@Id, @UserId, @Amount)",
                    newItem);
        }

        protected override SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["CreditCard"].ConnectionString);
        }

        public async Task Clear()
        {
            await connection.ExecuteAsync("Truncate Table [CreditCardCharge]");
        }
    }
}