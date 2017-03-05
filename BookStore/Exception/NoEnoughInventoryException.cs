using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Exception
{
    [Serializable]
    public class NoEnoughInventoryException : System.Exception
    {
        public NoEnoughInventoryException(string message) : base(message){ }
    }
}
