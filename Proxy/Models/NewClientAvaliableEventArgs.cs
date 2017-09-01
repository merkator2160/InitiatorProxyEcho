using System;
using System.Net.Sockets;

namespace Proxy.Models
{
    internal class NewClientAvaliableEventArgs : EventArgs
    {
        public NewClientAvaliableEventArgs(TcpClient client)
        {
            Client = client;
        }


        // PROPERTIES /////////////////////////////////////////////////////////////////////////////
        public TcpClient Client { get; }
    }
}