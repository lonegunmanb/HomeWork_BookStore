using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookStore.V2.Interface;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime.Configuration;

namespace BookStore.V2.SiloHost
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain hostDomain = AppDomain.CreateDomain("OrleansHost", null,
            new AppDomainSetup()
            {
                AppDomainInitializer = InitSilo
            });

            GrainClient.Initialize(ClientConfiguration.LocalhostSilo(30000));
            while (true)
            {
                var orderClerk = GrainClient.GrainFactory.GetGrain<IOrderClerk>(1l);
                orderClerk.PlaceNewOrder(1l.AsImmutable(), 1l.AsImmutable(), 5.AsImmutable());
                Console.ReadLine();
            }
        }

        private static void InitSilo(string[] args)
        {
            var config = new ClusterConfiguration();
            config.LoadFromFile(@".\OrleansHost.xml");
            var host = new Orleans.Runtime.Host.SiloHost(Guid.NewGuid().ToString(), config);
            host.InitializeOrleansSilo();
            host.StartOrleansSilo();
        }
    }
}
