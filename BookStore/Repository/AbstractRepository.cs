using System;
using System.Data.SqlClient;

namespace BookStore.Repository
{
    internal abstract class AbstractRepository : IDisposable
    {
        protected readonly SqlConnection connection;
        protected AbstractRepository()
        {
            connection = GetConnection();
        }

        protected abstract SqlConnection GetConnection();

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
