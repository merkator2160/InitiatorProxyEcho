using Common.Models.Enums;
using Common.Models.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Threading;

namespace Proxy
{
    internal class ProxyServer
    {
        private const ServerType Type = ServerType.Proxy;

        private readonly IMessenger _messenger;
        private readonly ConnectionListener _connectionListener;
        private ServerState _serverState;

        private static CommuticationHub _commuticationHub;


        public ProxyServer(IMessenger messenger, ConnectionListener connectionListener, CommuticationHub communicationHub)
        {
            _messenger = messenger;
            _commuticationHub = communicationHub;
            _connectionListener = connectionListener;
            _messenger.Register<StartCommandEnteredMessage>(this, OnStartCommandEntered);
            _messenger.Register<ExitCommandEnteredMessage>(this, OnExitCommandEntered);

            _serverState = ServerState.Suspended;
            _messenger.Send(new ConsoleMessage($"{Type}... {_serverState}"));
        }


        // EVENTS /////////////////////////////////////////////////////////////////////////////////
        private void OnStartCommandEntered(StartCommandEnteredMessage message)
        {
            _commuticationHub.Start();
            _connectionListener.Start();

            _serverState = ServerState.Working;
            _messenger.Send(new ConsoleMessage($"{Type}... {_serverState}"));
        }
        private void OnExitCommandEntered(ExitCommandEnteredMessage message)
        {
            _serverState = ServerState.Exited;
            _messenger.Send(new ConsoleMessage($"{Type}... {_serverState}"));

            Thread.Sleep(1000);
            Environment.Exit(0);
        }
    }
}