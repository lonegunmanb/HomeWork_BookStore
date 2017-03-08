using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using BookStore.Entity;
using BookStore.Exception;
using BookStore.Repository;

namespace BookStore
{
    public class OrderService
    {
        private static long _id = 0;
        public async Task<long> PlaceNewOrder(long userId, long bookId, int bookAmount)
        {
            long orderId = 0;
            using (var transactionScope = new TransactionScope())
            {
                using (var bookRepository = RepositoryContainer.GetRepository<IBookRepository>())
                {
                    var book = await bookRepository.Get(bookId);
                    var bookPrice = book.Price;

                    var total = bookPrice * bookAmount;

                    using (var userRepository = RepositoryContainer.GetRepository<IUserRepository>())
                    {
                        var user = await userRepository.Get(userId);

                        if (user.Credit < total)
                        {
                            throw new NoEnoughCreditException(
                                $"No enough user credit, current is {user.Credit}, need {total}");
                        }

                        using (
                            var bookInventoryRepository = RepositoryContainer.GetRepository<IBookInventoryRepository>())
                        {
                            var bookInventory = await bookInventoryRepository.Get(bookId);

                            if (bookInventory.Inventory < bookAmount)
                            {
                                throw new NoEnoughInventoryException(
                                    $"No enough book inventory, current is {bookInventory}, need {bookAmount}");
                            }

                            using (var creditCardChargeRepository =
                                RepositoryContainer.GetRepository<ICreditCardChargeRepository>())
                            {
                                if (await creditCardChargeRepository.Add(new CreditCardCharge
                                {
                                    Amount = total,
                                    UserId = userId,
                                    Id = Interlocked.Increment(ref _id)
                                }) != 1)
                                {
                                    throw new DbOperationFailedException();
                                }

                                if (await userRepository.ChargeCredit(userId, total) != 1)
                                {
                                    throw new DbOperationFailedException();
                                }

                                if (await bookInventoryRepository.AddInventory(bookId, -bookAmount) != 1)
                                {
                                    throw new DbOperationFailedException();
                                }

                                using (var orderRepository = RepositoryContainer.GetRepository<IOrderRepository>())
                                {
                                    orderId = Interlocked.Increment(ref _id);
                                    if (
                                        await orderRepository.Add(new Order
                                        {
                                            Amount = bookAmount,
                                            BookId = bookId,
                                            UserId = userId,
                                            Id = orderId
                                        }) != 1)
                                    {
                                        throw new DbOperationFailedException();
                                    }
                                }
                            }
                        }
                    }
                }

                transactionScope.Complete();
            }
            return orderId;
        }
    }
}
