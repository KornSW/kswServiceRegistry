//  +------------------------------------------------------------------------+
//  ¦ this file is part of an open-source solution which is originated here: ¦
//  ¦ https://github.com/KornSW/kswServiceRegistry                           ¦
//  ¦ the removal of this notice is prohibited by the author!                ¦
//  +------------------------------------------------------------------------+

namespace ServiceDiscovery.Cache {

  public interface IDiscoveryCache {

    DiscoveryCacheEntry[] LoadAllEntries();
    bool TryLoadEntry(string contractFullName, int contractMajorVersion, string infrastructureScopeName, ref DiscoveryCacheEntry loadedEntry);
    void SaveEntry(DiscoveryCacheEntry entry);
    void RemoveEntry(string contractFullName, int contractMajorVersion, string infrastructureScopeName);

  }

}
