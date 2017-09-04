using Common.Models.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;

namespace Common
{
    public class ConsoleWorker : IDisposable
    {
        private Boolean _disposed;
        private readonly IMessenger _messenger;
        private readonly Dictionary<String, Action> _commandDictionary;


        public ConsoleWorker(IMessenger messenger)
        {
            _messenger = messenger;
            _messenger.Register<ConsoleMessage>(this, PrintText);
            _commandDictionary = new Dictionary<String, Action>()
            {
                {
                    "start", StartCommandHandler
                },
                {
                    "stop", StopCommandHandler
                },
                {
                    "exit", ExitCommandHandler
                },
            };
        }


        // FUNCTIONS //////////////////////////////////////////////////////////////////////////////
        public void Run()
        {
            while (!_disposed)
            {
                var commandText = Console.ReadLine();
                if (_commandDictionary.ContainsKey(commandText))
                {
                    _commandDictionary[commandText].Invoke();
                }
            }
        }


        // HANDLERS ///////////////////////////////////////////////////////////////////////////////
        private void StartCommandHandler()
        {
            _messenger.Send(new StartCommandEnteredMessage());
        }
        private void StopCommandHandler()
        {
            _messenger.Send(new StopCommandEnteredMessage());
        }
        private void ExitCommandHandler()
        {
            _messenger.Send(new ExitCommandEnteredMessage());
        }
        private void PrintText(ConsoleMessage message)
        {
            Console.WriteLine(message.Text);
        }


        // IDisposable ////////////////////////////////////////////////////////////////////////////
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
}