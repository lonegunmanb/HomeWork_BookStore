using System.Collections.Generic;
using System.Threading.Tasks;
using BookStore.Entity;

namespace BookStore.Repository
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> AllOrders();
    }
}
