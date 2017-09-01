using System;

namespace Initiator.Models
{
    internal class NumberGeneratedEventArgs : EventArgs
    {
        public NumberGeneratedEventArgs(Int64 number)
        {
            Number = number;
        }


        // PROPERTIES /////////////////////////////////////////////////////////////////////////////
        public Int64 Number { get; }
    }
}