using System;

namespace Common.Models.Event
{
    public class MessageReceivedEventArgs : System.EventArgs
    {
        public String Message { get; set; }
    }
}