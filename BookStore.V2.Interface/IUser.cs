using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace BookStore.V2.Interface
{
    /// <summary>
    /// Actor represent user
    /// </summary>
    public interface IUser : IGrainWithIntegerKey
    {
        Task ChargeCredit(Immutable<long> orderId, Immutable<decimal> amount);
        Task RollbackCharge(Immutable<long> orderId);
    }
}
