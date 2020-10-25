//  +------------------------------------------------------------------------+
//  ¦ this file is part of an open-source solution which is originated here: ¦
//  ¦ https://github.com/KornSW/kswServiceRegistry                           ¦
//  ¦ the removal of this notice is prohibited by the author!                ¦
//  +------------------------------------------------------------------------+

using ServiceDiscovery;

namespace Demo {

  public class DemoServiceWcfConnector : RobustWcfConnectorBase<V1.IDemoService>, V1.IDemoService {

    private const string _ContractFullName = "Ksw.ServiceDiscoveryDemo";
    private const int _ContractMajorVersion = 1;

    public DemoServiceWcfConnector(UrlDiscoveryMethod urlDiscoveryMethod, string infrastructureScopeName) :
      base(_ContractFullName, _ContractMajorVersion, urlDiscoveryMethod, infrastructureScopeName) {
    }

    public DemoServiceWcfConnector(string wcfServiceUrl) :
      base(_ContractFullName, _ContractMajorVersion, wcfServiceUrl) {
    }

    public string Foo(string arg) {
      return this.InvokeProtected((svc) => svc.Foo(arg));
    }

  }

}
