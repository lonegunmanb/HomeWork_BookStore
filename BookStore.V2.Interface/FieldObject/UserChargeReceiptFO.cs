namespace BookStore.V2.Interface.FieldObject
{
    public enum UserChargeReceiptStatus
    {
        Charged,
        Rollbacked
    }
    public class UserChargeReceiptFO
    {
        public long UserId { get; set; }
        public long OrderId { get; set; }
        public decimal Amount { get; set; }
        public UserChargeReceiptStatus Status { get; set; }
    }
}
