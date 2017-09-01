using Common;
using Common.Enums;
using Common.Models.Event;
using System;

namespace Echo
{
    class Program
    {
        private const ServerType Type = ServerType.Echo;

        private static String _proxyAddress;
        private static Int32 _proxyPort;
        private static Int32 _threadsCount;

        private static FileWriter _echoFileWriter;

        private static ConsoleCommandReader _commandReader;
        private static Client _client;


        static void Main(String[] args)
        {
            Initialyze(args);

            Console.WriteLine($"{Type}... {_commandReader.ApplicationState}");
            _commandReader.Run();

            Dispose();
        }


        // EVENTS /////////////////////////////////////////////////////////////////////////////////
        private static void OnStartCommandEntered(Object sender, StartCommandEventArgs startCommandEventArgs)
        {

        }


        // FUNCTIONS //////////////////////////////////////////////////////////////////////////////
        private static void Initialyze(String[] args)
        {
            _proxyAddress = args[0];
            _proxyPort = Int32.Parse(args[1]);
            _threadsCount = Int32.Parse(args[2]);

            _echoFileWriter = new FileWriter("echo_send.txt");

            _commandReader = new ConsoleCommandReader(Type);
            _commandReader.StartCommandEntered += OnStartCommandEntered;
        }
        private static void Dispose()
        {
            _echoFileWriter?.Dispose();
            _client?.Dispose();
        }
    }
}