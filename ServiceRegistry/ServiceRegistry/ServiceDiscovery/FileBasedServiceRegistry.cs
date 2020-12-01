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
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDiscovery
{
  public class FileBasedServiceRegistry : IServiceRegistry {

    private string _SaltForLegitimationHash;
    private IDiscoveryStore _PersistentStore;
    private List<DiscoveryStoreEntry> _StoreEntries;

    public FileBasedServiceRegistry(IDiscoveryStore store, string saltForLegitimationHash) {
      _PersistentStore = store;
      _SaltForLegitimationHash = saltForLegitimationHash;
      _StoreEntries = _PersistentStore.LoadAllEntries().ToList();
    }

    private void SaveStoreEntry(DiscoveryStoreEntry entry) {
      _PersistentStore.SaveEntry(entry);
    }

    public bool AnnounceServiceUrl(string serviceUrl, string contractFullName, int contractMajorVersion, string infrastructureScopeName, int initialAvailabilityState, DateTime timestampUtc, string legitimationHash) {
      this.ThrowOnInvalidHash(serviceUrl, timestampUtc, legitimationHash);

      DiscoveryStoreEntry entry = this.PickStoreEntry(contractFullName, contractMajorVersion, infrastructureScopeName, true);

      entry.LastUpdateUtc = DateTime.UtcNow;

      DiscoveryStoreUrlEntry urlEntry = entry.UrlEntries.Where((e) => e.Url.Equals(serviceUrl, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();

      if ( urlEntry is null) {
        urlEntry = new DiscoveryStoreUrlEntry();
        urlEntry.Url = serviceUrl;
        urlEntry.AvailabilityState = initialAvailabilityState;
        entry.UrlEntries = entry.UrlEntries.Union(new DiscoveryStoreUrlEntry[] { urlEntry }).ToArray();
      } else {
        urlEntry.AvailabilityState = initialAvailabilityState;
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
          entry.UrlEntries = new DiscoveryStoreUrlEntry[] { };
          _StoreEntries.Add(entry);
        }
      }

      return entry;
    }

    public string[] GetServiceUrls(string contractFullName, int contractMajorVersion, string infrastructureScopeName) {
     
      DiscoveryStoreEntry entry = this.PickStoreEntry(contractFullName, contractMajorVersion, infrastructureScopeName, false);
      string[] urls;

      if (entry is null) {
       
        if (infrastructureScopeName.Contains("/")) {
          string[] tokens = infrastructureScopeName.Split('/');
          tokens = tokens.Take(tokens.Length - 1).ToArray();
          infrastructureScopeName = String.Join("/", tokens);

          urls = this.GetServiceUrls(contractFullName, contractMajorVersion, infrastructureScopeName);

        }
        else {
          urls = new string[] {};
        }
          
      }
      else {
        urls = entry.UrlEntries.Where((u) => u.AvailabilityState >= 0).OrderBy((u) => u.AvailabilityState).Select((u) => u.Url).ToArray();
      }

      return urls;
    }

    public bool UpdateAvailabilityState(string serviceUrl, int newAvailabilityState, DateTime timestampUtc, string legitimationHash) {
      this.ThrowOnInvalidHash(serviceUrl, timestampUtc, legitimationHash);

      lock (_StoreEntries) {

        foreach (DiscoveryStoreEntry entry in _StoreEntries) {
          DiscoveryStoreUrlEntry[] entriesOnUrl;

          entriesOnUrl = entry.UrlEntries.Where((u) => u.Url.Equals(serviceUrl, StringComparison.InvariantCultureIgnoreCase)).ToArray();

          bool found = false;
          foreach (DiscoveryStoreUrlEntry urlEntry in entriesOnUrl) {
            found = true;
            urlEntry.AvailabilityState = newAvailabilityState;
          }

          if (found) {
            entry.LastUpdateUtc = DateTime.UtcNow;
            try {
              this.SaveStoreEntry(entry);
            }
            catch {
              return false;
            }
          }

        }

      }

      return true;

    }

    private System.Security.Cryptography.MD5 _Md5 = System.Security.Cryptography.MD5.Create();
    private void ThrowOnInvalidHash(string serviceUrl, DateTime timestampUtc, string legitimationHash) {

      string phrase = timestampUtc.ToString("u") + _SaltForLegitimationHash + serviceUrl;
      string validHash = Encoding.Default.GetString(_Md5.ComputeHash(Encoding.Default.GetBytes(phrase)));

      if (!legitimationHash.Equals(validHash)) {
        throw new SecurityException("Invalid legitimationHash!");
      }
    }

  }

}
