using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BookStore.Entity;
using Dapper;

namespace BookStore.Repository
{
    internal class BookRepository : AbstractRepository, IBookRepository
    {
        public BookRepository() : base() { }
        public async Task<Book> Get(long id)
        {
            return await connection.QueryFirstOrDefaultAsync<Book>("SELECT * FROM [Book] WHERE [Id]=@Id",
                new {Id = id});
        }

        public async Task<int> Add(Book newItem)
        {
            return
                await connection.ExecuteAsync(
                    "INSERT INTO [Book] ([Id], [Name], [Price]) VALUES (@Id, @Name, @Price)", newItem);
        }

        protected override SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["Book"].ConnectionString);
        }

        public async Task Clear()
        {
            await connection.ExecuteAsync("Truncate Table [Book]");
        }
    }
}