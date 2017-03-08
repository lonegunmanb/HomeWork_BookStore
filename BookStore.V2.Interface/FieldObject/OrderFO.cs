using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.V2.Interface.FieldObject
{
    public enum OrderStatus
    {
        Created,
        Confirmed,
        Canceled
    }
    public class OrderFO
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long BookId { get; set; }
        public int Amount { get; set; }
        public OrderStatus Status { get; set; }
    }
}
