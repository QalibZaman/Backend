using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Application.Exceptions
{
    public class PaymentExceptions : Exception
    {
        public PaymentExceptions(string message)
               : base(message) { }
    }
}
