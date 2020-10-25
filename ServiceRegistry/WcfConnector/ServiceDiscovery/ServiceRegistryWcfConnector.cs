//  +------------------------------------------------------------------------+
//  ¦ this file is part of an open-source solution which is originated here: ¦
//  ¦ https://github.com/KornSW/kswServiceRegistry                           ¦
//  ¦ the removal of this notice is prohibited by the author!                ¦
//  +------------------------------------------------------------------------+

using System;

namespace ServiceDiscovery {

  public class ServiceRegistryWcfConnector : RobustWcfConnectorBase<IServiceRegistry>, IServiceRegistry {

    private const string _ContractFullName = "Ksw.ServiceRegistry";
    private const int _ContractMajorVersion = 1;

    /// <summary>
    /// WARNING: redirecting the 'urlDiscoveryMethod' to any artifact which is using this connector instance to discover the urls will cause a endless-loop!
    /// You should use a static configuration to specify the urls of the ServiceRegistry!
    /// </summary>
    /// <param name="urlDiscoveryMethod"></param>
    /// <param name="infrastructureScopeName"></param>
    public ServiceRegistryWcfConnector(UrlDiscoveryMethod urlDiscoveryMethod, string infrastructureScopeName) :
      base(_ContractFullName, _ContractMajorVersion, urlDiscoveryMethod, infrastructureScopeName) {
    }

    public ServiceRegistryWcfConnector(string wcfServiceUrl) :
      base(_ContractFullName, _ContractMajorVersion, wcfServiceUrl) {
    }

    public bool AnnounceServiceUrl(string serviceUrl, string contractFullName, int contractMajorVersion, string infrastructureScopeName, int initialLoadMetric, DateTime timestampUtc, string legitimationHash) {
      return this.InvokeProtected((svc) => svc.AnnounceServiceUrl(serviceUrl, contractFullName, contractMajorVersion, infrastructureScopeName, initialLoadMetric, timestampUtc, legitimationHash));
    }

    public string[] GetServiceUrls(string contractFullName, int contractMajorVersion, string infrastructureScopeName) {
      return this.InvokeProtected((svc) => svc.GetServiceUrls(contractFullName, contractMajorVersion, infrastructureScopeName));
    }

    public bool UpdateAvailabilityState(string serviceUrl, int newAvailabilityState, DateTime timestampUtc, string legitimationHash) {
      return this.InvokeProtected((svc) => svc.UpdateAvailabilityState(serviceUrl, newAvailabilityState, timestampUtc, legitimationHash));
    }

  }

}
