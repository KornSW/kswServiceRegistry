//  +------------------------------------------------------------------------+
//  ¦ this file is part of an open-source solution which is originated here: ¦
//  ¦ https://github.com/KornSW/kswServiceRegistry                           ¦
//  ¦ the removal of this notice is prohibited by the author!                ¦
//  +------------------------------------------------------------------------+

using ServiceDiscovery.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDiscovery {
  public class ServiceRegistryCacheAdapter : IServiceRegistry
  {

    private IServiceRegistry _PrimaryRegistry;
    private IDiscoveryCache _PersistentCache;
    private List<DiscoveryCacheEntry> _CacheEntries;
    private int _CacheRefreshIntervalMinutes;

    public ServiceRegistryCacheAdapter(IServiceRegistry primaryRegistry, IDiscoveryCache cache, int cacheRefreshIntervalMinutes) {
      _PrimaryRegistry = primaryRegistry;
      _PersistentCache = cache;
      try {
        _CacheEntries = _PersistentCache.LoadAllEntries().ToList();
      }
      catch {
        _CacheEntries = new List<DiscoveryCacheEntry>();
      }
      _CacheRefreshIntervalMinutes = cacheRefreshIntervalMinutes;
    }

    private void SaveCacheEntryFailSafe(DiscoveryCacheEntry entry) {
      Task.Run(() => {
        try {
          _PersistentCache.SaveEntry(entry);
        }
        catch {
        }
      });
    }

    public bool AnnounceServiceUrl(string serviceUrl, string contractFullName, int contractMajorVersion, string infrastructureScopeName, int initialLoadMetric, DateTime timestampUtc, string legitimationHash)
    {
      DiscoveryCacheEntry entry = this.PickCacheEntry(contractFullName, contractMajorVersion, infrastructureScopeName, true);

      //invalidate the cache to force a reload (because we dont know the sort-order)
      entry.LastUpdateUtc = DateTime.MinValue;

      //just for the case that the reload fails -> append as last entry  (but only if available when load >=0)
      if (initialLoadMetric >= 0 && !entry.UrlEntries.Contains(serviceUrl)) {
        entry.UrlEntries = entry.UrlEntries.Union(new string[] { serviceUrl }).ToArray();
      }

      this.SaveCacheEntryFailSafe(entry);

      return _PrimaryRegistry.AnnounceServiceUrl(serviceUrl, contractFullName, contractMajorVersion, infrastructureScopeName, initialLoadMetric, timestampUtc, legitimationHash);
    }

    private DiscoveryCacheEntry PickCacheEntry(string contractFullName, int contractMajorVersion, string infrastructureScopeName, bool createIfNotEists) {
      DiscoveryCacheEntry entry;

      lock (_CacheEntries) {
        entry = _CacheEntries.Where((e) => e.ContractFullName == contractFullName && e.ContractMajorVersion == contractMajorVersion && e.InfrastructureScopeName == infrastructureScopeName).SingleOrDefault();

        if (entry is null && createIfNotEists) {
          entry = new DiscoveryCacheEntry();
          entry.ContractFullName = contractFullName;
          entry.ContractMajorVersion = contractMajorVersion;
          entry.InfrastructureScopeName = infrastructureScopeName;
          entry.LastUpdateUtc = DateTime.MinValue;
          entry.UrlEntries = new string[] { };
          _CacheEntries.Add(entry);
        }
      }

      return entry;
    }

    public string[] GetServiceUrls(string contractFullName, int contractMajorVersion, string infrastructureScopeName)
    {
      DiscoveryCacheEntry entry = this.PickCacheEntry(contractFullName, contractMajorVersion, infrastructureScopeName, true);

      if (entry.LastUpdateUtc.AddMinutes(_CacheRefreshIntervalMinutes) < DateTime.UtcNow) {
        try {
          entry.UrlEntries = _PrimaryRegistry.GetServiceUrls(contractFullName, contractMajorVersion, infrastructureScopeName);        
        }
        catch {
          //cache should be used as long as registry is not available
        }
        //this will always be set to avoid retries
        entry.LastUpdateUtc = DateTime.UtcNow;
        this.SaveCacheEntryFailSafe(entry);
      }

      return entry.UrlEntries;
    }

    public bool UpdateAvailabilityState(string serviceUrl, int newAvailabilityState, DateTime timestampUtc, string legitimationHash)
    {
      DiscoveryCacheEntry[] entries;
    
      lock (_CacheEntries) {
        entries = _CacheEntries.Where((e) => e.UrlEntries.Contains(serviceUrl)).ToArray();
      }

      foreach (DiscoveryCacheEntry entry in entries) {
        //invalidate the cache to force a reload (because we dont know the sort-order)
        entry.LastUpdateUtc = DateTime.MinValue;
        this.SaveCacheEntryFailSafe(entry);
      }

      return _PrimaryRegistry.UpdateAvailabilityState(serviceUrl, newAvailabilityState, timestampUtc, legitimationHash);
    }

  }

}
