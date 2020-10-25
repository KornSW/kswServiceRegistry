//  +------------------------------------------------------------------------+
//  ¦ this file is part of an open-source solution which is originated here: ¦
//  ¦ https://github.com/KornSW/kswServiceRegistry                           ¦
//  ¦ the removal of this notice is prohibited by the author!                ¦
//  +------------------------------------------------------------------------+

namespace ServiceDiscovery.Store {

  public interface IDiscoveryStore {

    DiscoveryStoreEntry[] LoadAllEntries();
    bool TryLoadEntry(string contractFullName, int contractMajorVersion, string infrastructureScopeName, ref DiscoveryStoreEntry loadedEntry);
    void SaveEntry(DiscoveryStoreEntry entry);
    void RemoveEntry(string contractFullName, int contractMajorVersion, string infrastructureScopeName);

  }

}
