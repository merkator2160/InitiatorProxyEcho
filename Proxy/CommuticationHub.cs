using Common;
using Common.Enums;
using Common.Extensions;
using Common.Models;
using Common.Models.Event;
using Proxy.Models;
using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace Proxy
{
    internal class CommuticationHub : IDisposable
    {
        private Boolean _disposed;
        private readonly ConnectionListener _connectionListener;
        private Client _initiatorClient;
        private Client _echoClient;


        public CommuticationHub(ConnectionListener communicationController)
        {
            _connectionListener = communicationController;
            _connectionListener.NewClientAvaliable += OnNewClientAvaliable;
        }
        ~CommuticationHub()
        {
            Dispose(false);
        }


        public event UserNotificationAvaliableEventHandler UserNotificationAvaliable = (sender, args) => { };


        // EVENTS /////////////////////////////////////////////////////////////////////////////////
        private void OnNewClientAvaliable(Object sender, NewClientAvaliableEventArgs newClientAvaliableEventArgs)
        {
            CheckWhoIsIt(newClientAvaliableEventArgs.Client);
        }
        private void InitiatorClientOnMessageReceived(Object sender, MessageReceivedEventArgs messageReceivedEventArgs)
        {
            _initiatorClient.SendMessage(messageReceivedEventArgs.Message);
        }
        private void InitiatorClientOnStateMessageReceived(Object sender, StateMessageReceivedEventArgs stateMessageReceivedEventArgs)
        {
            UserNotificationAvaliable.Invoke(this, new UserNotificationAvaliableEventArgs($"Initiator Server: {stateMessageReceivedEventArgs.State}"));
        }
        private void InitiatorClientOnCommunicationErrorOccurred(Object sender, CommunicationErrorEventArgs communicationErrorEventArgs)
        {
            _initiatorClient.CommunicationErrorOccurred -= InitiatorClientOnCommunicationErrorOccurred;
            _initiatorClient.MessageReceived -= InitiatorClientOnMessageReceived;
            _initiatorClient.StateMessageReceived -= InitiatorClientOnStateMessageReceived;

            _initiatorClient.Dispose();
            _initiatorClient = null;

            UserNotificationAvaliable.Invoke(this, new UserNotificationAvaliableEventArgs("Initiation Server connection is broken!"));
        }
        private void EchoClientOnMessageReceived(Object sender, MessageReceivedEventArgs messageReceivedEventArgs)
        {

        }
        private void EchoClientOnStateMessageReceived(Object o, StateMessageReceivedEventArgs stateMessageReceivedEventArgs)
        {
            UserNotificationAvaliable.Invoke(this, new UserNotificationAvaliableEventArgs($"Echo Server: {stateMessageReceivedEventArgs.State}"));
        }
        private void EchoClientOnCommunicationErrorOccurred(Object sender, CommunicationErrorEventArgs communicationErrorEventArgs)
        {
            _echoClient.CommunicationErrorOccurred -= EchoClientOnCommunicationErrorOccurred;
            _echoClient.MessageReceived -= EchoClientOnMessageReceived;
            _echoClient.StateMessageReceived -= EchoClientOnStateMessageReceived;

            _echoClient.Dispose();
            _echoClient = null;

            UserNotificationAvaliable.Invoke(this, new UserNotificationAvaliableEventArgs("Echo Server connection is broken!"));
        }


        // FUNCTIONS //////////////////////////////////////////////////////////////////////////////
        private void CheckWhoIsIt(TcpClient client)
        {
            try
            {
                using (var stream = client.GetStream())
                {
                    stream.WriteObject(new Command()
                    {
                        Type = CommandType.WhoAreYou
                    });

                    var responce = stream.ReadObject<Command>();
                    if (responce.Type != CommandType.WhoAreYou)
                        return;
                    if (responce.Data == null)
                        return;

                    var clientType = (ServerType)BitConverter.ToInt32(responce.Data, 0);
                    switch (clientType)
                    {
                        case ServerType.Initiator:
                            if (_initiatorClient != null)
                            {
                                _initiatorClient = new Client(client, ServerType.Proxy);
                                _initiatorClient.CommunicationErrorOccurred += InitiatorClientOnCommunicationErrorOccurred;
                                _initiatorClient.MessageReceived += InitiatorClientOnMessageReceived;
                                _initiatorClient.StateMessageReceived += InitiatorClientOnStateMessageReceived;
                            }
                            break;

                        case ServerType.Echo:
                            if (_echoClient != null)
                            {
                                _echoClient = new Client(client, ServerType.Proxy);
                                _echoClient.CommunicationErrorOccurred += EchoClientOnCommunicationErrorOccurred;
                                _echoClient.MessageReceived += EchoClientOnMessageReceived;
                                _echoClient.StateMessageReceived += EchoClientOnStateMessageReceived;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
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
            _connectionListener?.Dispose();
        }
    }
}