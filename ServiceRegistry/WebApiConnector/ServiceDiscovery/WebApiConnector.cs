using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDiscovery
{
  public class WebApiConnector : IServiceRegistry
  {

    public bool AnnounceServiceUrl(string serviceUrl, int contractMajorVersion, string infrastructureScopeName, int initialLoadMetric, DateTime timestampUtc, string legitimationHash)
    {
      return false;
    }

    public string[] GetServiceUrls(string contractFullName, int contractMajorVersion, string infrastructureScopeName)
    {
      return new string[] { };
    }

    public bool UpdateAvailabilityState(string serviceUrl, int newAvailabilityState, DateTime timestampUtc, string legitimationHash)
    {
      return false;
    }

  }
}
