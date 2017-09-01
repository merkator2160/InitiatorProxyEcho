using Proxy.Models;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace Proxy
{
    internal class ConnectionListener : IDisposable
    {
        private Boolean _disposed;
        private readonly TcpListener _tcpListener;


        public ConnectionListener(Int32 port)
        {
            _tcpListener = TcpListener.Create(port);
            _tcpListener.Start();
            ThreadPool.QueueUserWorkItem(AcceptNewClientThread);
        }


        public delegate void NewClientAvaliableEventHandler(object sender, NewClientAvaliableEventArgs e);
        public event NewClientAvaliableEventHandler NewClientAvaliable = (sender, args) => { };


        // THREADS ////////////////////////////////////////////////////////////////////////////////
        private void AcceptNewClientThread(Object state)
        {
            while (true)
            {
                try
                {
                    NewClientAvaliable.Invoke(this, new NewClientAvaliableEventArgs(_tcpListener.AcceptTcpClient()));
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(Boolean disposing)
        {
            if (!_disposed)
            {
                ReleaseUnmanagedResources();
                if (disposing)
                    ReleaseManagedResources();

                _disposed = true;
            }
        }
        private void ReleaseUnmanagedResources()
        {
            // We didn't have it yet.
        }
        private void ReleaseManagedResources()
        {
            _tcpListener?.Stop();
        }
    }
}