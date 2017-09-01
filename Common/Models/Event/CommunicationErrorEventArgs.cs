using System;

namespace Common.Models.Event
{
    public class CommunicationErrorEventArgs : System.EventArgs
    {
        public CommunicationErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }


        // PROPERTIES /////////////////////////////////////////////////////////////////////////////
        public Exception Exception { get; }
    }
}