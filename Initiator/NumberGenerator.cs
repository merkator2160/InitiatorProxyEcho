using Initiator.Models;
using System;
using System.Threading;

namespace Initiator
{
    internal class NumberGenerator : IDisposable
    {
        private readonly Int64 _offset;
        private readonly Int64 _delay;
        private readonly Timer _timer;
        private Boolean _disposed;
        private Int64 _counter;


        public NumberGenerator(Int64 offset, Int64 delay)
        {
            _offset = offset;
            _delay = delay;
            _counter = 0;
            _timer = new Timer(TimerCallback);
        }
        ~NumberGenerator()
        {
            Dispose(false);
        }


        public delegate void NumberGeneratedEventHandler(object sender, NumberGeneratedEventArgs e);
        public event NumberGeneratedEventHandler NumberGenerated = (sender, args) => { };


        // EVENTS /////////////////////////////////////////////////////////////////////////////////
        private void TimerCallback(Object state)
        {
            NumberGenerated.Invoke(this, new NumberGeneratedEventArgs(_counter++));
        }


        // FUNCTIONS //////////////////////////////////////////////////////////////////////////////
        public void Start()
        {
            _timer.Change(_offset, _delay);
        }
        public void Stop()
        {
            _timer.Change(0, Timeout.Infinite);
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
            _timer?.Dispose();
        }
    }
}