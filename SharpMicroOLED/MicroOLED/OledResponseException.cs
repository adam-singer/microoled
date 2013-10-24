using System;
using System.Collections.Generic;
using System.Text;

namespace MicroOLED
{
    public class OledResponseException : Exception
    {
        public OledResponseException()
        {
        }

        public OledResponseException(string message)
            : base(message)
        {
        }

        public OledResponseException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
