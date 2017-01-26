using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookAStar.Data
{
    class ApiCallFailedException : Exception
    {
        public ApiCallFailedException(string message) : base(message)
        {

        }

        public ApiCallFailedException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
