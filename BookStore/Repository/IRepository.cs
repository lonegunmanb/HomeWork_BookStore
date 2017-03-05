using System;
using System.Threading.Tasks;

namespace BookStore.Repository
{
    public interface IRepository<T> : IDisposable
    {
        Task<T> Get(long id);
        Task<int> Add(T newItem);
        Task Clear();
    }
}