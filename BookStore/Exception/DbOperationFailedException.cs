using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Exception
{
    [Serializable]
    public class DbOperationFailedException : System.Exception
    {
        public DbOperationFailedException() { }
        public DbOperationFailedException(string message) : base(message) { }
    }
}
