using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BookStore.V2.Interface;
using BookStore.V2.Interface.FieldObject;
using Dapper;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;

namespace BookStore.V2.Grain.Grain
{
    public class OrderClerkGrain : Orleans.Grain, IOrderClerk
    {
        private static readonly TimeSpan CheckDuration = TimeSpan.FromMinutes(1);
        private const string CreateOrderName = "CreateOrder";
        private const string ChargeUserCreditName = "ChargeUserCredit";
        private const string ChargeCreditCardName = "ChargeCreditCard";
        private const string AcquireBookInventoryName = "AcquireBookInventory";
        
        private readonly Dictionary<string, Func<Task>> _reminderHandlers = new Dictionary<string, Func<Task>>();

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            await _reminderHandlers[reminderName]();
        }

        public async Task PlaceNewOrder(Immutable<long> userId, Immutable<long> bookId, Immutable<int> amount)
        {
            InitRollbackHandlers();

            var orderId = this.GetPrimaryKeyLong();
            var orderReminder = await CreateNewOrderFO(userId, bookId, amount, orderId);
            var totalPrice = await GetTotalPrice(bookId, amount);
            var userCreditReminder = await ChargeUserCredit(userId, orderId, totalPrice);
            var creditCardReminder = await ChargeCreditCard(userId, orderId, totalPrice);
            var bookInventoryReminder = await AcquireBookInventory(bookId, amount, orderId);

            await ConfirmOrderFO(orderId);

            await UnregisterAllRollbackReminder(orderReminder, userCreditReminder, creditCardReminder, bookInventoryReminder);
            Console.WriteLine("Done");
        }

        private async Task UnregisterAllRollbackReminder(params IGrainReminder[] reminders)
        {
            foreach (var reminder in reminders)
            {
                await UnregisterReminder(reminder);
            }
        }

