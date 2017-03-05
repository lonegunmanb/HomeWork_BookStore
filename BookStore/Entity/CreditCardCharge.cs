namespace BookStore.Entity
{
    public class CreditCardCharge
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public decimal Amount { get; set; }
    }
}
