using GalaSoft.MvvmLight.Messaging;
using Proxy.Models;
using Server.Models;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace Proxy
{
    internal class ConnectionListener : IDisposable
    {
        private readonly NetworkConfig _config;
        private readonly IMessenger _messenger;
        private Boolean _disposed;
        private readonly TcpListener _tcpListener;


        public ConnectionListener(NetworkConfig config, IMessenger messenger)
        {
            _config = config;
            _messenger = messenger;
            _tcpListener = TcpListener.Create(_config.Port);

            ThreadPool.QueueUserWorkItem(AcceptNewClientThread);
        }

        // FUNCTONS ///////////////////////////////////////////////////////////////////////////////
        public void Start()
        {
            _tcpListener.Start();
        }
        public void Stop()
        {
            _tcpListener.Stop();
        }


        // THREADS ////////////////////////////////////////////////////////////////////////////////
        private void AcceptNewClientThread(Object state)
        {
            while (!_disposed)
            {
                try
                {
                    var tcpClient = _tcpListener.AcceptTcpClient();
                    _messenger.Send(new NewClientAvaliableMessage(tcpClient));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
        }


        // IDisposable ////////////////////////////////////////////////////////////////////////////
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                _tcpListener?.Stop();
            }
        }
    }
}