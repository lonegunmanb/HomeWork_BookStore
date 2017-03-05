using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Exception
{
    [Serializable]
    public class NoEnoughCreditException : System.Exception
    {
        public NoEnoughCreditException(string message) : base(message) { }
    }
}
