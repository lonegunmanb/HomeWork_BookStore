using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BookStore.V2.Interface;
using BookStore.V2.Interface.FieldObject;
using Dapper;
using Orleans;
using Orleans.Concurrency;

namespace BookStore.V2.Grain.Grain
{
    internal class CreditCardPosGrain : Orleans.Grain, ICreditCardPos
    {
        public async Task<CreditCardChargeFO> Charge(Immutable<long> userId, Immutable<decimal> amount)
        {
            var orderId = this.GetPrimaryKeyLong();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                var newCreditCardChargeFO = new CreditCardChargeFO
                {
                    Amount = amount.Value,
                    UserId = userId.Value,
                    OrderId = orderId,
                    Status = CreditCardChargeStatus.Charged
                };

                await connection.ExecuteAsync(
                    "INSERT INTO [CreditCardCharge] ([Amount], [UserId], [OrderId], [Status]) VALUES " +
                    "(@Amount, @UserId, @OrderId, @Status)", newCreditCardChargeFO);

                return newCreditCardChargeFO;
            }
        }

        private static SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["CreditCard"].ConnectionString);
        }

        public async Task RollbackCharge()
        {
            Console.WriteLine("Rollback CreditCard");
            var orderId = this.GetPrimaryKeyLong();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(
                    "UPDATE [CreditCardCharge] SET [Status]=@Dest WHERE [OrderId]=@OrderId AND [Status]=@Expected",
                    new
                    {
                        OrderId = orderId,
                        Expected = (int) CreditCardChargeStatus.Charged,
                        Dest = (int) CreditCardChargeStatus.Rollbacked
                    });
            }
        }
    }
}
