using Common.Enums;

namespace Common.Models.Event
{
    public class StateMessageReceivedEventArgs : System.EventArgs
    {
        public StateMessageReceivedEventArgs(ApplicationState state)
        {
            State = state;
        }


        // PROPERTIES /////////////////////////////////////////////////////////////////////////////
        public ApplicationState State { get; }
    }
}