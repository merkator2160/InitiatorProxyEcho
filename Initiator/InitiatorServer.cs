using Common;
using Common.Models.Enums;
using Common.Models.MvvmLight;
using Common.Models.Network;
using GalaSoft.MvvmLight.Messaging;
using Initiator.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Initiator
{
    internal class InitiatorServer : IDisposable
    {
        private const ServerType Type = ServerType.Initiator;

        private ServerState _serverState;
        private readonly NetworkConfig _config;
        private readonly IMessenger _messenger;
        private readonly NumberGenerator _numberGenerator;
        private readonly Dictionary<MessageType, Action<Byte[]>> _messageDictionary;
        private Boolean _disposed;

        private readonly ManualResetEventSlim _workingMres;
        private readonly ManualResetEventSlim _connectedMres;

        private static FileWriter _sendFileWriter;
        private static FileWriter _receiveFileWriter;

        private readonly ConnectionBuffer _bufferedClient;


        public InitiatorServer(NetworkConfig config, IMessenger messenger, NumberGenerator numberGenerator)
        {
            _config = config;
            _numberGenerator = numberGenerator;
            _bufferedClient = new ConnectionBuffer(Guid.NewGuid(), _config.NumberOfThreads);
            _messageDictionary = new Dictionary<MessageType, Action<Byte[]>>()
            {
                { MessageType.Number, HandleNumberMessage },
                { MessageType.KeepAlive, (data) => { } },
                { MessageType.ConnectionRequest, (data) => { } },
                { MessageType.State, (data) => { } },
            };

            _messenger = messenger;
            _messenger.Register<NumberGeneratedMessage>(this, OnNumberGenerated);
            _messenger.Register<StartCommandEnteredMessage>(this, OnStartCommandEntered);
            _messenger.Register<StopCommandEnteredMessage>(this, OnStopCommandEntered);
            _messenger.Register<ExitCommandEnteredMessage>(this, OnExitCommandEntered);

            _workingMres = new ManualResetEventSlim(false);
            _connectedMres = new ManualResetEventSlim(true);

            _sendFileWriter = new FileWriter("initiator_send.txt");
            _receiveFileWriter = new FileWriter("initiator_receive.txt");

            ThreadPool.QueueUserWorkItem(ConnectionControlThread);
            ThreadPool.QueueUserWorkItem(ReadingMessagesThread);

            _serverState = ServerState.Suspended;
            _messenger.Send(new ConsoleMessage($"{Type}... {_serverState}"));
        }


        // THREADS ////////////////////////////////////////////////////////////////////////////////
        private void ConnectionControlThread(Object state)
        {
            while (!_disposed)
            {
                if (_bufferedClient.Connected)
                {
                    Thread.Sleep(10);
                    continue;
                }

                if (TryConnect())
                {
                    _messenger.Send(new ConsoleMessage("Connected..."));
                    _connectedMres.Set();
                }
                else
                {
                    _messenger.Send(new ConsoleMessage("Proxy server unavalible. Retrying to connect."));
                    Thread.Sleep(_config.ReconnectDelay);
                }
            }
        }
        private Boolean TryConnect()
        {
            try
            {
                var tcpClient = new TcpClient(_config.ProxyHost, _config.ProxyPort);
                _bufferedClient.SetClient(tcpClient);
                _bufferedClient.SendMessageQueue.Enqueue(new NetworkMessage()
                {
                    Type = MessageType.ConnectionRequest,
                    Data = BitConverter.GetBytes((Int32)Type)
                });
                return true;
            }
            catch (Exception ex)
            {
                if (ex is SocketException || ex is IOException)
                {
                    return false;
                }

                Debug.WriteLine(ex.Message);
                throw;
            }
        }


        private void ReadingMessagesThread(Object state)
        {
            while (!_disposed)
            {
                try
                {
                    _connectedMres.Wait();
                    if (_bufferedClient.ReceivedMessageQueue.IsEmpty)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    if (_bufferedClient.ReceivedMessageQueue.TryDequeue(out NetworkMessage message))
                    {
                        _messageDictionary[message.Type].Invoke(message.Data);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is SocketException || ex is IOException)
                    {
                        _connectedMres.Reset();
                    }

                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
        }
        private void HandleNumberMessage(Byte[] data)
        {
            var number = BitConverter.ToInt64(data, 0);
            _receiveFileWriter.WriteLine(number.ToString());
        }


        // EVENTS /////////////////////////////////////////////////////////////////////////////////
        private void OnNumberGenerated(NumberGeneratedMessage message)
        {
            _sendFileWriter.WriteLine(message.Number.ToString());
            _bufferedClient.SendMessageQueue.Enqueue(new NetworkMessage()
            {
                Type = MessageType.Number,
                Data = BitConverter.GetBytes(message.Number)
            });
        }
        private void OnStartCommandEntered(StartCommandEnteredMessage message)
        {
            _workingMres.Set();
            _numberGenerator.Start();

            _serverState = ServerState.Working;
            _messenger.Send(new ConsoleMessage($"{Type}... {_serverState}"));
            SendServerStatus(_serverState);
        }
        private void OnStopCommandEntered(StopCommandEnteredMessage message)
        {
            _workingMres.Reset();
            _numberGenerator.Stop();

            _serverState = ServerState.Suspended;
            _messenger.Send(new ConsoleMessage($"{Type}... {_serverState}"));
            SendServerStatus(_serverState);
        }
        private void OnExitCommandEntered(ExitCommandEnteredMessage message)
        {
            _workingMres.Reset();
            _numberGenerator.Stop();

            _serverState = ServerState.Exited;
            _messenger.Send(new ConsoleMessage($"{Type}... {_serverState}"));
            SendServerStatus(_serverState);

            Thread.Sleep(1000);
            Environment.Exit(0);
        }


        // FUNCTIONS //////////////////////////////////////////////////////////////////////////////
        private void SendServerStatus(ServerState state)
        {
            if (_bufferedClient.Connected)
            {
                _bufferedClient.SendMessageQueue.Enqueue(new NetworkMessage()
                {
                    Type = MessageType.State,
                    Data = BitConverter.GetBytes((Int32)state)
                });
            }
        }


        // IDisposable ////////////////////////////////////////////////////////////////////////////
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                _bufferedClient?.Dispose();

                _workingMres?.Dispose();
                _connectedMres?.Dispose();
            }
        }
    }
}