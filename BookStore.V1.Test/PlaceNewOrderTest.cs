using System.Linq;
using System.Threading.Tasks;
using BookStore.Entity;
using BookStore.Repository;
using Xunit;

namespace BookStore.V1.Test
{
    public class PlaceNewOrderTest
    {
        public PlaceNewOrderTest()
        {
            using (var orderRepository = RepositoryContainer.GetRepository<IOrderRepository>())
            {
                using (var userRepository = RepositoryContainer.GetRepository<IUserRepository>())
                {
                    using (var bookRepository = RepositoryContainer.GetRepository<IBookRepository>())
                    {
                        using (
                            var bookInventoryRepository = RepositoryContainer.GetRepository<IBookInventoryRepository>())
                        {
                            using (
                                var creditCarcChargeRepository =
                                    RepositoryContainer.GetRepository<ICreditCardChargeRepository>())
                            {
                                Task.WaitAll(orderRepository.Clear(),
                                    userRepository.Clear(),
                                    bookRepository.Clear(),
                                    bookInventoryRepository.Clear(),
                                    creditCarcChargeRepository.Clear());

                                Task.WaitAll(
                                    bookRepository
                                        .Add(new Book {Id = 1, Name = "TestBook", Price = 100}),
                                    bookInventoryRepository
                                        .Add(new BookInventory {BookId = 1, Inventory = 100}),
                                    userRepository
                                        .Add(new User {Id = 1, Credit = 1000, Name = "zjhe"}));
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task place_a_new_order()
        {
            var orderService = new OrderService();
            await orderService.PlaceNewOrder(1, 1, 5);
            using (var userRepository = RepositoryContainer.GetRepository<IUserRepository>())
            {
                var user = await userRepository.Get(1);
                Assert.Equal(500, user.Credit);
            }
            using (var orderRepository = RepositoryContainer.GetRepository<IOrderRepository>())
            {
                var orders = await orderRepository.AllOrders();
                Assert.Equal(1, orders.Count());

                var order = orders.First();
                Assert.Equal(1, order.UserId);
                Assert.Equal(1, order.BookId);
                Assert.Equal(5, order.Amount);
            }
        }
    }
}