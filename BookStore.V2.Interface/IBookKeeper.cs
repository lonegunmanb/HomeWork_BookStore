using System.Threading.Tasks;
using Orleans;

namespace BookStore.V2.Interface
{
    /// <summary>
    /// Actor represent a book
    /// </summary>
    public interface IBookKeeper : IGrainWithIntegerKey
    {
        Task<decimal> GetPrice();
    }
}
