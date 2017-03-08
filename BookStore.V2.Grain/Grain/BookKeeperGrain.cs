using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BookStore.V2.Interface;
using Dapper;
using Orleans;

namespace BookStore.V2.Grain.Grain
{
    internal class BookKeeperGrain : Orleans.Grain, IBookKeeper
    {
        public async Task<decimal> GetPrice()
        {
            var bookId = this.GetPrimaryKeyLong();
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Book"].ConnectionString))
            {
                await connection.OpenAsync();

                return await connection.QueryFirstOrDefaultAsync<decimal>("SELECT [Price] FROM [Book] WHERE [Id]=@Id",
                    new {Id = bookId});
            }
        }
    }
}
