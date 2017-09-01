using Common;
using Common.Enums;
using Common.Models.Event;
using Initiator.Models;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace Initiator
{
    class Program
    {
        private const ServerType Type = ServerType.Initiator;

        private static String _proxyAddress;
        private static Int32 _proxyPort;
        private static Int32 _threadsCount;

        private static FileWriter _sendFileWriter;
        private static FileWriter _receiveFileWriter;

        private static ConsoleCommandReader _commandReader;
        private static NumberGenerator _numberGenerator;
        private static Client _client;


        static void Main(string[] args)
        {
            Initialyze(args);

            Console.WriteLine($"{Type}... {_commandReader.ApplicationState}");
            _commandReader.Run();

            Dispose();
        }


        // THREADS ////////////////////////////////////////////////////////////////////////////////
        private static void ConnectionControlThread(Object state)
        {
            while (true)
            {
                try
                {
                    while (true)
                    {
                        if (_client != null)
                        {
                            Thread.Sleep(3000);
                            continue;
                        }

                        var tcpCLient = new TcpClient(_proxyAddress, _proxyPort);
                        _client = new Client(tcpCLient, Type);
                        _client.MessageReceived += OnMessageReceived;
                        _client.CommunicationErrorOccurred += OnCommunicationErrorOccurred;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    throw;
                }
            }
        }


        // EVENTS /////////////////////////////////////////////////////////////////////////////////
        private static void OnNumberGenerated(Object sender, NumberGeneratedEventArgs numberGeneratedEventArgs)
        {
            _sendFileWriter.WriteLine(numberGeneratedEventArgs.Number.ToString());
            _client.SendMessage(numberGeneratedEventArgs.Number.ToString());
        }
        private static void OnStartCommandEntered(Object sender, StartCommandEventArgs startCommandEventArgs)
        {
            ThreadPool.QueueUserWorkItem(ConnectionControlThread);
            _client.SendMyState(startCommandEventArgs.State);
            _numberGenerator.Start();
        }
        private static void OnStopCommandEntered(Object sender, StopCommandEventArgs stopCommandEventArgs)
        {
            _numberGenerator.Stop();
            _client.SendMyState(stopCommandEventArgs.State);
            Console.WriteLine(stopCommandEventArgs.Message);
        }
        private static void OnMessageReceived(Object sender, MessageReceivedEventArgs messageReceivedEventArgs)
        {
            _receiveFileWriter.WriteLine(messageReceivedEventArgs.Message);

            Console.WriteLine(messageReceivedEventArgs.Message);
        }
        private static void OnCommunicationErrorOccurred(Object sender, CommunicationErrorEventArgs communicationErrorEventArgs)
        {
            _client.MessageReceived -= OnMessageReceived;
            _client.CommunicationErrorOccurred -= OnCommunicationErrorOccurred;
            _client.Dispose();
            _client = null;
        }


        // FUNCTIONS //////////////////////////////////////////////////////////////////////////////
        private static void Initialyze(String[] args)
        {
            _proxyAddress = args[0];
            _proxyPort = Int32.Parse(args[1]);
            _threadsCount = Int32.Parse(args[2]);

            _sendFileWriter = new FileWriter("initiator_send.txt");
            _receiveFileWriter = new FileWriter("initiator_receive.txt");

            _commandReader = new ConsoleCommandReader(Type);
            _commandReader.StartCommandEntered += OnStartCommandEntered;
            _commandReader.StopCommandEntered += OnStopCommandEntered;

            _numberGenerator = new NumberGenerator(1000, 500);
            _numberGenerator.NumberGenerated += OnNumberGenerated;
        }
        private static void Dispose()
        {
            _sendFileWriter?.Dispose();
            _receiveFileWriter?.Dispose();
            _numberGenerator?.Dispose();
            _client?.Dispose();
        }
    }
}
