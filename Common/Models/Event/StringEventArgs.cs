using System;

namespace Common.Models.Event
{
    public class StringEventArgs
    {
        public StringEventArgs(String message)
        {
            Message = message;
        }


        // PROPERTIES /////////////////////////////////////////////////////////////////////////////
        public String Message { get; }
    }
}