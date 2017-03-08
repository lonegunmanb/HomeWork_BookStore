namespace BookStore.V2.Interface.FieldObject
{
    public enum CreditCardChargeStatus
    {
        Charged,
        Rollbacked
    }
    public class CreditCardChargeFO
    {
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public decimal Amount { get; set; }
        public CreditCardChargeStatus Status { get; set; }
    }
}
