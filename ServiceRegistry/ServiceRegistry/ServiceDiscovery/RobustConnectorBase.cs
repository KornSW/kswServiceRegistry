//  +------------------------------------------------------------------------+
//  ¦ this file is part of an open-source solution which is originated here: ¦
//  ¦ https://github.com/KornSW/kswServiceRegistry                           ¦
//  ¦ the removal of this notice is prohibited by the author!                ¦
//  +------------------------------------------------------------------------+

using System;
using System.Collections.Generic;
using System.Threading;

namespace ServiceDiscovery {

  abstract public class RobustConnectorBase<TContract> {

    private Dictionary<string, TContract> _ClientsPerUrl = new Dictionary<string, TContract>();
    private Dictionary<TContract, DateTime> _SuspendedTimesPerClient = new Dictionary<TContract, DateTime>();
    private Func<string[]> _UrlDiscoveryMethod;
    private TContract _PreferredClient;

    public RobustConnectorBase(UrlDiscoveryMethod urlDiscoveryMethod, string infrastructureScopeName) {
      _UrlDiscoveryMethod = () => urlDiscoveryMethod.Invoke(this.ContractFullName, this.ContractMajorVersion, infrastructureScopeName);
    }

    public RobustConnectorBase(string serviceUrl) {
      _UrlDiscoveryMethod = () => new string[] { serviceUrl };
    }

    protected abstract string ContractFullName { get; }
    protected abstract int ContractMajorVersion { get; }
    protected abstract TContract InitializeClient(string url);

    private TContract GetOrInitializeClient(string url) {
      lock (_ClientsPerUrl) {
        if (_ClientsPerUrl.ContainsKey(url)) {
          return _ClientsPerUrl[url];
        }
        else {
          TContract newClient = this.InitializeClient(url);
          _ClientsPerUrl.Add(url, newClient);
          return newClient;
        }
      }
    }

    protected void InvokeProtected(Action<TContract> action) {
      this.InvokeProtected<object>((svc) => {
        action.Invoke(svc);
        return null;
      });
    }

    protected result InvokeProtected<result>(Func<TContract, result> action) {

      if (!object.ReferenceEquals(_PreferredClient, null)) {
        try {
          result r = this.InvokeProtected(action, _PreferredClient);
          return r;
        }
        catch {
        }
      }

      string[] availableUrls = _UrlDiscoveryMethod.Invoke();
      if (availableUrls.Length < 1) {
        throw new Exception("no service urls available");
      }

      for (int i = 0; i < availableUrls.Length; i++) {
        string availableUrl = availableUrls[i];
        TContract client = this.GetOrInitializeClient(availableUrls[i]);
        DateTime suspendedTime = DateTime.MinValue;
        if (!_SuspendedTimesPerClient.TryGetValue(client, out suspendedTime) || suspendedTime < DateTime.Now) {
          try {
            result r = this.InvokeProtected(action, client);
            _PreferredClient = client;
            return r;
          }
          catch {
            if (i == availableUrls.Length - 1) {
              throw;
            }
          }
        }
      }

      throw new Exception("no service available");
    }

    private result InvokeProtected<result>(Func<TContract, result> action, TContract clientToUse) {

      //wait 500/1000/1500/2000 ms
      const int maxTryCount = 4;
      const int retryIntervalMs = 500;

      for (int i = 1; i <= maxTryCount; i++) {
        try {
          return action.Invoke(clientToUse);
        }
        catch (TimeoutException ex) {
          if (i >= maxTryCount) {
            _SuspendedTimesPerClient[clientToUse] = DateTime.Now.AddSeconds(20000);
            throw;
          }
          else {
            Thread.Sleep(i * retryIntervalMs);
          }
        }
      }

      throw new Exception("maxTryCount must be > 0!!!");
    }

  }

}

