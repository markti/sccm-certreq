using Microsoft.ServiceBus;
using SccmRelay.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SccmRelay.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost sh = new ServiceHost(typeof(CertificateGenerator));

            var localAddress = ConfigurationManager.AppSettings["LocalAddress"];
            var serviceNamespace = ConfigurationManager.AppSettings["ServiceNamespace"];
            var serviceKey = ConfigurationManager.AppSettings["ServiceKey"];
            var serviceKeyName = ConfigurationManager.AppSettings["ServiceKeyName"];
            var servicePath = ConfigurationManager.AppSettings["ServicePath"];

            sh.AddServiceEndpoint(typeof(ICertificateGenerator), new NetTcpBinding(), localAddress);

            sh.AddServiceEndpoint(
               typeof(ICertificateGenerator), new NetTcpRelayBinding(),
               ServiceBusEnvironment.CreateServiceUri("sb", serviceNamespace, servicePath))
                .Behaviors.Add(new TransportClientEndpointBehavior
                {
                    TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(serviceKeyName, serviceKey)
                });

            sh.Open();

            Console.WriteLine("Press ENTER to close");
            Console.ReadLine();

            sh.Close();
        }
    }
}