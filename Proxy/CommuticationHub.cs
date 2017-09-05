using Common;
using Common.Extensions;
using Common.Models.Enums;
using Common.Models.MvvmLight;
using Common.Models.Network;
using GalaSoft.MvvmLight.Messaging;
using Proxy.Models;
using Server.Models;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace Proxy
{
    internal class CommuticationHub : IDisposable
    {
        private readonly IMessenger _messenger;
        private readonly NetworkConfig _config;
        private Boolean _disposed;
        private ConnectionBuffer _initiatorClient;
        private ConnectionBuffer _echoClient;
        private readonly ManualResetEventSlim _workingMres;
        private readonly ConcurrentQueue<TcpClient> _newClientQueue;


        public CommuticationHub(IMessenger messenger, NetworkConfig config)
        {
            _config = config;
            _messenger = messenger;
            _messenger.Register<NewClientAvaliableMessage>(this, OnNewClientAvaliable);

            _newClientQueue = new ConcurrentQueue<TcpClient>();
            _workingMres = new ManualResetEventSlim(false);

            ThreadPool.QueueUserWorkItem(NewClientHandlingThread);

            for (var i = 0; i < _config.NumberOfThreads; i++)
            {
                ThreadPool.QueueUserWorkItem(InitiatorHandlerThread);
                ThreadPool.QueueUserWorkItem(EchoHandlerThread);
            }
        }


        // HANDLERS ///////////////////////////////////////////////////////////////////////////////
        private void OnNewClientAvaliable(NewClientAvaliableMessage message)
        {
            _newClientQueue.Enqueue(message.Client);
        }


        // THREADS ////////////////////////////////////////////////////////////////////////////////
        private void NewClientHandlingThread(Object state)
        {
            while (!_disposed)
            {
                try
                {
                    _workingMres.Wait();
                    if (_newClientQueue.IsEmpty)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    if (_newClientQueue.TryDequeue(out TcpClient client))
                    {
                        CheckWhoIsIt(client);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
        }
        private void CheckWhoIsIt(TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                var responce = stream.ReadObject<NetworkMessage>();
                if (responce.Type != MessageType.ConnectionRequest)
                    return;
                if (responce.Data == null)
                    return;

                var serverType = (ServerType)BitConverter.ToInt32(responce.Data, 0);
                switch (serverType)
                {
                    case ServerType.Initiator:
                        if (_initiatorClient == null)
                        {
                            _initiatorClient = new ConnectionBuffer(responce.SessionId.ToGuid(), _config.NumberOfThreads);
                        }
                        _initiatorClient.SetClient(client);
                        break;
                    case ServerType.Echo:
                        if (_initiatorClient == null)
                        {
                            _echoClient = new ConnectionBuffer(responce.SessionId.ToGuid(), _config.NumberOfThreads);
                        }
                        _echoClient.SetClient(client);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }


        private void InitiatorHandlerThread(Object state)
        {
            while (!_disposed)
            {
                try
                {
                    _workingMres.Wait();
                    if (_initiatorClient == null || !_initiatorClient.Connected || _initiatorClient.ReceivedMessageQueue.IsEmpty)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    HandleInitiatorServerMessage();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
        }
        private void HandleInitiatorServerMessage()
        {
            if (_initiatorClient.ReceivedMessageQueue.TryDequeue(out NetworkMessage message))
            {
                switch (message.Type)
                {
                    case MessageType.ConnectionRequest:
                    case MessageType.KeepAlive:
                        break;

                    case MessageType.Number:
                        _echoClient.SendMessageQueue.Enqueue(message);
                        break;

                    case MessageType.State:
                        var initiatorState = (ServerState)BitConverter.ToInt32(message.Data, 0);
                        _messenger.Send(new ConsoleMessage($"{ServerType.Initiator}... {initiatorState}"));
                        break;
                }
            }
        }


        private void EchoHandlerThread(Object state)
        {
            while (!_disposed)
            {
                try
                {
                    _workingMres.Wait();
                    if (_echoClient == null || !_echoClient.Connected || _echoClient.ReceivedMessageQueue.IsEmpty)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    HandleEchoServerMessage();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
        }
        private void HandleEchoServerMessage()
        {
            if (_echoClient.ReceivedMessageQueue.TryDequeue(out NetworkMessage message))
            {
                switch (message.Type)
                {
                    case MessageType.ConnectionRequest:
                    case MessageType.KeepAlive:
                        break;

                    case MessageType.Number:
                        _initiatorClient.SendMessageQueue.Enqueue(message);
                        break;

                    case MessageType.State:
                        var initiatorState = (ServerState)BitConverter.ToInt32(message.Data, 0);
                        _messenger.Send(new ConsoleMessage($"{ServerType.Echo}... {initiatorState}"));
                        break;
                }
            }
        }


        // FUNCTIONS //////////////////////////////////////////////////////////////////////////////
        public void Start()
        {
            _workingMres.Set();
        }
        public void Stop()
        {
            _workingMres.Reset();
        }


        // IDisposable ////////////////////////////////////////////////////////////////////////////
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                _workingMres?.Dispose();
            }
        }
    }
}