using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BookStore.Exception;
using BookStore.V2.Interface;
using BookStore.V2.Interface.FieldObject;
using Dapper;
using Orleans;
using Orleans.Concurrency;

namespace BookStore.V2.Grain.Grain
{
    internal class BookInventoryManagerGrain : Orleans.Grain, IBookInventoryManager
    {
        public async Task<BookInventoryApplication> AcquireInventory(Immutable<long> orderId, Immutable<int> amount)
        {
            var bookId = this.GetPrimaryKeyLong();
            using (
                var connection =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["BookInventory"].ConnectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    var remainInventory =
                        await connection.QueryFirstOrDefaultAsync<decimal>(
                            "SELECT [Inventory] FROM [BookInventory] WHERE [BookId]=@Id", new {Id = bookId}, transaction);

                    if (remainInventory < amount.Value)
                    {
                        throw new NoEnoughInventoryException(
                            $"No enough book inventory, current is {remainInventory}, need {amount.Value}");
                    }

                    if (await connection.ExecuteAsync(
                            "UPDATE [BookInventory] SET [Inventory]=[Inventory]-@Inventory WHERE [BookId]=@Id",
                            new {Inventory = amount.Value, Id = bookId}, transaction) != 1)
                    {
                        throw new DbOperationFailedException();
                    }

                    var newApplication = new BookInventoryApplication
                    {
                        OrderId = orderId.Value,
                        BookId = bookId,
                        Amount = amount.Value,
                        Status = (int) BookInventoryApplicationStatus.Granted
                    };

                    await connection.ExecuteAsync(
                        "INSERT INTO [BookInventoryApplication] ([OrderId], [BookId], [Amount], [Status]) VALUES " +
                        "(@OrderId, @BookId, @Amount, @Status)",
                        newApplication
                   , transaction);

                    transaction.Commit();
                    return newApplication;
                }
            }
        }

        public async Task RollbackApplication(Immutable<long> orderId)
        {
            Console.WriteLine("Rollback Inventory");
            using (
                var connection =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["BookInventory"].ConnectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    if (await connection.ExecuteAsync(
                            "UPDATE [BookInventoryApplication] SET [Status]=@Dest WHERE [OrderId]=@OrderId AND [Status]=@Expected",
                            new {OrderId = orderId.Value, Dest = (int) BookInventoryApplicationStatus.Rollbacked, Expected = (int)BookInventoryApplicationStatus.Granted}, transaction) != 1)
                    {
                        return;
                    }
                    var application =
                        await connection.QueryFirstOrDefaultAsync<BookInventoryApplication>(
                            "SELECT * FROM [BookInventoryApplication] WHERE [OrderId]=@OrderId",
                            new {OrderId = orderId.Value}, transaction);
                    await connection.ExecuteAsync(
                        "UPDATE [BookInventory] SET [Inventory]=[Inventory]+@Inventory WHERE [BookId]=@BookId",
                        new { Inventory = application.Amount, BookId = application.BookId }, transaction);
                    transaction.Commit();
                }
            }
        }
    }
}
