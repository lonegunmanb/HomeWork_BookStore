using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace BookStore.V2.Interface
{
    /// <summary>
    /// Actor represent a book saler, one saler for each book
    /// </summary>
    public interface IOrderClerk : IGrainWithIntegerKey, IRemindable
    {
        Task PlaceNewOrder(Immutable<long> userId, Immutable<long> bookId, Immutable<int> amount);
    }
}
