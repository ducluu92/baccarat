using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Exception
{
    public class ChargeException : SystemException
    {
        public ChargeException() { }

        public ChargeException(string message) : base(message) { }

        public ChargeException(string message, System.Exception inner) : base(message, inner) { }
    }
}