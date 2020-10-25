//  +------------------------------------------------------------------------+
//  ¦ this file is part of an open-source solution which is originated here: ¦
//  ¦ https://github.com/KornSW/kswServiceRegistry                           ¦
//  ¦ the removal of this notice is prohibited by the author!                ¦
//  +------------------------------------------------------------------------+

using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Services;

namespace ServiceDiscovery {

  public delegate string[] UrlDiscoveryMethod(string contractFullName, int contractMajorVersion, string infrastructureScopeName);

  [ServiceContract]
  public interface IServiceRegistry
  {
    [OperationContract, WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    bool AnnounceServiceUrl(string serviceUrl, string contractFullName, int contractMajorVersion, string infrastructureScopeName, int initialLoadMetric, DateTime timestampUtc, string legitimationHash);

    [OperationContract, WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    string[] GetServiceUrls(string contractFullName, int contractMajorVersion, string infrastructureScopeName);

    [OperationContract, WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    bool UpdateAvailabilityState(string serviceUrl, int newAvailabilityState, DateTime timestampUtc, string legitimationHash);

  }

}

