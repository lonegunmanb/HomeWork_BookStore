using System.Threading.Tasks;
using BookStore.Entity;

namespace BookStore.Repository
{
    public interface IBookInventoryRepository : IRepository<BookInventory>
    {
        Task<int> AddInventory(long bookId, int inventory);
    }
}