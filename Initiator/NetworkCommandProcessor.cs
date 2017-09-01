using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Common;
using Common.Enums;
using Common.Extensions;
using Common.Models;
using Common.Models.Event;
using Initiator.Models;

namespace Initiator
{
    internal class NetworkCommandProcessor : IDisposable
    {
        private Boolean _disposed;
        private readonly Dictionary<CommandType, Action<Byte[]>> _commandHandlerDictionary;

        private readonly TcpClient _client;
        private readonly ConcurrentQueue<Command> _sendCommandQueue;
        private readonly ConcurrentQueue<Command> _receiveCommandQueue;


        public NetworkCommandProcessor(String address, Int32 port)
        {
            _commandHandlerDictionary = new Dictionary<CommandType, Action<Byte[]>>()
            {
                { CommandType.Message, MessageCommandHandler },
                { CommandType.HardBit, HardBitCommandHandler }
            };
            _client = new TcpClient(address, port);
            _sendCommandQueue = new ConcurrentQueue<Command>();
            _receiveCommandQueue = new ConcurrentQueue<Command>();

            ThreadPool.QueueUserWorkItem(NetworkReaderThread);
            ThreadPool.QueueUserWorkItem(NetworkWriterThread);
            ThreadPool.QueueUserWorkItem(CommandHandlerThread);
        }
        ~NetworkCommandProcessor()
        {
            Dispose(false);
        }


        public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
        public event MessageReceivedEventHandler MessageReceived = (sender, args) => { };


        // THREADS ////////////////////////////////////////////////////////////////////////////////
        private void NetworkReaderThread(Object state)
        {
            while (true)
            {
                try
                {
                    if (_client != null && !_client.Connected)
                    {
                        using (var stream = _client.GetStream())
                        {
                            while (true)
                            {
                                if (!stream.DataAvailable)
                                    Thread.Sleep(10);

                                _receiveCommandQueue.Enqueue(stream.ReadObject<Command>());
                            }
                        }
                    }
                    Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
        }
        private void NetworkWriterThread(Object state)
        {
            while (true)
            {
                try
                {
                    if (_client != null && !_client.Connected)
                    {
                        using (var stream = _client.GetStream())
                        {
                            while (true)
                            {
                                if (_sendCommandQueue.IsEmpty)
                                    Thread.Sleep(10);

                                if (_sendCommandQueue.TryDequeue(out Command command))
                                {
                                    stream.WriteObject(command);
                                }
                            }
                        }
                    }
                    Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
        }
        private void CommandHandlerThread(Object state)
        {
            while (true)
            {
                try
                {
                    if (_client != null || !_client.Connected)
                    {
                        var stream = _client.GetStream();
                        while (true)
                        {
                            if (_receiveCommandQueue.IsEmpty)
                                Thread.Sleep(10);

                            if (_receiveCommandQueue.TryDequeue(out Command command))
                            {
                                _commandHandlerDictionary[command.Type].Invoke(command.Data);
                            }
                        }
                    }
                    Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
        }


        // FUNCTIONS //////////////////////////////////////////////////////////////////////////////
        public void SendMessage(String message)
        {
            _sendCommandQueue.Enqueue(new Command()
            {
                Type = CommandType.Message,
                Data = Encoding.ASCII.GetBytes(message)
            });
        }
        private void MessageCommandHandler(Byte[] commandData)
        {
            MessageReceived.Invoke(this, new MessageReceivedEventArgs()
            {
                Message = Encoding.UTF8.GetString(commandData)
            });
        }
        private void HardBitCommandHandler(Byte[] commandData)
        {
            _sendCommandQueue.Enqueue(new Command()
            {
                Type = CommandType.HardBit
            });
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
            _client?.Dispose();
        }
    }
}