using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Enums;
using Common.Models;
using Common.Models.Event;

namespace Proxy
{
    class Program
    {
        private const ServerType Type = ServerType.Proxy;

        private static Int32 _listeningPort;

        private static ConsoleCommandReader _commandReader;
        private static CommuticationHub _commuticationHub;


        static void Main(String[] args)
        {
            CheckForAnotherInstances();
            Initialyze(args);

            Console.WriteLine($"{Type}... {_commandReader.ApplicationState}");
            _commandReader.Run();

            Dispose();
        }


        // EVENTS /////////////////////////////////////////////////////////////////////////////////
        private static void OnStartCommandEntered(Object sender, StartCommandEventArgs stopCommandEventArgs)
        {
            _commuticationHub = new CommuticationHub(new ConnectionListener(_listeningPort));
            _commuticationHub.UserNotificationAvaliable += CommuticationHubOnUserNotificationAvaliable;

            Console.WriteLine(stopCommandEventArgs.Message);
        }
        private static void CommuticationHubOnUserNotificationAvaliable(Object sender, UserNotificationAvaliableEventArgs infoAvaliableEventArgs)
        {
            Console.WriteLine(infoAvaliableEventArgs.Message);
        }


        // SUPPORT FUNCTIONS //////////////////////////////////////////////////////////////////////
        private static void Initialyze(String[] args)
        {
            _listeningPort = Int32.Parse(args[0]);

            _commandReader = new ConsoleCommandReader(Type);
            _commandReader.StartCommandEntered += OnStartCommandEntered;
        }
        private static void Dispose()
        {
            _commuticationHub?.Dispose();
        }
        private static void CheckForAnotherInstances()
        {
            var guid = Marshal.GetTypeLibGuidForAssembly(Assembly.GetExecutingAssembly()).ToString();

            Boolean created;
            var mutexObj = new Mutex(true, guid, out created);
            if (!created)
            {
                Console.WriteLine("Application instance already exist");
                Thread.Sleep(3000);
                Environment.Exit(0);
            }
        }
    }
}
