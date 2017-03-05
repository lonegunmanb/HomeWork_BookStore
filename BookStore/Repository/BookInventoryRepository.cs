using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BookStore.Entity;
using Dapper;

namespace BookStore.Repository
{
    internal class BookInventoryRepository : AbstractRepository, IBookInventoryRepository
    {
        public BookInventoryRepository() : base() { }
        public async Task<BookInventory> Get(long id)
        {
            return
                await connection.QueryFirstOrDefaultAsync<BookInventory>(
                    "SELECT * FROM [BookInventory] WHERE [BookId]=@ID",
                    new {ID = id});
        }

        public async Task<int> Add(BookInventory newItem)
        {
            return
                await connection.ExecuteAsync(
                    "INSERT INTO [BookInventory] ([BookId], [Inventory]) VALUES (@BookId, @Inventory)", newItem);
        }

        public async Task<int> AddInventory(long bookId, int inventory)
        {
            return await connection.ExecuteAsync("UPDATE [BookInventory] SET [Inventory]=[Inventory]+@Inventory",
                new {Inventory = inventory});
        }

        protected override SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["BookInventory"].ConnectionString);
        }

        public async Task Clear()
        {
            await connection.ExecuteAsync("Truncate Table [BookInventory]");
        }
    }
}