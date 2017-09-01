using System;
using Common.Enums;

namespace Common.Models.Event
{
    public class StopCommandEventArgs : EventArgs
    {
        public StopCommandEventArgs(ApplicationState state, String message)
        {
            Message = message;
            State = state;
        }

        // PROPERTIES /////////////////////////////////////////////////////////////////////////////
        public String Message { get; }
        public ApplicationState State { get; set; }
    }
}