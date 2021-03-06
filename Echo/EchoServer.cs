﻿using Common;
using Common.Models.Enums;
using Common.Models.MvvmLight;
using Common.Models.Network;
using Echo.Models;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Echo
{
    internal class EchoServer
    {
        private const ServerType Type = ServerType.Echo;

        private ServerState _serverState;
        private readonly NetworkConfig _config;
        private readonly IMessenger _messenger;
        private readonly Dictionary<MessageType, Action<Byte[]>> _messageDictionary;
        private Boolean _disposed;

        private readonly ManualResetEventSlim _connectedMres;
        private static FileWriter _echoSendFileWriter;
        private readonly ConnectionBuffer _bufferedClient;


        public EchoServer(NetworkConfig config, IMessenger messenger)
        {
            _config = config;
            _messenger = messenger;
            _bufferedClient = new ConnectionBuffer(Guid.NewGuid(), _config.NumberOfThreads);
            _messageDictionary = new Dictionary<MessageType, Action<Byte[]>>()
            {
                { MessageType.Number, HandleNumberMessage },
                { MessageType.KeepAlive, (data) => { } },
                { MessageType.ConnectionRequest, (data) => { } },
                { MessageType.State, (data) => { } },
            };

            _messenger = messenger;
            _messenger.Register<StartCommandEnteredMessage>(this, OnStartCommandEntered);
            _messenger.Register<StopCommandEnteredMessage>(this, OnStopCommandEntered);
            _messenger.Register<ExitCommandEnteredMessage>(this, OnExitCommandEntered);

            _connectedMres = new ManualResetEventSlim(true);

            _echoSendFileWriter = new FileWriter("echo_send.txt");

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
            _bufferedClient.SendMessageQueue.Enqueue(new NetworkMessage()
            {
                Type = MessageType.Number,
                Data = data
            });

            var number = BitConverter.ToInt64(data, 0);
            _echoSendFileWriter.WriteLine(number.ToString());
        }


        // EVENTS /////////////////////////////////////////////////////////////////////////////////
        private void OnStartCommandEntered(StartCommandEnteredMessage message)
        {
            _serverState = ServerState.Working;
            _messenger.Send(new ConsoleMessage($"{Type}... {_serverState}"));
            SendServerStatus(_serverState);
        }
        private void OnStopCommandEntered(StopCommandEnteredMessage message)
        {
            _serverState = ServerState.Suspended;
            _messenger.Send(new ConsoleMessage($"{Type}... {_serverState}"));
            SendServerStatus(_serverState);
        }
        private void OnExitCommandEntered(ExitCommandEnteredMessage message)
        {
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

                _connectedMres?.Dispose();
            }
        }
    }
}