        private static async Task ConfirmOrderFO(long orderId)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync("UPDATE [Order] SET [Status]=@Status WHERE [Id]=@Id",
                    new {Id = orderId, Status = (int) OrderStatus.Confirmed});
            }
        }

        private async Task<IGrainReminder> AcquireBookInventory(Immutable<long> bookId, Immutable<int> amount, long orderId)
        {
            var bookInventoryManager = GrainFactory.GetGrain<IBookInventoryManager>(bookId.Value);
            var reminder = await RegisterOrUpdateReminder(GetAcquireBookInventoryReminderName(), CheckDuration, CheckDuration);
            await bookInventoryManager.AcquireInventory(orderId.AsImmutable(), amount);
            return reminder;
        }

        private async Task<IGrainReminder> ChargeCreditCard(Immutable<long> userId, long orderId, decimal totalPrice)
        {
            var creditCardPos = GrainFactory.GetGrain<ICreditCardPos>(orderId);
            var reminder = await RegisterOrUpdateReminder(GetChargeCreditCardReminderName(), CheckDuration, CheckDuration);
            await creditCardPos.Charge(userId, totalPrice.AsImmutable());
            return reminder;
        }

        private async Task<IGrainReminder> ChargeUserCredit(Immutable<long> userId, long orderId, decimal totalPrice)
        {
            var userGrain = GrainFactory.GetGrain<IUser>(userId.Value);
            var reminder = await RegisterOrUpdateReminder(GetChargeUserCreditReminderName(), CheckDuration, CheckDuration);
            await userGrain.ChargeCredit(orderId.AsImmutable(), totalPrice.AsImmutable());
            return reminder;
        }

        private async Task<decimal> GetTotalPrice(Immutable<long> bookId, Immutable<int> amount)
        {
            var bookKeeper = GrainFactory.GetGrain<IBookKeeper>(bookId.Value);
            var price = await bookKeeper.GetPrice();
            var totalPrice = price * amount.Value;
            return totalPrice;
        }

        private async Task<IGrainReminder> CreateNewOrderFO(Immutable<long> userId, Immutable<long> bookId, Immutable<int> amount, long orderId)
        {
            var reminder = await RegisterOrUpdateReminder(GetCreateOrderReminderName(), CheckDuration, CheckDuration);
            using (var connection = GetConnection())
            {
                await connection.ExecuteAsync("INSERT INTO [Order] ([Id], [UserId], [BookId], [Amount], [Status]) VALUES " +
                                              "(@Id, @UserId, @BookId, @Amount, @Status)", new OrderFO
                {
                    Amount = amount.Value,
                    BookId = bookId.Value,
                    Id = orderId,
                    UserId = userId.Value,
                    Status = OrderStatus.Created
                });
            }
            return reminder;
        }

        private string GetCreateOrderReminderName()
        {
            return $"{this.GetPrimaryKeyLong()} {CreateOrderName}";
        }

        private string GetChargeUserCreditReminderName()
        {
            return $"{this.GetPrimaryKeyLong()} {ChargeUserCreditName}";
        }

        private string GetChargeCreditCardReminderName()
        {
            return $"{this.GetPrimaryKeyLong()} {ChargeCreditCardName}";
        }

        private string GetAcquireBookInventoryReminderName()
        {
            return $"{this.GetPrimaryKeyLong()} {AcquireBookInventoryName}";
        }

        private void InitRollbackHandlers()
        {
            if (!_reminderHandlers.Any())
            {
                _reminderHandlers.Add(GetCreateOrderReminderName(), RollbackOrder);
                _reminderHandlers.Add(GetChargeCreditCardReminderName(), RollbackCreditCard);
                _reminderHandlers.Add(GetChargeUserCreditReminderName(), RollbackUserCredit);
                _reminderHandlers.Add(GetAcquireBookInventoryReminderName(), RollbackBookInventory);
            }
        }

        private async Task RollbackOrder()
        {
            Console.WriteLine("Rollback Order");
            var orderId = this.GetPrimaryKeyLong();
            using (var connection = GetConnection())
            {
                await connection.ExecuteAsync(
                    "UPDATE [Order] SET [Status]=@Dest WHERE [Id]=@Id AND [Status]=@Expected",
                    new {Id = orderId, Expected = (int) OrderStatus.Created, Dest = (int) OrderStatus.Canceled});
            }

            await UnregisterReminder(await GetReminder(GetCreateOrderReminderName()));
        }

        private async Task RollbackUserCredit()
        {
            if (!await CheckOrderConfirmed())
            {
                var orderId = this.GetPrimaryKeyLong();
                long userId = 0;
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();
                    userId = await connection.ExecuteScalarAsync<long>(
                        "SELECT [UserId] FROM [Order] WHERE [Id]=@Id", new {Id = orderId});;
                }
                var userGrain = GrainFactory.GetGrain<IUser>(userId);
                await userGrain.RollbackCharge(orderId.AsImmutable());
            }
            await UnregisterReminder(await GetReminder(GetChargeUserCreditReminderName()));
        }

        private async Task RollbackCreditCard()
        {
            if (!await CheckOrderConfirmed())
            {
                var orderId = this.GetPrimaryKeyLong();
                var creditCardPos = GrainFactory.GetGrain<ICreditCardPos>(orderId);
                await creditCardPos.RollbackCharge();
            }
            await UnregisterReminder(await GetReminder(GetChargeCreditCardReminderName()));
        }

        private async Task RollbackBookInventory()
        {
            if (!await CheckOrderConfirmed())
            {
                var orderId = this.GetPrimaryKeyLong();
                var bookId = 0l;
                using (var connection = GetConnection())
                {
                    bookId = await connection.ExecuteScalarAsync<long>("SELECT [BookId] FROM [Order] WHERE [Id]=@Id",
                        new {Id = orderId});
                }
                var bookInventoryManager = GrainFactory.GetGrain<IBookInventoryManager>(bookId);
                await bookInventoryManager.RollbackApplication(orderId.AsImmutable());
            }
            await UnregisterReminder(await GetReminder(GetAcquireBookInventoryReminderName()));
        }

        private async Task<bool> CheckOrderConfirmed()
        {
            var orderId = this.GetPrimaryKeyLong();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                return (await connection.ExecuteScalarAsync<int>("SELECT [Status] FROM [Order] WHERE [Id]=@Id",
                            new {Id = orderId}) == (int) OrderStatus.Confirmed);
            }
        }

        protected static SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["Order"].ConnectionString);
        }
    }
}
