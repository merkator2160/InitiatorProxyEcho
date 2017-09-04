using Common;
using Common.Models.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.Unity;
using Proxy.Models;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Proxy
{
    class Program
    {
        static void Main(String[] args)
        {
            CheckForAnotherInstances();
            var container = ConfigureContainer(args);
            var consoleWorker = container.Resolve<ConsoleWorker>();
            var proxyServer = container.Resolve<ProxyServer>();
            consoleWorker.Run();

            container.Dispose();
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
        private static IUnityContainer ConfigureContainer(String[] args)
        {
            var container = new UnityContainer();
            container.RegisterType<IMessenger, Messenger>(new ContainerControlledLifetimeManager());
            container.RegisterInstance(CreateConfigFromCommandArgs(args));
            container.RegisterType<ConsoleWorker>();
            container.RegisterType<ProxyServer>();
            container.RegisterType<ConnectionListener>();

            return container;
        }
        private static NetworkConfig CreateConfigFromCommandArgs(String[] args)
        {
            var port = args.Length > 0 ? Int32.Parse(args[0]) : 8888;
            var numberOfThreads = args.Length > 1 ? Int32.Parse(args[1]) : 1;
            if (numberOfThreads == 0)
                throw new ArgumentException($"{nameof(numberOfThreads)} mast be above 0 but less than 3");

            return new NetworkConfig()
            {
                Port = port,
                NumberOfThreads = numberOfThreads
            };
        }
        private void PrintText(ConsoleMessage message)
        {
            Console.WriteLine(message.Text);
        }
    }
}
