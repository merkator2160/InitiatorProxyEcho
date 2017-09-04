using System;

namespace Initiator.Models
{
    internal class NumberGeneratedMessage
    {
        public NumberGeneratedMessage(Int64 number)
        {
            Number = number;
        }


        // PROPERTIES /////////////////////////////////////////////////////////////////////////////
        public Int64 Number { get; set; }
    }
}