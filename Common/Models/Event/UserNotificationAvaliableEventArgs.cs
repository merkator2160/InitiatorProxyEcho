using System;

namespace Common.Models.Event
{
    public class UserNotificationAvaliableEventArgs : System.EventArgs
    {
        public UserNotificationAvaliableEventArgs(String message)
        {
            Message = message;
        }


        // PROPERTIES /////////////////////////////////////////////////////////////////////////////
        public String Message { get; }
    }
}