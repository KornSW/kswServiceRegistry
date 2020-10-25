//  +------------------------------------------------------------------------+
//  ¦ this file is part of an open-source solution which is originated here: ¦
//  ¦ https://github.com/KornSW/kswServiceRegistry                           ¦
//  ¦ the removal of this notice is prohibited by the author!                ¦
//  +------------------------------------------------------------------------+

using ServiceDiscovery.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDiscovery
{
  public class FileBasedServiceRegistry : IServiceRegistry {

    private IDiscoveryStore _PersistentStore;
    private List<DiscoveryStoreEntry> _StoreEntries;

    public FileBasedServiceRegistry(IDiscoveryStore store) {
      _PersistentStore = store;
      _StoreEntries = _PersistentStore.LoadAllEntries().ToList();
    }

    private void SaveStoreEntry(DiscoveryStoreEntry entry) {
      _PersistentStore.SaveEntry(entry);
    }

    public bool AnnounceServiceUrl(string serviceUrl, string contractFullName, int contractMajorVersion, string infrastructureScopeName, int initialLoadMetric, DateTime timestampUtc, string legitimationHash) {
      DiscoveryStoreEntry entry = this.PickStoreEntry(contractFullName, contractMajorVersion, infrastructureScopeName, true);

      //invalidate the store to force a reload (because we dont know the sort-order)
      entry.LastUpdateUtc = DateTime.MinValue;

      //just for the case that the reload fails -> append as last entry  (but only if available when load >=0)
      if (initialLoadMetric >= 0 && !entry.UrlEntries.Contains(serviceUrl)) {
        entry.UrlEntries = entry.UrlEntries.Union(new string[] { serviceUrl }).ToArray();
      }

      try {
        this.SaveStoreEntry(entry);
      }
      catch {
        return false;
      }

      return true;
    }

    private DiscoveryStoreEntry PickStoreEntry(string contractFullName, int contractMajorVersion, string infrastructureScopeName, bool createIfNotEists) {
      DiscoveryStoreEntry entry;

      lock (_StoreEntries) {
        entry = _StoreEntries.Where((e) => e.ContractFullName == contractFullName && e.ContractMajorVersion == contractMajorVersion && e.InfrastructureScopeName == infrastructureScopeName).SingleOrDefault();

        if (entry is null && createIfNotEists) {
          entry = new DiscoveryStoreEntry();
          entry.ContractFullName = contractFullName;
          entry.ContractMajorVersion = contractMajorVersion;
          entry.InfrastructureScopeName = infrastructureScopeName;
          entry.LastUpdateUtc = DateTime.MinValue;
          entry.UrlEntries = new string[] { };
          _StoreEntries.Add(entry);
        }
      }

      return entry;
    }

    public string[] GetServiceUrls(string contractFullName, int contractMajorVersion, string infrastructureScopeName) {
      DiscoveryStoreEntry entry = this.PickStoreEntry(contractFullName, contractMajorVersion, infrastructureScopeName, true);

      if (entry.LastUpdateUtc.AddMinutes(_StoreRefreshIntervalMinutes) < DateTime.UtcNow) {
        try {
          entry.UrlEntries = _PrimaryRegistry.GetServiceUrls(contractFullName, contractMajorVersion, infrastructureScopeName);
        }
        catch {
          //store should be used as long as registry is not available
        }
        //this will always be set to avoid retries
        entry.LastUpdateUtc = DateTime.UtcNow;
        this.SaveStoreEntry(entry);
      }

      return entry.UrlEntries;
    }

    public bool UpdateAvailabilityState(string serviceUrl, int newAvailabilityState, DateTime timestampUtc, string legitimationHash) {
      DiscoveryStoreEntry[] entries;

      lock (_StoreEntries) {
        entries = _StoreEntries.Where((e) => e.UrlEntries.Contains(serviceUrl)).ToArray();
      }

      foreach (DiscoveryStoreEntry entry in entries) {
        //invalidate the store to force a reload (because we dont know the sort-order)
        entry.LastUpdateUtc = DateTime.MinValue;
        this.SaveStoreEntry(entry);
      }
      try {
        this.SaveStoreEntry(entry);
      }
      catch {
        return false;
      }

      return true;

     // return _PrimaryRegistry.UpdateAvailabilityState(serviceUrl, newAvailabilityState, timestampUtc, legitimationHash);
    }

  }

}

