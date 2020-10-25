//  +------------------------------------------------------------------------+
//  ¦ this file is part of an open-source solution which is originated here: ¦
//  ¦ https://github.com/KornSW/kswServiceRegistry                           ¦
//  ¦ the removal of this notice is prohibited by the author!                ¦
//  +------------------------------------------------------------------------+

using ServiceDiscovery;
using ServiceDiscovery.Cache;
using System;
using System.IO;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

namespace Demo {

  class Program {

    static void Main(string[] args) {

      //wait for services to start
      System.Threading.Thread.Sleep(1500);

      //setup service registry connection and cache
      string appDataFolderFullName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      string cacheFolderFullName = Path.Combine(appDataFolderFullName, "kswServiceRegistry", "Cache", "localhost");
      IServiceRegistry serviceRegistry = new ServiceRegistryWcfConnector("http://localhost:65453/v1/ServiceRegistry.svc");
      serviceRegistry = new ServiceRegistryCacheAdapter(serviceRegistry, new FileBasedDiscoveryCache(cacheFolderFullName), 2);

      //should be executed only once, because the registry has a persistent store!!!
      if (serviceRegistry.GetServiceUrls("Ksw.ServiceDiscoveryDemo", 1, "default").Length == 0) {
        serviceRegistry.AnnounceServiceUrl("http://localhost:65479/v1/DemoService.svc","Ksw.ServiceDiscoveryDemo", 1, "default", 0, DateTime.UtcNow, "HASH(utcnow+secret)");
        serviceRegistry.AnnounceServiceUrl("http://localhost:65480/v1/DemoService.svc","Ksw.ServiceDiscoveryDemo", 1, "default", 0, DateTime.UtcNow, "HASH(utcnow+secret)");
        serviceRegistry.AnnounceServiceUrl("ttp://localhost:65480/v2/DemoService.svc","Ksw.ServiceDiscoveryDemo", 2, "default", 0, DateTime.UtcNow, "HASH(utcnow+secret)");
      }

      V1.IDemoService v1AnywhereService = new DemoServiceWcfConnector(serviceRegistry.GetServiceUrls, "default");

      //V1.IDemoService v1S1Service = InitializeClient<V1.IDemoService>("http://localhost:65479/v1/DemoService.svc");
      //V1.IDemoService v1S2Service = InitializeClient<V1.IDemoService>("http://localhost:65480/v1/DemoService.svc");
      //V2.IDemoService v2S2Service = InitializeClient<V2.IDemoService>("http://localhost:65480/v2/DemoService.svc");
      //V1.IDemoService v1S1Service = new DemoServiceWcfConnector("http://localhost:65479/v1/DemoService.svc");
      //V1.IDemoService v1S2Service = new DemoServiceWcfConnector("http://localhost:65480/v1/DemoService.svc");

      string v1AnywhereResult = v1AnywhereService.Foo("Hello");
      //string v1S1Result = v1S1Service.Foo("Hello");
      //string v1S2Result = v1S2Service.Foo("Hello");
      //string v2S2Result = v2S2Service.Foo("Hello", 2);

      Console.WriteLine(v1AnywhereResult);
      //Console.WriteLine(v1S1Result);
      //Console.WriteLine(v1S2Result);
      //Console.WriteLine(v2S2Result);

      Console.ReadLine();
    }

    private static TContract InitializeClient<TContract>(string url) {

      EndpointAddress endPointAddress = new EndpointAddress(new Uri(url));

      WSHttpBinding wsHttpBinding = new WSHttpBinding();
      wsHttpBinding.SendTimeout = new TimeSpan(0, 5, 0);
      wsHttpBinding.MaxReceivedMessageSize = Int32.MaxValue;
      wsHttpBinding.BypassProxyOnLocal = true;
      //wsHttpBinding.TransactionFlow = true;
      //wsHttpBinding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;

      WebHttpBinding webHttpBinding = new WebHttpBinding();
      webHttpBinding.Security.Mode = WebHttpSecurityMode.None;
      webHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
      webHttpBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
      webHttpBinding.BypassProxyOnLocal = true;

      WebHttpBehavior webHttpBehavior = new WebHttpBehavior();
      webHttpBehavior.DefaultOutgoingRequestFormat = WebMessageFormat.Json;
      //behavior.DefaultBodyStyle = WebMessageBodyStyle.Bare;
      webHttpBehavior.DefaultBodyStyle = WebMessageBodyStyle.Wrapped;
      webHttpBehavior.FaultExceptionEnabled = true;

      //ChannelFactory<TContract> factory = new ChannelFactory<TContract>(wsHttpBinding, endPointAddress);
      ChannelFactory<TContract> factory = new ChannelFactory<TContract>(webHttpBinding, endPointAddress);
      factory.Endpoint.EndpointBehaviors.Add(webHttpBehavior);

      TContract instance = factory.CreateChannel();
      if (instance is IClientChannel) {
        IClientChannel cc = instance as IClientChannel;
        cc.OperationTimeout = TimeSpan.FromSeconds(60);
        //cc.Closed += this....
        //cc.Faulted += this....
      }

      return instance;
    }

  }

}
