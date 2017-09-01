using System;
using Common.Enums;

namespace Common.Models.Event
{
    public class StartCommandEventArgs : EventArgs
    {
        public StartCommandEventArgs(ApplicationState state, String message)
        {
            Message = message;
            State = state;
        }

        // PROPERTIES /////////////////////////////////////////////////////////////////////////////
        public String Message { get; }
        public ApplicationState State { get; set; }
    }
}