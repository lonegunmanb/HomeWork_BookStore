using System.Threading.Tasks;
using BookStore.V2.Interface.FieldObject;
using Orleans;
using Orleans.Concurrency;

namespace BookStore.V2.Interface
{
    /// <summary>
    /// Actor who manage book inventory, one manager for each book
    /// </summary>
    public interface IBookInventoryManager : IGrainWithIntegerKey
    {
        Task<BookInventoryApplication> AcquireInventory(Immutable<long> orderId, Immutable<int> amount);
        Task RollbackApplication(Immutable<long> orderId);
    }
}
