using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using BookStore.Exception;
using BookStore.V2.Interface;
using BookStore.V2.Interface.FieldObject;
using Dapper;
using Orleans;
using Orleans.Concurrency;

namespace BookStore.V2.Grain.Grain
{
    public class UserGrain : Orleans.Grain, IUser
    {
        public async Task ChargeCredit(Immutable<long> orderId, Immutable<decimal> amount)
        {
            var userId = this.GetPrimaryKeyLong();
            
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["User"].ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    var remainCredit = await
                        connection.QueryFirstOrDefaultAsync<decimal?>("SELECT [Credit] FROM [User] WHERE [Id]=@Id",
                            new {Id = userId}, transaction);

                    if (remainCredit.GetValueOrDefault() < amount.Value)
                    {
                        throw new NoEnoughCreditException(
                            $"No enough user credit, current is {remainCredit.GetValueOrDefault()}, need {amount.Value}");
                    }

                    if (await connection.ExecuteAsync("UPDATE [User] SET [Credit]=[Credit]-@Credit WHERE [Id]=@Id",
                            new {Credit = amount.Value, Id = userId}, transaction) != 1)
                    {
                        throw new DbOperationFailedException();
                    }

                    await connection.ExecuteAsync(
                        "INSERT INTO [UserChargeReceipt] ([UserId], [OrderId], [Amount], [Status]) VALUES " +
                        "(@UserId, @OrderId, @Amount, @Status)",
                        new
                        {
                            UserId = userId,
                            OrderId = orderId.Value,
                            Amount = amount.Value,
                            Status = (int) UserChargeReceiptStatus.Charged
                        }, transaction);

                    transaction.Commit();
                }
            }
        }

        public async Task RollbackCharge(Immutable<long> orderId)
        {
            Console.WriteLine("Rollback User Charge");
            var userId = this.GetPrimaryKeyLong();

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["User"].ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    if (await connection.ExecuteAsync(
                        "UPDATE [UserChargeReceipt] SET [Status]=@Dest WHERE [UserId]=@UserId AND [OrderId]=@OrderId AND [Status]=@Expected",
                        new
                        {
                            UserId = userId,
                            OrderId = orderId.Value,
                            Expected = (int)UserChargeReceiptStatus.Charged,
                            Dest = (int)UserChargeReceiptStatus.Rollbacked
                        }, transaction) != 1)
                    {
                        transaction.Rollback();
                        return;
                    }

                    var userChargeReceipt =
                        await connection.QueryFirstOrDefaultAsync<UserChargeReceiptFO>(
                            "SELECT * FROM [UserChargeReceipt] WHERE [UserId]=@UserId " +
                            "AND [OrderId]=@OrderId", new { UserId = userId, OrderId = orderId.Value }, transaction);

                    await connection.ExecuteAsync("UPDATE [User] SET [Credit]=[Credit]+@Credit WHERE [Id]=@Id",
                        new { Credit = userChargeReceipt.Amount, Id = userId }, transaction);

                    transaction.Commit();
                }
            }
        }
    }
}
