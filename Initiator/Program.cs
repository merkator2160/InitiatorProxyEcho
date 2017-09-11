using Common;
using GalaSoft.MvvmLight.Messaging;
using Initiator.Models;
using Microsoft.Practices.Unity;
using System;

namespace Initiator
{
    class Program
    {
        public static void Main(String[] args)
        {
            using (var container = ConfigureContainer(args))
            {
                var consoleWorker = container.Resolve<ConsoleWorker>();
                var initiatorServer = container.Resolve<InitiatorServer>();
                consoleWorker.Run();
            }
        }
        private static IUnityContainer ConfigureContainer(String[] args)
        {
            var container = new UnityContainer();
            container.RegisterType<IMessenger, Messenger>(new ContainerControlledLifetimeManager());
            container.RegisterInstance(CreateConfigFromCommandArgs(args));
            container.RegisterInstance(new NumberGeneratorConfig()
            {
                Delay = 500,
                Offset = 1000
            });
            container.RegisterType<ConsoleWorker>();
            container.RegisterType<InitiatorServer>();

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
