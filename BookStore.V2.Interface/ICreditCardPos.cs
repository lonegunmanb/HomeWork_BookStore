using System.Threading.Tasks;
using BookStore.V2.Interface.FieldObject;
using Orleans;
using Orleans.Concurrency;

namespace BookStore.V2.Interface
{
    /// <summary>
    /// Actor represent a credit card pos, one pos for each order
    /// </summary>
    public interface ICreditCardPos : IGrainWithIntegerKey
    {
        Task<CreditCardChargeFO> Charge(Immutable<long> userId, Immutable<decimal> amount);
        Task RollbackCharge();
    }
}
