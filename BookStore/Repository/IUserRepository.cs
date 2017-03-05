using System.Threading.Tasks;
using BookStore.Entity;

namespace BookStore.Repository
{
    public interface IUserRepository : IRepository<User>
    {
        Task<int> ChargeCredit(long userId, decimal credit);
    }
}