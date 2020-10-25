//  +------------------------------------------------------------------------+
//  ¦ this file is part of an open-source solution which is originated here: ¦
//  ¦ https://github.com/KornSW/kswServiceRegistry                           ¦
//  ¦ the removal of this notice is prohibited by the author!                ¦
//  +------------------------------------------------------------------------+

using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

namespace ServiceDiscovery {

  public class RobustWcfConnectorBase<TContract>: RobustConnectorBase<TContract> {

    private string _ContractFullName = "";
    private int _ContractMajorVersion = 1;

    public RobustWcfConnectorBase(string contractFullName, int contractMajorVersion, UrlDiscoveryMethod urlDiscoveryMethod, string infrastructureScopeName) : base(urlDiscoveryMethod, infrastructureScopeName) {
      _ContractFullName = contractFullName;
      _ContractMajorVersion = contractMajorVersion;
    }

    public RobustWcfConnectorBase(string contractFullName, int contractMajorVersion, string wcfServiceUrl):base(wcfServiceUrl) {
      _ContractFullName = contractFullName;
      _ContractMajorVersion = contractMajorVersion;
    }

    protected override TContract InitializeClient(string url) {

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

    protected override string ContractFullName {
      get {
        return _ContractFullName;
      }
    }

    protected override int ContractMajorVersion {
      get {
        return _ContractMajorVersion;
      }
    }

  }

}
