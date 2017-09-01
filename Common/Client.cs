using Common.Enums;
using Common.Extensions;
using Common.Models;
using Common.Models.Event;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Common
{
    public class Client : IDisposable
    {
        private readonly Dictionary<CommandType, Action<Byte[]>> _commandHandlerDictionary;
        private readonly TcpClient _client;
        private readonly ConcurrentQueue<Command> _sendCommandQueue;
        private readonly ConcurrentQueue<Command> _receiveCommandQueue;


        public Client(TcpClient client, ServerType type)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            Type = type;
            _commandHandlerDictionary = new Dictionary<CommandType, Action<Byte[]>>()
            {
                { CommandType.Message, MessageCommandHandler },
                { CommandType.WhoAreYou, WhoAreYouCommandHandler },
                { CommandType.State, StatusCommandHandler },
                { CommandType.HardBit, HardBitCommandHandler }
            };

            _sendCommandQueue = new ConcurrentQueue<Command>();
            _receiveCommandQueue = new ConcurrentQueue<Command>();

            ThreadPool.QueueUserWorkItem(NetworkReaderThread);
            ThreadPool.QueueUserWorkItem(NetworkWriterThread);
            ThreadPool.QueueUserWorkItem(CommandHandlerThread);
        }


        public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
        public event MessageReceivedEventHandler MessageReceived = (sender, args) => { };

        public delegate void CommunicationErrorEventHandler(object sender, CommunicationErrorEventArgs e);
        public event CommunicationErrorEventHandler CommunicationErrorOccurred = (sender, args) => { };

        public delegate void StateRequestReceivedEventHandler(object sender, StateMessageReceivedEventArgs e);
        public event StateRequestReceivedEventHandler StateMessageReceived = (sender, args) => { };


        // PROPERTIES /////////////////////////////////////////////////////////////////////////////
        public ServerType Type { get; }


        // THREADS ////////////////////////////////////////////////////////////////////////////////
        private void NetworkReaderThread(Object state)
        {
            try
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
            catch (Exception ex)
            {
                CommunicationErrorOccurred.Invoke(this, new CommunicationErrorEventArgs(ex));
            }
        }
        private void NetworkWriterThread(Object state)
        {
            try
            {
                using (var stream = _client.GetStream())
                {
                    while (true)
                    {
                        if (_sendCommandQueue.IsEmpty)
                            Thread.Sleep(10);

                        if (_sendCommandQueue.TryDequeue(out Command command))
                            stream.WriteObject(command);
                    }
                }
            }
            catch (Exception ex)
            {
                CommunicationErrorOccurred.Invoke(this, new CommunicationErrorEventArgs(ex));
            }
        }
        private void CommandHandlerThread(Object state)
        {
            try
            {
                while (true)
                {
                    if (_receiveCommandQueue.IsEmpty)
                        Thread.Sleep(10);

                    if (_receiveCommandQueue.TryDequeue(out Command command))
                        _commandHandlerDictionary[command.Type].Invoke(command.Data);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }


        // FUNCTIONS //////////////////////////////////////////////////////////////////////////////
        private void CheckRemoteSideAvaliability()
        {
            _sendCommandQueue.Enqueue(new Command()
            {
                Type = CommandType.HardBit,
                Data = BitConverter.GetBytes((Int32)HardBitDirection.Request)
            });
        }
        public void SendMessage(String message)
        {
            _sendCommandQueue.Enqueue(new Command()
            {
                Type = CommandType.Message,
                Data = Encoding.UTF8.GetBytes(message)
            });
        }
        public void SendMyState(ApplicationState state)
        {
            _sendCommandQueue.Enqueue(new Command()
            {
                Type = CommandType.State,
                Data = BitConverter.GetBytes((Int32)state)
            });
        }
        private void MessageCommandHandler(Byte[] commandData)
        {
            MessageReceived.Invoke(this, new MessageReceivedEventArgs()
            {
                Message = Encoding.UTF8.GetString(commandData)
            });
        }
        private void WhoAreYouCommandHandler(Byte[] commandData)
        {
            _sendCommandQueue.Enqueue(new Command()
            {
                Type = CommandType.WhoAreYou,
                Data = BitConverter.GetBytes((Int32)Type)
            });
        }
        private void StatusCommandHandler(Byte[] commandData)
        {
            StateMessageReceived.Invoke(this, new StateMessageReceivedEventArgs((ApplicationState)BitConverter.ToInt32(commandData, 0)));
        }
        private void HardBitCommandHandler(Byte[] commandData)
        {
            var directionInfo = (HardBitDirection)BitConverter.ToInt32(commandData, 0);
            if (directionInfo == HardBitDirection.Request)
            {
                _sendCommandQueue.Enqueue(new Command()
                {
                    Type = CommandType.HardBit,
                    Data = BitConverter.GetBytes((Int32)HardBitDirection.Responce)
                });
            }
        }


        // IDisposable ////////////////////////////////////////////////////////////////////////////
        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}