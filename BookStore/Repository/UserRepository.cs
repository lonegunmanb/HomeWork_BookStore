using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BookStore.Entity;
using Dapper;

namespace BookStore.Repository
{
    internal class UserRepository : AbstractRepository, IUserRepository
    {
        public UserRepository() : base() { }
        public async Task<User> Get(long id)
        {
            return await connection.QueryFirstAsync<User>("SELECT * FROM [User] WHERE [Id]=@Id", new {Id = id});
        }

        public async Task<int> Add(User newItem)
        {
            return
                await connection.ExecuteAsync(
                    "INSERT INTO [User] ([Id], [Name], [Credit]) VALUES (@Id, @Name, @Credit)", newItem);
        }

        public async Task<int> ChargeCredit(long userId, decimal credit)
        {
            return await connection.ExecuteAsync("UPDATE [User] SET [Credit]=[Credit]-@Credit WHERE [Id]=@Id",
                new {Id = userId, Credit = credit});
        }

        public async Task Clear()
        {
            await connection.ExecuteAsync("Truncate Table [User]");
        }

        protected override SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["User"].ConnectionString);
        }
    }
}