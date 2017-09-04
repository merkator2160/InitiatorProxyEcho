using System;

namespace Common.Models.MvvmLight
{
    public class HandledNumberMessage
    {
        public HandledNumberMessage(Int64 number)
        {
            Number = number;
        }


        // PROPERTIES /////////////////////////////////////////////////////////////////////////////
        public Int64 Number { get; set; }
    }
}