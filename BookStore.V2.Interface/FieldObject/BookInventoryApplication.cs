using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.V2.Interface.FieldObject
{
    public enum BookInventoryApplicationStatus
    {
        Granted,
        Rollbacked,
    }
    public class BookInventoryApplication
    {
        public long OrderId { get; set; }
        public long BookId { get; set; }
        public int Amount { get; set; }
        public BookInventoryApplicationStatus Status { get;set; }
    }
}
