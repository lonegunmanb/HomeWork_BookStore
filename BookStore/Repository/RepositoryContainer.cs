using Microsoft.Practices.Unity;

namespace BookStore.Repository
{
    public static class RepositoryContainer
    {
        private static readonly IUnityContainer Container = new UnityContainer();

        static RepositoryContainer()
        {
            Container.RegisterType<IBookInventoryRepository, BookInventoryRepository>();
            Container.RegisterType<IBookRepository, BookRepository>();
            Container.RegisterType<IUserRepository, UserRepository>();
            Container.RegisterType<IOrderRepository, OrderRepository>();
            Container.RegisterType<ICreditCardChargeRepository, CreditCardChargeRepository>();
        }

        public static T GetRepository<T>()
        {
            return Container.Resolve<T>();
        }
    }
}
