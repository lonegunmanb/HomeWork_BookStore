namespace BookStore.Entity
{
    public class Order
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long BookId { get; set; }
        public int Amount { get; set; }
    }
}
