﻿using GalaSoft.MvvmLight.Messaging;
using Initiator.Models;
using System;
using System.Threading;

namespace Initiator
{
    internal class NumberGenerator : IDisposable
    {
        private readonly NumberGeneratorConfig _config;
        private readonly IMessenger _messenger;
        private readonly Timer _timer;
        private Boolean _disposed;
        private Int64 _counter;


        public NumberGenerator(NumberGeneratorConfig config, IMessenger messenger)
        {
            _config = config;
            _messenger = messenger;
            _counter = 0;
            _timer = new Timer(TimerCallback);
        }


        // EVENTS /////////////////////////////////////////////////////////////////////////////////
        private void TimerCallback(Object state)
        {
            _messenger.Send(new NumberGeneratedMessage(_counter++));
        }


        // FUNCTIONS //////////////////////////////////////////////////////////////////////////////
        public void Start()
        {
            _timer.Change(_config.Offset, _config.Delay);
        }
        public void Stop()
        {
            _timer.Change(0, Timeout.Infinite);
        }


        // IDisposable ////////////////////////////////////////////////////////////////////////////
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                _timer?.Dispose();
            }
        }
    }
}