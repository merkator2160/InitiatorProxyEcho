using Common;
using Echo.Models;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.Unity;
using System;

namespace Echo
{
    class Program
    {
        static void Main(String[] args)
        {
            using (var container = ConfigureContainer(args))
            {
                var consoleWorker = container.Resolve<ConsoleWorker>();
                var echoServer = container.Resolve<EchoServer>();
                consoleWorker.Run();
            }
        }
        private static IUnityContainer ConfigureContainer(String[] args)
        {
            var container = new UnityContainer();
            container.RegisterType<IMessenger, Messenger>(new ContainerControlledLifetimeManager());
            container.RegisterInstance(CreateConfigFromCommandArgs(args));

            container.RegisterType<ConsoleWorker>();
            container.RegisterType<EchoServer>();

            return container;
        }
        private static NetworkConfig CreateConfigFromCommandArgs(String[] args)
        {
            var proxyAddress = args.Length > 0 ? args[0] : "127.0.0.1";
            var proxyPort = args.Length > 1 ? Int32.Parse(args[1]) : 8888;
            var numberOfThreads = args.Length > 1 ? Int32.Parse(args[2]) : 1;
            if (numberOfThreads == 0 || numberOfThreads > 3)
                throw new ArgumentException($"{nameof(numberOfThreads)} mast be above 0 but less than 3");

            return new NetworkConfig()
            {
                ProxyHost = proxyAddress,
                ProxyPort = proxyPort,
                NumberOfThreads = numberOfThreads,
                ReconnectDelay = 3000
            };
        }
    }
}