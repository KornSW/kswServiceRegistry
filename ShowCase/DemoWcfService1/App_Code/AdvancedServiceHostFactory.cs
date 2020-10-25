//  +------------------------------------------------------------------------+
//  ¦ this file is part of an open-source solution which is originated here: ¦
//  ¦ https://github.com/KornSW/kswServiceRegistry                           ¦
//  ¦ the removal of this notice is prohibited by the author!                ¦
//  +------------------------------------------------------------------------+

using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

  public class AdvancedServiceHostFactory : WebServiceHostFactory {

    public AdvancedServiceHostFactory() {
    }

    protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses) {

      string[] urlTokens = baseAddresses[0].AbsolutePath.Split('/');
      string version = urlTokens[1].ToUpper();

      Type[] contractInterfaces = serviceType.GetInterfaces().Where(i => i.GetCustomAttributes(true).Where(a => a.GetType() == typeof(System.ServiceModel.ServiceContractAttribute)).Any()).ToArray();
      Type contractType = contractInterfaces.Where(i => i.Namespace.ToUpper().Contains(version)).Single();

      ServiceHost host = base.CreateServiceHost(serviceType, baseAddresses);

      WSHttpBinding wsHttpBinding = new WSHttpBinding();
      wsHttpBinding.SendTimeout = new TimeSpan(0, 5, 0);
      wsHttpBinding.MaxReceivedMessageSize = Int32.MaxValue;
      wsHttpBinding.BypassProxyOnLocal = true;
      wsHttpBinding.Security.Mode = SecurityMode.None;
      //wsHttpBinding.TransactionFlow = true;
      //wsHttpBinding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;

      //ServiceEndpoint endpoint = host.AddServiceEndpoint(contractType, wsHttpBinding, "");

      WebHttpBinding webHttpBinding = new WebHttpBinding();
      webHttpBinding.Security.Mode = WebHttpSecurityMode.None;
      webHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
      webHttpBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
      webHttpBinding.BypassProxyOnLocal = true;

      ServiceEndpoint endpoint = host.AddServiceEndpoint(contractType, webHttpBinding, "");

      WebHttpBehavior webHttpBehavior = new WebHttpBehavior();
      webHttpBehavior.DefaultOutgoingRequestFormat = WebMessageFormat.Json;
      //behavior.DefaultBodyStyle = WebMessageBodyStyle.Bare;
      webHttpBehavior.DefaultBodyStyle = WebMessageBodyStyle.Wrapped;
      webHttpBehavior.FaultExceptionEnabled = true;

      endpoint.Behaviors.Add(webHttpBehavior);

      ServiceMetadataBehavior metadataBehaviour;
      if ((host.Description.Behaviors.Contains(typeof(ServiceMetadataBehavior)))) {
        metadataBehaviour = (ServiceMetadataBehavior)host.Description.Behaviors[typeof(ServiceMetadataBehavior)];
      }
      else {
        metadataBehaviour = new ServiceMetadataBehavior();
        host.Description.Behaviors.Add(metadataBehaviour);
      }
      metadataBehaviour.HttpGetEnabled = true;

      ServiceDebugBehavior debugBehaviour;
      if (host.Description.Behaviors.Contains(typeof(ServiceDebugBehavior))) {
        debugBehaviour = (ServiceDebugBehavior)host.Description.Behaviors[typeof(ServiceDebugBehavior)];
      }
      else {
        debugBehaviour = new ServiceDebugBehavior();
        host.Description.Behaviors.Add(debugBehaviour);
      }
      debugBehaviour.IncludeExceptionDetailInFaults = true;

      return host;
    }

}
